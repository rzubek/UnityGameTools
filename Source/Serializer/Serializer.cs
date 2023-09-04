// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using SomaSim.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SomaSim.SION
{
    public class SerializationException : Exception
    {
        public SerializationException (string message) : base(message) { }
        public SerializationException (string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Specifies whether an enum will be serialized as its numeric value (default),
    /// or whether it will be serialized by name (lowercase, locale-insensitive)
    /// </summary>
    public enum EnumSerializationOption
    {
        SerializeAsNumber = 0,
        SerializeAsSimpleName
    }

    public class SerializationOptions
    {
        public static void DefaultSpuriousDataCallback (string key, Type type) =>
            throw new SerializationException("Deserializer found key in data but not in class, key=" + key + ", class=" + type);

        public static void DefaultUnknownTypeCallback (string typename) =>
            throw new SerializationException("Serializer could not find type information for " + typename);

        public static void DefaultSerializationException (string message, Exception innerException) =>
            throw new SerializationException(message, innerException);

        /// <summary>
        /// Determines how enums should be serialized. By default, they will be serialized using their numeric values.
        /// </summary>
        public EnumSerializationOption EnumSerialization = EnumSerializationOption.SerializeAsNumber;

        /// <summary>
        /// Special key used to embed type information in serialized hashtables
        /// </summary>
        public string SpecialTypeToken = "#type";

        /// <summary>
        /// If true (default), member fields which are holding default values will not be serialized,
        /// only those whose value is not equal to the default value (as determined by Object.Equal()).
        /// If false, all serializable fields will be serialized, including those holding default values.
        /// </summary>
        public bool SkipDefaultValuesDuringSerialization = true;

        /// <summary>
        /// This error callback gets called on generic serialization exceptions, so that 
        /// client code can intercept and add additional logging. If null, the exception
        /// will get ignored and data that caused the error will be treated as null.
        /// </summary>
        public Action<string, Exception> OnSerializationException = DefaultSerializationException;

        /// <summary>
        /// This callback will get called when spurious data is encountered during deserialization 
        /// (ie. a key in data that doesn't exist in the class/struct being deserialized into).
        /// If null, spurious data will be ignored without logging.
        /// </summary>
        public Action<string, Type> OnSpuriousDataCallback = DefaultSpuriousDataCallback;

        /// <summary>
        /// This callback will be called when attempting to deserialize a type that 
        /// is not found in the currently loaded assemblies. If null, it will be ignored.
        /// </summary>
        public Action<string> OnUnknownTypeCallback = DefaultUnknownTypeCallback;
    }

    /// <summary>
    /// Serializes typed structures into generic objects and back out into class instances.
    /// If serialized object's type can be inferred from context (from enclosing object)
    /// its type specification will be skipped, otherwise it will be serialized out.
    /// 
    /// This serializer will write out any public fields, and any properties
    /// that have both a public getter and a public setters.
    /// 
    /// Some caveats:
    /// - Structures must be trees (ie. no cycles). There is no cycle detection.
    /// - Serialized classes *must* have default constructors (without parameters)
    ///
    /// Some details:
    /// - Serialized values must be one of: bool, long/ulong, string, double, ArrayList, Hashtable
    /// - Other types get serialized as follows:
    ///   - Primitives of type boolean or null will be serialized verbatim
    ///   - Signed integers (sbyte, short, int, long) will be serialized as signed long
    ///   - Unsigned integers (byte, ushort, uint, ulong) will be serialized as unsigned long
    ///   - Floating point numbers (float and double) will be serialized as double
    ///   - Characters and strings will both be serialized as string
    ///   - Enum value will be serialized either as a long int value, or as string name (customizable)
    ///   - DateTime will be serialized as a ulong
    ///   - Dictionaries will be serialized as Hashtables
    ///   - Other enumerables will be serialized as ArrayList
    ///   - Other class or struct instances will be serialized as Hashtable, with optional type info
    /// </summary>
    public class Serializer
    {
        public struct NSDef
        {
            public string prefix;
            public bool isclass;
        }

        internal class Settings
        {
            public List<NSDef> ImplicitNamespaces = new List<NSDef>();
            public Dictionary<Type, Func<object>> CustomFactories = new Dictionary<Type, Func<object>>();

            public Dictionary<Type, Func<object, object>> CustomSerializers = new Dictionary<Type, Func<object, object>>();
            public Dictionary<Type, Func<object, object>> CustomDeserializers = new Dictionary<Type, Func<object, object>>();
        }

        public SerializationOptions Options = new SerializationOptions();

        internal Settings _Settings = new Settings();

        // these caches are per-instance because they're updated during de/serialize, 
        // so they're not really thread-safe
        private Dictionary<string, Type> _ExplicitlyNamedTypes = new Dictionary<string, Type>();
        private Dictionary<Type, Dictionary<MemberInfo, object>> _DefaultInstanceValues = new Dictionary<Type, Dictionary<MemberInfo, object>>();
        private Dictionary<Type, Dictionary<string, MemberInfo>> _TypeToAllMemberInfos = new Dictionary<Type, Dictionary<string, MemberInfo>>();
        private Dictionary<Type, Dictionary<string, object>> _TypeAndNameToEnum = new Dictionary<Type, Dictionary<string, object>>();

        public static Serializer CloneSerializer (Serializer instance) {
            var newinst = new Serializer();
            newinst.Options = instance.Options;
            newinst._Settings = instance._Settings;
            return newinst;
        }

        public T Clone<T> (T source) => Deserialize<T>(Serialize(source, true));

        #region Serialization

        public object Serialize (object source) => Serialize(source, false);

        public object Serialize (object source, bool specifyValueTypes) {

            if (source == null) { return source; }

            Type sourceType = source.GetType();

            object result =
                TrySerializeCustom(source, sourceType) ??
                TrySerializePrimitive(source, sourceType) ??
                TrySerializeUntypedCollection(source, sourceType) ??
                TrySerializeClassOrStruct(source, sourceType, specifyValueTypes);

            return result;
        }

        private object TrySerializeCustom (object source, Type sourceType) {
            if (_Settings.CustomSerializers.TryGetValue(sourceType, out Func<object, object> serializer)) {
                return serializer(source);
            }

            return null;
        }

        //
        // serialization of built-in types

        private object TrySerializePrimitive (object source, Type sourceType) {
            if (source is bool) { return source; }
            if (source is string) { return source; }
            if (source is char @char) { return @char.ToString(); }

            if (source is sbyte || source is short || source is int || source is long) {
                return Convert.ToInt64(source);
            }
            if (source is byte || source is ushort || source is uint || source is ulong) {
                return Convert.ToUInt64(source);
            }
            if (source is float || source is double) {
                return Convert.ToDouble(source);
            }

            if (source is DateTime time) {
                return time.ToBinary();
            }

            if (sourceType.IsEnum) {
                if (Options.EnumSerialization == EnumSerializationOption.SerializeAsSimpleName) {
                    return Enum.GetName(sourceType, source).ToLowerInvariant();
                } else {
                    return Convert.ToInt64(source);
                }
            }

            return null;
        }

        private object TrySerializeUntypedCollection (object source, Type sourceType) {

            // if this is a collection and is not generic (so an old school collection
            // like ArrayList or Hashtable) we need to emit type info for each element
            var shouldEmitTypes = source is ArrayList || source is Hashtable;

            if (sourceType.IsGenericType) {
                // but if it's a generic type, yet applied to an interface or an abstract class, emit as well
                shouldEmitTypes = IsGenericOverInterfaceOrAbstractTypes(sourceType);
            }

            if (source is IDictionary) {
                return SerializeDictionary((source as IDictionary), shouldEmitTypes);
            }
            if (source is IEnumerable) {
                return SerializeEnumerable((source as IEnumerable), shouldEmitTypes);
            }

            return null;
        }

        private bool IsGenericOverInterfaceOrAbstractTypes (Type t) {
            var generics = t.GetGenericArguments();
            foreach (var generic in generics) {
                if (generic.IsAbstract || generic.IsInterface || generic.IsGenericType) {
                    return true;
                }
            }
            return false;
        }

        private Hashtable SerializeDictionary (IDictionary source, bool specifyValueTypes) {
            Hashtable results = new Hashtable(source.Count);
            IDictionaryEnumerator en = source.GetEnumerator();
            while (en.MoveNext()) {
                DictionaryEntry entry = en.Entry;
                object key = Serialize(entry.Key, specifyValueTypes);
                object value = Serialize(entry.Value, specifyValueTypes);
                results[key] = value;
            }
            return results;
        }

        private ArrayList SerializeEnumerable (IEnumerable source, bool specifyValueTypes) {
            ArrayList results = new ArrayList();
            IEnumerator en = source.GetEnumerator();
            while (en.MoveNext()) {
                object value = Serialize(en.Current, specifyValueTypes);
                results.Add(value);
            }
            return results;
        }

        // serialize custom classes or structs

        private object TrySerializeClassOrStruct (object source, Type sourceType, bool specifyType) {
            Hashtable result = new Hashtable();
            if (specifyType) {
                result[Options.SpecialTypeToken] = GetObjectTypeNameUsingImplicits(source);
            }

            var fields = GetSerializableMemberInfos(sourceType);
            foreach (var entry in fields) {
                string fieldName = entry.Key;
                MemberInfo field = entry.Value;

                object fieldValue = TypeUtils.GetValue(source, field);
                bool serialize = true;

                if (Options.SkipDefaultValuesDuringSerialization) {
                    object defaultValue = GetDefaultInstanceValue(source, field);
                    serialize = (fieldValue != null) &&
                                !fieldValue.Equals(defaultValue);
                }

                if (serialize) {
                    var fieldType = TypeUtils.GetSerializableTypeOrNull(field);
                    var specifyFieldType = fieldType == null || fieldType.IsAbstract || fieldType.IsInterface;
                    object serializedFieldValue = Serialize(fieldValue, specifyFieldType);
                    result[fieldName] = serializedFieldValue;
                }
            }

            return result;
        }

        private object GetDefaultInstanceValue (object obj, MemberInfo field) {
            if (obj == null) {
                return null;
            }

            Type t = obj.GetType();
            Dictionary<MemberInfo, object> defaultValues = GetAllDefaultInstanceValues(t);

            if (defaultValues.TryGetValue(field, out object result)) {
                return result;
            } else {
                return null;
            }
        }

        private Dictionary<MemberInfo, object> GetAllDefaultInstanceValues (Type t) {
            if (!_DefaultInstanceValues.TryGetValue(t, out Dictionary<MemberInfo, object> result)) {
                _DefaultInstanceValues[t] = result = new Dictionary<MemberInfo, object>();
                var instance = CreateInstance(t, 0);
                var serializables = TypeUtils.GetSerializableMembers(t);
                foreach (var field in serializables) {
                    result[field] = TypeUtils.GetValue(instance, field);
                }
            }
            return result;
        }

        private object CreateInstance (Type type, int length = 0) {
            if (_Settings.CustomFactories.ContainsKey(type)) {
                return (_Settings.CustomFactories[type]).Invoke();
            }

            if (type.IsArray) {
                return Array.CreateInstance(type.GetElementType(), length);
            }

            return Activator.CreateInstance(type);
        }

        #endregion

        #region Deserialization

        public object Deserialize (object source) => Deserialize(source, null);

        public T Deserialize<T> (object source) => (T) Deserialize(source, typeof(T));

        public object Deserialize (object source, Type desiredType) {
            // reset type to "unknown" if it's insufficiently specific
            if (desiredType == typeof(object)) { desiredType = null; }

            if (source == null) { return source; }

            return
                TryDeserializeCustom(source, desiredType) ??
                TryDeserializePrimitive(source, desiredType) ??
                TryDeserializeComposite(source, desiredType);
        }

        private object TryDeserializeCustom (object source, Type desiredType) {
            if (desiredType != null &&
                _Settings.CustomDeserializers.TryGetValue(desiredType, out Func<object, object> deserializer)) {

                return deserializer(source);
            }
            return null;
        }

        // 
        // primitives

        private object TryDeserializePrimitive (object source, Type desiredType) {

            // if we have a specific primitive type, try that
            if (desiredType != null) {
                return TryDeserializePrimitiveAsType(source, desiredType);
            }

            // if it's a primitive and type is not specified, just return it as is
            if (source is ulong || source is long || source is string || source is bool || source is double) {
                return source;
            }

            return null; // not a valid primitive, try something else
        }

        private Dictionary<Type, Func<object, object>> PrimitiveConverters = new Dictionary<Type, Func<object, object>>() {
            { typeof(bool), (object obj) => { return Convert.ToBoolean(obj); } },
            { typeof(ulong), (object obj) => { return Convert.ToUInt64(obj); } },
            { typeof(long), (object obj) => { return Convert.ToInt64(obj); } },
            { typeof(uint), (object obj) => { return Convert.ToUInt32(obj); } },
            { typeof(int), (object obj) => { return Convert.ToInt32(obj); } },
            { typeof(ushort), (object obj) => { return Convert.ToUInt16(obj); } },
            { typeof(short), (object obj) => { return Convert.ToInt16(obj); } },
            { typeof(byte), (object obj) => { return Convert.ToByte(obj); } },
            { typeof(sbyte), (object obj) => { return Convert.ToSByte(obj); } },
            { typeof(double), (object obj) => { return Convert.ToDouble(obj); } },
            { typeof(float), (object obj) => { return Convert.ToSingle(obj); } },
            { typeof(char), (object obj) => { return Convert.ToChar(obj); } },
            { typeof(string), (object obj) => { return Convert.ToString(obj); } },
            { typeof(DateTime), (object obj) => { return DateTime.FromBinary(Convert.ToInt64(obj)); } },
        };

        private V TryGetValueOrNull<K, V> (Dictionary<K, V> dictionary, K key) where V : class =>
            dictionary.TryGetValue(key, out V value) ? value : null;

        private object TryDeserializePrimitiveAsType (object source, Type desiredType) {
            try {
                if (desiredType.IsEnum) {
                    return TryDeserializeEnum(source, desiredType);
                }

                var converter = TryGetValueOrNull(PrimitiveConverters, desiredType);
                return converter != null ? converter(source) : null;

            } catch (Exception e) {
                Options.OnSerializationException("Failed to deserialize primitive, desired type = " +
                    desiredType + ", actual type = " + source.GetType() + ", value = " + source, e);
                return null;
            }
        }

        private object TryDeserializeEnum (object source, Type desiredType) {
            if (source is string @string) {
                return TryParseAndCacheEnum(@string, desiredType);
            }

            if (source is long @long) {
                return Enum.ToObject(desiredType, @long);
            }

            if (source is ulong || source is double) { // just in case
                return Enum.ToObject(desiredType, Convert.ToInt64(source));
            }

            return null;
        }

        private object TryParseAndCacheEnum (string str, Type enumType) {
            if (str == null || enumType == null) {
                Options.OnSerializationException($"Missing enum source or type: {str}/{enumType}", null);
                return null;
            }

            // see if it's cached
            var result = _TypeAndNameToEnum.FindOrNull(enumType, str);
            if (result != null) { return result; }

            // try parsing. we remove any dashes first.
            try {
                string testValue = str.Contains('-') ? str.Replace("-", "") : str;
                result = Enum.Parse(enumType, testValue, true);
            } catch (Exception ex) {
                Options.OnSerializationException($"Cannot parse {str} as enum of type {enumType}, got parse exception", ex);
                return null;
            }
            if (result == null) {
                Options.OnSerializationException($"Cannot parse {str} as enum of type {enumType}", null);
                return null;
            }
            // finally cache and return the results
            _TypeAndNameToEnum.Add(enumType, str, result);
            return result;
        }

        //
        // non-primitives

        private object TryDeserializeComposite (object source, Type desiredType) {
            // if there's an explicit type in the object, use it
            if (desiredType == null || IsExplicitlyTypedHashtable(source)) {
                desiredType = InferType(source);
            }

            // we know the type, make an empty instance to deserialize into
            // int size = (source is ArrayList) ? ((ArrayList)source).Count : 0;
            // object instance = CreateInstance(desiredType, size);

            // are we deserializing a dictionary?
            if (typeof(IDictionary).IsAssignableFrom(desiredType)) {
                return TryDeserializeIntoDictionary(source, desiredType);

            } else if (typeof(IEnumerable).IsAssignableFrom(desiredType)) {
                return TryDeserializeIntoEnumerable(source, desiredType);

            } else {
                return TryDeserializeIntoClassOrStruct(source, desiredType);
            }
        }

        private object TryDeserializeIntoDictionary (object source, Type desiredType) {
            if (source is not Hashtable table) {
                Options.OnSerializationException("Deserializer found value where Hashtable expected: " + source, null);
                return null;
            }

            object instance = CreateInstance(desiredType);
            var addfn = desiredType.GetMethod("Add");

            // recursively deserialize all values
            var en = table.GetEnumerator();
            while (en.MoveNext()) {
                DictionaryEntry entry = (DictionaryEntry) en.Current;

                // do we need to convert values into some specific type?
                TryExtractDictionaryKeyValueTypes(desiredType, out Type keytype, out Type valuetype);

                try {
                    object key = Deserialize(entry.Key, keytype);
                    object value = Deserialize(entry.Value, valuetype);
                    addfn.Invoke(instance, new object[] { key, value });
                } catch (Exception e) {
                    Options.OnSerializationException("Failed to deserialize field named " + entry.Key, e);
                }
            }

            return instance;
        }

        private static void TryExtractDictionaryKeyValueTypes (Type desiredType, out Type keytype, out Type valuetype) {
            // this is a concrete dictionary
            if (!desiredType.IsGenericType) {
                // is this a concrete subclass like MyDict : Dictionary<K, V> ?
                // if so, go up until we hit generic arguments
                var superType = FindGenericTypeOrBase(desiredType.BaseType);
                if (superType != null) {
                    TryExtractDictionaryKeyValueTypes(superType, out keytype, out valuetype);
                    return;
                } else {
                    keytype = valuetype = null;
                    return;
                }
            }

            // try this type, see if it's Dictionary<K, V>
            var genargs = desiredType.GetGenericArguments();
            if (genargs.Length >= 2) {
                keytype = genargs[0];       // these are <K, V> in Dictionary<K, V>
                valuetype = genargs[1];
                return;
            }

            // it's a subclass, like MyDict<T> : Dictionary<K, T> so let's recurse until we hit the Dictionary<K, V>
            TryExtractDictionaryKeyValueTypes(desiredType.BaseType, out keytype, out valuetype);
        }

        private object TryDeserializeIntoEnumerable (object source, Type desiredType) {
            // recursively deserialize all values
            if (source is not ArrayList list) {
                Options.OnSerializationException("Deserializer found value where ArrayList expected: " + source, null);
                return null;
            }

            object instance = CreateInstance(desiredType, list.Count);
            if (instance == null) {
                return null;
            }

            Type valuetype = null;

            // do we need to convert values to a specific array type?
            Array array = instance as Array;
            if (array != null) {
                valuetype = array.GetType().GetElementType();
            }

            // do we need to convert values into some specific generic type?
            var genericType = FindGenericTypeOrBase(desiredType);
            if (genericType != null) {
                var args = genericType.GetGenericArguments();
                if (args.Length >= 1) {
                    valuetype = args[0]; // T in IList<T> etc
                }
            }

            for (int i = 0; i < list.Count; i++) {
                object element = list[i];
                object typedElement = Deserialize(element, valuetype);

                // now insert into the list or array as needed
                if (array != null) { // T[]
                    array.SetValue(typedElement, i);
                } else {             // List<T> or List or some such
                    desiredType.GetMethod("Add").Invoke(instance, new object[] { typedElement });
                }
            }

            return instance;
        }

        private static Type FindGenericTypeOrBase (Type type) {
            while (type != null) {
                if (type.IsGenericType) { return type; }
                type = type.BaseType;
            }
            return null;
        }

        private object TryDeserializeIntoClassOrStruct (object source, Type desiredType) {
            if (source is not Hashtable table) {
                Options.OnSerializationException("Deserializer can't populate class or struct from a non-hashtable: " + source, null);
                return null;
            }

            object instance = CreateInstance(desiredType);
            var membersToProcess = GetSerializableMemberInfos(desiredType);

            var en = table.GetEnumerator();
            while (en.MoveNext()) {
                DictionaryEntry entry = (DictionaryEntry) en.Current;
                string key = entry.Key as String; // classes/struct member names are always strings

                if (key == Options.SpecialTypeToken) {
                    // ignore it

                } else if (membersToProcess.ContainsKey(key)) {
                    MemberInfo member = membersToProcess[key];
                    object fieldval = Deserialize(entry.Value, TypeUtils.GetSerializableTypeOrNull(member));
                    if (member is PropertyInfo pinfo) { pinfo.SetValue(instance, fieldval, null); }
                    if (member is FieldInfo finfo) { finfo.SetValue(instance, fieldval); }

                } else {
                    Options.OnSpuriousDataCallback?.Invoke(key, desiredType);
                }
            }

            return instance;
        }


        // type helpers

        private bool IsExplicitlyTypedHashtable (object source) =>
            source is Hashtable table && table.ContainsKey(Options.SpecialTypeToken);

        private Type InferType (object value) {
            var table = value as Hashtable;

            // if it's a hashtable, see if we have the magical type marker
            if (IsExplicitlyTypedHashtable(value)) {
                // manufacture the type
                string typeName = table[Options.SpecialTypeToken] as string;
                Type explicitType = FindTypeByName(typeName);
                if (explicitType != null) {
                    return explicitType;
                }
            }

            if (table != null) {
                // it's a hashtable but either the type wasn't specified, or it's unknown
                return typeof(Hashtable);
            }

            if (value is ArrayList) {
                return typeof(ArrayList);
            }

            // it's a scalar, no type
            return null;
        }

        private Type FindTypeByName (string name, bool ignoreCase = false, bool cache = true) {
            // see if we need to convert from shorthand notation or not 
            if (name.Contains("-")) {
                name = TypeUtils.ConvertLispToPascalCase(name);
            }

            // do we have it cached?
            Type result = TryGetValueOrNull(_ExplicitlyNamedTypes, name);
            if (result != null) {
                return result;
            }

            // search for it the hard way and add to the cache as needed
            Type type = FindTypeIncludingImplicits(name, ignoreCase);
            if (type != null) {
                if (cache) {
                    _ExplicitlyNamedTypes[name] = type;
                }
                return type;
            }

            // signal error, and we're done
            Options.OnUnknownTypeCallback?.Invoke(name);

            return null;
        }

        private Type FindTypeIncludingImplicits (string name, bool ignoreCase) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // first try to find based on explicit name
            Type type = FindInAllAssemblies(name, ignoreCase, assemblies);
            if (type != null) {
                return type;
            }

            // otherwise try all implicit namespaces in order
            for (int i = 0, count = _Settings.ImplicitNamespaces.Count; i < count; i++) {
                var def = _Settings.ImplicitNamespaces[i];
                string longName = def.prefix + (def.isclass ? "+" : ".") + name;
                type = FindInAllAssemblies(longName, ignoreCase, assemblies);
                if (type != null) {
                    return type;
                }
            }

            return null;
        }

        private string GetObjectTypeNameUsingImplicits (object value) {
            string fullname = value.GetType().FullName;
            for (int i = 0, count = _Settings.ImplicitNamespaces.Count; i < count; i++) {
                string prefix = _Settings.ImplicitNamespaces[i].prefix;
                if (fullname.StartsWith(prefix)) {
                    return fullname[(prefix.Length + 1)..]; // + 1 for the extra "." or "+"
                }
            }
            return fullname;
        }

        private Type FindInAllAssemblies (string name, bool ignoreCase, Assembly[] assemblies) {
            foreach (var assembly in assemblies) {
                Type type = assembly.GetType(name, false, ignoreCase);
                if (type != null) {
                    return type;
                }
            }
            return null;
        }

        private Dictionary<string, MemberInfo> GetSerializableMemberInfos (Type t) {
            Dictionary<string, MemberInfo> result = TryGetValueOrNull(_TypeToAllMemberInfos, t);
            if (result != null) {
                return result;
            }

            result = new Dictionary<string, MemberInfo>();
            foreach (var info in TypeUtils.GetSerializableMembers(t)) {
                result[info.Name] = info;
            }

            _TypeToAllMemberInfos.Add(t, result);
            return result;
        }

        #endregion



        //
        //
        // IMPLICIT NAMESPACES

        public void AddImplicitNamespace (string prefix, bool isNamespace) =>
            _Settings.ImplicitNamespaces.Add(new NSDef() { prefix = prefix, isclass = !isNamespace });

        public void RemoveImplicitNamespace (string prefix) =>
            _Settings.ImplicitNamespaces.RemoveAll((NSDef def) => def.prefix == prefix);



        // 
        //
        // CUSTOM SERIALIZERS

        public void AddCustomSerializer<T> (Func<T, Serializer, object> serializer, Func<object, Serializer, T> deserializer) where T : new() {
            Type type = typeof(T);
            // we have to lobotomize type checking here, so sorry
            _Settings.CustomFactories[type] = () => { return new T(); };
            _Settings.CustomSerializers[type] = (object source) => { return serializer((T) source, this); };
            _Settings.CustomDeserializers[type] = (object value) => { return deserializer(value, this); };
        }

        public void AddCustomSerializer<T> (Func<T> factory, Func<T, object> serializer, Func<object, T> deserializer) {
            Type type = typeof(T);
            // we have to lobotomize type checking here, so sorry
            _Settings.CustomFactories[type] = () => { return factory(); };
            _Settings.CustomSerializers[type] = (object source) => { return serializer((T) source); };
            _Settings.CustomDeserializers[type] = (object value) => { return deserializer(value); };
        }

        public void RemoveCustomSerializer<T> () {
            Type type = typeof(T);
            _Settings.CustomFactories.Remove(type);
            _Settings.CustomSerializers.Remove(type);
            _Settings.CustomDeserializers.Remove(type);
        }




        /// <summary>
        /// Collection of various utilities for getting and manipulating reflected type information
        /// </summary>
        public static class TypeUtils
        {
            /// <summary>
            /// If the member is either a public variable, or a property with public getter and setter, 
            /// returns its type, otherwise returns null.
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public static Type GetSerializableTypeOrNull (MemberInfo i) {
                if (i is PropertyInfo) {
                    PropertyInfo pi = i as PropertyInfo;
                    return (pi.CanRead && pi.CanWrite) ? pi.PropertyType : null;
                } else if (i is FieldInfo) {
                    FieldInfo fi = i as FieldInfo;
                    return (fi.IsPublic && !fi.IsStatic) ? fi.FieldType : null;
                } else {
                    return null;
                }
            }

            /// <summary>
            /// Returns all serializable fields or properties from the given object
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static IEnumerable<MemberInfo> GetSerializableMembers (Type type) {
                MemberInfo[] members = type.GetMembers();
                return members.Where(member => GetSerializableTypeOrNull(member) != null);
            }


            /// <summary>
            /// If the member is either a variable or a property, returns its value
            /// </summary>
            /// <param name="i"></param>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static object GetValue (object obj, MemberInfo i) =>
                (i is PropertyInfo pinfo) ? pinfo.GetValue(obj, null) :
                (i is FieldInfo finfo) ? finfo.GetValue(obj) :
                null;

            /// <summary>
            /// Converts a Lisp-style name like "foo-bar-baz" into a Pascal case name like "FooBarBaz"
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static string ConvertLispToPascalCase (string name) {
                // split on dash, capitalize each segment, then squish back together
                string[] segments = name.Split('-');
                for (int i = 0; i < segments.Length; ++i) { segments[i] = UpcaseFirstLetter(segments[i]); }
                return string.Join("", segments);

                static string UpcaseFirstLetter (string str) =>
                    string.IsNullOrEmpty(str) ? "" :
                    (str.Length == 1) ? str.ToUpperInvariant() :
                    (char.ToUpperInvariant(str[0]) + str[1..]);
            }
        }
    }
}

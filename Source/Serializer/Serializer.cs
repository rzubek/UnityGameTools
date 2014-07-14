using SomaSim;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SomaSim.Serializer
{
    /// <summary>
    /// Serializes typed structures into JSON and back out into class instances.
    /// If serialized object's type can be inferred from context (from enclosing object)
    /// its type specification will be skipped, otherwise it will be serialized out.
    /// 
    /// This serializer will write out any public member variables, and any properties
    /// that have public getters and setters.
    /// 
    /// Some caveats:
    /// - Structures must be trees (ie. no cycles). 
    /// - Serialized classes *must* have default constructors (without parameters)
    /// </summary>
    public class Serializer : IService
    {
        public string TYPEKEY = "#type";

        /// <summary>
        /// If true, only values different from defaults will be written out
        /// during serialization. Produces much smaller files.
        /// </summary>
        public bool SkipDefaultsDuringSerialization = true;

        /// <summary>
        /// If true, when deserializing a strongly typed class, any key in JSON
        /// that doesn't correspond to a member field in the class instance will 
        /// cause an exception to be thrown.
        /// </summary>
        public bool ThrowErrorOnSpuriousData = true;

        /// <summary>
        /// If true, attempting to serialize null will cause an exception to be thrown.
        /// </summary>
        public bool ThrowErrorOnSerializingNull = false;

        /// <summary>
        /// In case we try to deserialize a scalar value into a field that expects a 
        /// class instance or a collection, if this is true an exception will be thrown,
        /// otherwise the unexpected value will be deserialized as null.
        /// </summary>
        public bool ThrowErrorOnUnexpectedCollections = true;

        private Dictionary<string, Type> _ExplicitlyNamedTypes;
        private Dictionary<Type, object> _DefaultInstances;

        public void Initialize()
        {
            this._ExplicitlyNamedTypes = new Dictionary<string, Type>();
            this._DefaultInstances = new Dictionary<Type,object>();
        }

        public void Release()
        {
            this._DefaultInstances.Clear();
            this._DefaultInstances = null;

            this._ExplicitlyNamedTypes.Clear();
            this._ExplicitlyNamedTypes = null;
        }


        //
        //
        // DESERIALIZE

        /// <summary>
        /// Deserializes a parsed value object, forcing it into an instance of given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Deserialize<T>(object value)
        {
            return (T) Deserialize(value, typeof(T));
        }

        /// <summary>
        /// Deserializes a parsed value object, allowing the user to supply type
        /// or trying to infer it from annotation inside the value object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targettype"></param>
        /// <returns></returns>
        public object Deserialize(object value, Type targettype = null)
        {
            // reset type to "unknown" if it's insufficiently specific
            if (targettype == typeof(object))
            {
                targettype = null;
            }

            // scalars get simply converted (if needed)
            if (IsScalar(value))
            {
                return (targettype != null) ? Convert.ChangeType(value, targettype) : value;
            }

            // try to infer type from the value if it's a dictionary with a #type key
            if (targettype == null)
            {
                targettype = InferType(value);
            }

            // we know the type, make an instance to deserialize into
            object instance = Activator.CreateInstance(targettype);

            // are we deserializing a dictionary?
            if (typeof(IDictionary).IsAssignableFrom(targettype)) 
            {
                Hashtable table = value as Hashtable;
                if (table == null)
                {
                    if (ThrowErrorOnUnexpectedCollections)
                    {
                        throw new Exception("Deserializer found value where Hashtable expected: " + value);
                    }
                    return null;
                }

                // recursively deserialize all values
                foreach (DictionaryEntry entry in table)
                {
                    // do we need to convert values into some specific type?
                    Type valuetype = null;
                    if (targettype.IsGenericType)
                    {
                        valuetype = targettype.GetGenericArguments()[1]; // T in Dictionary<S,T>
                    }
                    object typedEntryValue = Deserialize(entry.Value, valuetype);
                    targettype.GetMethod("Add").Invoke(instance, new object[] { entry.Key, typedEntryValue });
                }

            } 

            // are we deserializing a linear collection?
            else if (typeof(IEnumerable).IsAssignableFrom(targettype))
            {
                // recursively deserialize all values
                ArrayList list = value as ArrayList;
                if (list == null)
                {
                    if (ThrowErrorOnUnexpectedCollections)
                    {
                        throw new Exception("Deserializer found value where ArrayList expected: " + value);
                    }
                    return null;
                }

                foreach (object element in list)
                {
                    // do we need to convert values into some specific type?
                    Type valuetype = null;
                    if (targettype.IsGenericType)
                    {
                        valuetype = targettype.GetGenericArguments()[0]; // T in List<T>
                    }
                    object typedElement = Deserialize(element, valuetype);
                    targettype.GetMethod("Add").Invoke(instance, new object[] { typedElement });
                }
            }
            else
            {
                // class - deserialize each field recursively
                DeserializeIntoClassOrStruct(value, instance);
            }

            return instance;
        }

        private static Dictionary<string, MemberInfo> GetMemberInfos(Type t)
        {
            Dictionary<string, MemberInfo> results = new Dictionary<string, MemberInfo>();
            foreach (var info in TypeUtils.GetMembers(t))
            {
                results[info.Name] = info;
            }
            return results;
        }

        /// <summary>
        /// Helper function, deserializes a parsed object into a specific class instance,
        /// using each field's type in recursive deserialization.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        internal void DeserializeIntoClassOrStruct (object value, object target) {
            Type targetType = target.GetType();
            var members = GetMemberInfos(targetType);

            if (value is Hashtable)
            {
                Hashtable table = value as Hashtable;
                foreach (DictionaryEntry entry in table)
                {
                    string key = entry.Key as String;
                    if (key == TYPEKEY)
                    {
                        // ignore it
                    }
                    else if (members.ContainsKey(key))
                    {
                        MemberInfo member = members[key];
                        object fieldval = Deserialize(entry.Value, TypeUtils.GetMemberType(member));
                        if (member is PropertyInfo) { ((PropertyInfo)member).SetValue(target, fieldval, null); }
                        if (member is FieldInfo) { ((FieldInfo)member).SetValue(target, fieldval); }
                    }
                    else if (ThrowErrorOnSpuriousData)
                    {
                        throw new Exception("Deserializer found key in data but not in class, key=" + key + ", class=" + target);
                    }
                }
            }
            else if (ThrowErrorOnSpuriousData)
            {
                throw new Exception("Deserializer can't populate class or struct from a non-hashtable");
            }
        }


        //
        //
        // SERIALIZE

        private static List<Type> INT_TYPES = new List<Type> { 
            typeof(Int16), typeof(Int32), typeof(UInt16), typeof(UInt32), typeof(Char), typeof(Byte), typeof(SByte) 
        };
        
        /// <summary>
        /// Serializes a class instance or a collection into a parsed value, ready to be converted into JSON.
        /// Also allows the caller to specify if the type of this instance should be added to the 
        /// produced value, otherwise it will be left out.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="specifyType"></param>
        /// <returns></returns>
        public object Serialize(object value, bool specifyType = false)
        {
            if (value == null)
            {
                if (ThrowErrorOnSerializingNull)
                {
                    throw new Exception("Serializer encountered an unexpected null");
                }

                return null;
            }

            Type type = value.GetType();

            // booleans and strings get returned as is
            if (value is Boolean || value is String)
            {
                return value;
            }

            // numeric types and chars get converted to either a double or an int
            if (type.IsPrimitive)
            {
                if (INT_TYPES.Contains(type))
                {
                    return Convert.ToInt32(value);
                }
                else
                {
                    return Convert.ToDouble(value);
                }
            }

            // this is either a collection or a class instance. 
            // if it's a collection and it's not generic, we'll need to explicitly serialize out
            // type names for each value
            bool isPotentiallyUntyped = !type.IsGenericType;

            // if it's a dictionary, convert to hashtable
            if (value is IDictionary)
            {
                return SerializeDictionary(value as IDictionary, isPotentiallyUntyped);
            }

            // if it's a list, convert to array list
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return SerializeEnumerable(value as IEnumerable, isPotentiallyUntyped);
            }

            // it's some other type of class or object - serialize field by field
            return SerializeClassOrStruct(value, specifyType);
        }

        private object SerializeClassOrStruct(object value, bool specifyType)
        {
            Hashtable result = new Hashtable();
            if (specifyType) {
                result[TYPEKEY] = value.GetType().FullName;
            }

            var fields = TypeUtils.GetMembers(value);
            foreach (MemberInfo field in fields)
            {
                string fieldName = field.Name;
                object fieldValue = Serialize(TypeUtils.GetValue(field, value), false);
                bool serialize = true;

                if (SkipDefaultsDuringSerialization)
                {
                    object defaultValue = GetDefaultInstanceValue(value, field);
                    serialize = (fieldValue != null) &&
                                !fieldValue.Equals(defaultValue);
                }
                
                if (serialize)
                {
                    result[fieldName] = fieldValue;
                }
            }

            return result;
        }

        private Hashtable SerializeDictionary(IDictionary dict, bool specifyValueTypes)
        {
            Hashtable results = new Hashtable();
            foreach (DictionaryEntry entry in dict)
            {
                object key = entry.Key;
                object value = Serialize(entry.Value, specifyValueTypes);
                results[key] = value;
            }

            return results;
        }

        private ArrayList SerializeEnumerable(IEnumerable list, bool specifyValueTypes)
        {
            ArrayList results = new ArrayList();
            foreach (object element in list)
            {
                object value = Serialize(element, specifyValueTypes);
                results.Add(value);
            }

            return results;
        }

        //
        //
        // HELPERS

        private bool IsScalar(object value)
        {
            return value is Double || value is Boolean || value is String;
        }

        private Type FindTypeByName(string name, bool ignoreCase = false, bool cache = true)
        {
            // do we have it cached?
            Type result = null;
            if (_ExplicitlyNamedTypes.TryGetValue(name, out result))
            {
                return result;
            }

            // search for it the hard way
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type type = assembly.GetType(name, false, ignoreCase);
                if (type != null)
                {
                    if (cache)
                    {
                        this._ExplicitlyNamedTypes[name] = type;
                    }
                    return type;
                }
            }

            return null;
        }

        private Type InferType(object value)
        {
            if (value is Hashtable)
            {
                var table = value as Hashtable;

                // see if we have the magical type marker
                if (table.ContainsKey(TYPEKEY))
                {
                    // manufacture the type
                    string typestring = table[TYPEKEY] as string;
                    return FindTypeByName(typestring);
                }
                else
                {
                    // it's a dictionary
                    return typeof(Hashtable);
                }
            }

            else if (value is ArrayList)
            {
                return typeof(ArrayList);
            }

            // it's a scalar, no type
            return null;
        }

        //
        //
        // INSTANCE CACHE

        private bool HasDefaultInstance(object o)
        {
            return (o != null) && HasDefaultInstance(o.GetType());
        }

        private bool HasDefaultInstance(Type t)
        {
            return _DefaultInstances.ContainsKey(t);
        }

        private object GetDefaultInstanceValue(object o, MemberInfo field)
        {
            if (o == null) {
                return null;
            }

            Type t = o.GetType();
            object instance = null;
            if (!_DefaultInstances.ContainsKey(t))
            {
                instance = _DefaultInstances[t] = Activator.CreateInstance(t);                
            } else {
                instance = _DefaultInstances[t];
            }

            return TypeUtils.GetValue(field, instance);
        }

    }
}

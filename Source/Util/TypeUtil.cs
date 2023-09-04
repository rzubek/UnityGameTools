// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SomaSim.Util
{
    public static class TypeUtils
    {
        /// <summary>
        /// Maps from a type, to a filtered list of serializable members of this type.
        /// This cache can be cleared as needed (eg. when reloading assemblies)
        /// </summary>
        public static Dictionary<Type, List<MemberInfo>> TypeCache { get; private set; }

        /// <summary>
        /// Maps from a source type and a desired member field type, to a filtered list of serializable members of this type.
        /// This cache can be cleared as needed (eg. when reloading assemblies)
        /// </summary>
        public static Dictionary<Type, Dictionary<Type, List<MemberInfo>>> SourcedTypeCache { get; private set; }

        static TypeUtils () {
            TypeCache = new Dictionary<Type, List<MemberInfo>>();
            SourcedTypeCache = new Dictionary<Type, Dictionary<Type, List<MemberInfo>>>();
        }


        /// <summary>
        /// If the member is either a variable or a property, returns its type, otherwise returns null
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Type GetMemberType (MemberInfo i) {
            if (i is PropertyInfo) {
                PropertyInfo pi = i as PropertyInfo;
                return (pi.CanRead && pi.CanWrite) ? pi.PropertyType : null;
            } else if (i is FieldInfo) {
                FieldInfo fi = i as FieldInfo;
                return fi.IsPublic ? fi.FieldType : null;
            } else {
                return null;
            }
        }

        /// <summary>
        /// If the member is either a variable or a property, sets its value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetValue (MemberInfo i, object obj, object value) {
            if (i is PropertyInfo pi) {
                pi.SetValue(obj, value, null);
            } else if (i is FieldInfo fi) {
                fi.SetValue(obj, value);
            } else { /* do nothing */ }
        }

        /// <summary>
        /// Takes a target object and a value, and sets any fields inside target object
        /// that are compatible with value's type to value.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetValueByType (object obj, object value) {
            List<MemberInfo> members = GetMembersByType(value.GetType(), obj);
            foreach (MemberInfo member in members) {
                SetValue(member, obj, value);
            }
        }

        /// <summary>
        /// If the member is either a variable or a property, returns its value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetValue (MemberInfo i, object obj) =>
            (i is PropertyInfo pi) ? pi.GetValue(obj, null) :
            (i is FieldInfo fi) ? fi.GetValue(obj) :
            null;

        /// <summary>
        /// Returns all serializable fields or properties of a given type (including subtypes) from the given object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMembersByType<T> (object obj)
            => GetMembersByType(typeof(T), obj);

        /// <summary>
        /// Returns all serializable fields or properties of a given type (including subtypes) from the given target type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMembersByType<T> (Type target)
            => GetMembersByType(typeof(T), target);

        /// <summary>
        /// Returns all serializable fields or properties of a given type (including subtypes) from the given object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMembersByType (Type t, object obj)
            => GetMembersByType(t, obj.GetType());

        /// <summary>
        /// Returns all serializable fields or properties of a given type (including subtypes) from the given source type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMembersByType (Type t, Type source) {
            var result = SourcedTypeCache.FindOrNull(source, t);
            if (result == null) {
                result = MakeAndGetMembersByType(t, source);
                SourcedTypeCache.FindOrMakeSubDictionary(source)[t] = result;
            }
            return result;
        }

        private static List<MemberInfo> MakeAndGetMembersByType (Type t, Type source) =>
            // pulled out into its own function so that we don't uncur the cost of allocating a closure unnecessarily
            source.GetMembers().Where(member => {
                Type memberType = GetMemberType(member);
                return (memberType != null) && t.IsAssignableFrom(memberType);
            }).ToList();

        /// <summary>
        /// Returns the unique serializable field or property with the given name.
        /// If there are none, or more than one, returns null.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MemberInfo GetMember (object obj, string name) {
            // MemberInfo[] members = obj.GetType().GetMember(name);
            // return (members.Length == 1) ? members[0] : null;
            var members = GetMembers(obj);
            MemberInfo first = null;
            foreach (var member in members) {
                if (member.Name != name) {
                    continue;
                }
                if (first == null) { first = member; } else { first = null; } // null out if more than one
            }

            return first;
        }

        /// <summary>
        /// Returns all serializable fields or properties from the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMembers (object obj) => GetMembers(obj.GetType());

        /// <summary>
        /// Returns all fields or properties from a type descriptor
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMembers (Type t) {
            var result = TypeCache.FindOrNull(t);
            if (result == null) {
                result = MakeAndGetMembers(t);
                TypeCache[t] = result;
            }
            return result;
        }

        private static List<MemberInfo> MakeAndGetMembers (Type t) =>
            // pulled out into its own function so that we don't uncur the cost of allocating a closure unnecessarily
            t.GetMembers().Where(member => {
                Type memberType = GetMemberType(member);
                return (memberType != null);
            }).ToList();

        /// <summary>
        /// Given an object and a type T, sets all variables and properties of this type 
        /// (including subtypes) to new instances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void MakeMemberInstances<T> (object obj) {
            List<MemberInfo> members = GetMembersByType<T>(obj);
            foreach (MemberInfo member in members) {
                object instance = Activator.CreateInstance(GetMemberType(member), null);
                SetValue(member, obj, instance);
            }
        }

        /// <summary>
        /// Given an object and a type T, sets all variables and properties with given name
        /// to new instances.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        public static void MakeMemberInstances (object obj, string name) {
            MemberInfo member = GetMember(obj, name);
            if (member != null) {
                object instance = Activator.CreateInstance(GetMemberType(member), null);
                SetValue(member, obj, instance);
            }
        }

        /// <summary>
        /// Sets all variables and properties on the given object to new instances.
        /// </summary>
        /// <param name="obj"></param>
        public static void MakeMemberInstancesAll (object obj) {
            List<MemberInfo> members = GetMembers(obj);
            foreach (MemberInfo member in members) {
                object instance = Activator.CreateInstance(GetMemberType(member), null);
                SetValue(member, obj, instance);
            }
        }

        /// <summary>
        /// Clears out all references of variables and properties of the given type (including subtypes)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void RemoveMemberInstances<T> (object obj) {
            List<MemberInfo> members = GetMembersByType<T>(obj);
            foreach (MemberInfo member in members) {
                SetValue(member, obj, null);
            }
        }

        /// <summary>
        /// Returns all non-null values of variables and properties of a given type (including subtypes, reference only)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serv"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetMemberInstances<T> (object serv) {
            var members = GetMembersByType<T>(serv);
            return members.Select(member => GetValue(member, serv)).WhereTypeIs<T>();
        }


        /// <summary>
        /// Iterates through all current assemblies and finds all children of interface/class type t.
        /// </summary>
        public static IEnumerable<Type> FindAllChildrenOf (Type t) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                ass => ass.GetTypes().Where(candidate => candidate != t && t.IsAssignableFrom(candidate)));

        /// <summary>
        /// Returns public / protected / internal methods on a type that match a specific signature
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethodsBySig (Type type, Type returnType, params Type[] parameterTypes) {
            var all = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            return all.Where((m) => {
                if (m.ReturnType != returnType) { return false; }

                var parameters = m.GetParameters();

                if (parameterTypes == null || parameterTypes.Length == 0) { return parameters.Length == 0; }
                if (parameters.Length != parameterTypes.Length) { return false; }

                for (int i = 0; i < parameterTypes.Length; i++) {
                    if (parameters[i].ParameterType != parameterTypes[i]) { return false; }
                }

                return true;
            });
        }
    }
}

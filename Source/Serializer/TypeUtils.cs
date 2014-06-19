using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SomaSim.Serializer
{
    public class TypeUtils
    {
        /// <summary>
        /// If the member is either a variable or a property, returns its type, otherwise returns null
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Type GetMemberType(MemberInfo i) {
            return 
                (i is PropertyInfo) ? ((PropertyInfo)i).PropertyType :
                (i is FieldInfo) ? ((FieldInfo)i).FieldType :
                null;
        }

        /// <summary>
        /// If the member is either a variable or a property, sets its value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetValue(MemberInfo i, object obj, object value) {
            if      (i is PropertyInfo) { ((PropertyInfo)i).SetValue(obj, value, null); }
            else if (i is FieldInfo) { ((FieldInfo)i).SetValue(obj, value); }
            else { /* do nothing */ }
        }

        /// <summary>
        /// If the member is either a variable or a property, returns its value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetValue(MemberInfo i, object obj) { 
            return
                (i is PropertyInfo) ? ((PropertyInfo)i).GetValue(obj, null) :
                (i is FieldInfo) ? ((FieldInfo)i).GetValue(obj) :
                null;
        }

        /// <summary>
        /// Returns all fields or properties of a given type (including subtypes) from the given object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetFieldsByType<T>(object obj)
        {
            MemberInfo[] members = obj.GetType().GetMembers();
            return members.Where(member => {
                Type memberType = GetMemberType(member);
                return (memberType != null) && typeof(T).IsAssignableFrom(memberType);
            }).ToList();
        }

        /// <summary>
        /// Returns all fields or properties from the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetFields(object obj)
        {
            MemberInfo[] members = obj.GetType().GetMembers();
            return members.Where(member =>
            {
                Type memberType = GetMemberType(member);
                return (memberType != null);
            }).ToList();
        }

        /// <summary>
        /// Returns all fields or properties from a type descriptor
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetFields(Type t)
        {
            MemberInfo[] members = t.GetMembers();
            return members.Where(member =>
            {
                Type memberType = GetMemberType(member);
                return (memberType != null);
            }).ToList();
        }

        /// <summary>
        /// Given an object and a type T, sets all variables and properties of this type 
        /// (including subtypes) to new instances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void MakeFieldInstances<T>(object obj)
        {
            List<MemberInfo> members = GetFieldsByType<T>(obj);
            foreach (MemberInfo member in members)
            {
                object instance = Activator.CreateInstance(GetMemberType(member), null);
                SetValue(member, obj, instance);
            }
        }

        /// <summary>
        /// Sets all variables and properties on the given object to new instances.
        /// </summary>
        /// <param name="obj"></param>
        public static void MakeFieldInstancesAll(object obj)
        {
            List<MemberInfo> members = GetFields(obj);
            foreach (MemberInfo member in members)
            {
                object instance = Activator.CreateInstance(GetMemberType(member), null);
                SetValue(member, obj, instance);
            }
        }

        /// <summary>
        /// Clears out all references of variables and properties of the given type (including subtypes)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void RemoveFieldInstances<T>(object obj)
        {
            List<MemberInfo> members = GetFieldsByType<T>(obj);
            foreach (MemberInfo member in members)
            {
                SetValue(member, obj, null);
            }
        }

        /// <summary>
        /// Returns all values of variables and properties of a given type (including subtypes, reference only)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serv"></param>
        /// <returns></returns>
        public static List<T> GetFieldInstances<T>(object serv) where T : class
        {
            var members = GetFieldsByType<T>(serv);
            return members.Select(member => GetValue(member, serv) as T).ToList();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Canducci.QueryableExpressions.Filters.Extensions.Internals
{
    internal static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<Tuple<Type, string>, PropertyInfo> s_propertyCache = new ConcurrentDictionary<Tuple<Type, string>, PropertyInfo>();
        private static readonly ConcurrentDictionary<Tuple<Type, string, string>, MethodInfo> s_methodCache = new ConcurrentDictionary<Tuple<Type, string, string>, MethodInfo>();

        public static PropertyInfo GetPropertyInfo(Type ownerType, string propertyName)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException(nameof(ownerType));
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            Tuple<Type, string> key = Tuple.Create(ownerType, propertyName);
            PropertyInfo property = s_propertyCache.GetOrAdd(key, (Tuple<Type, string> k) =>
            {
                return k.Item1.GetProperty(k.Item2);
            });

            return property;
        }

        public static MethodInfo GetMethod(Type ownerType, string methodName, Type[] parameterTypes)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException(nameof(ownerType));
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            string paramKey = parameterTypes == null ? string.Empty : string.Join(",", parameterTypes.Select(t => t.FullName));
            Tuple<Type, string, string> key = Tuple.Create(ownerType, methodName, paramKey);
            MethodInfo method = s_methodCache.GetOrAdd(key, (Tuple<Type, string, string> k) =>
            {
                return ownerType.GetMethod(methodName, parameterTypes);
            });

            return method;
        }

        public static MethodInfo GetStringMethod(string methodName)
        {
            return GetMethod(typeof(string), methodName, new[] { typeof(string) });
        }
    }
}
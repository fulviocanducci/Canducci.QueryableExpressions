using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Canducci.QueryableExpressions.Filters.Extensions.Internals
{
    internal static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo> s_propertyCache = new ConcurrentDictionary<string, PropertyInfo>();
        private static readonly ConcurrentDictionary<string, MethodInfo> s_methodCache = new ConcurrentDictionary<string, MethodInfo>();

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

            string key = ownerType.FullName + "|" + propertyName;
            PropertyInfo property = s_propertyCache.GetOrAdd(key, (unused) =>
            {
                return ownerType.GetProperty(propertyName);
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
            string key = ownerType.FullName + "|" + methodName + "|" + paramKey;
            MethodInfo method = s_methodCache.GetOrAdd(key, (unused) =>
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
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Canducci.QueryableExpressions.Filters.Extensions.Internals
{
    internal static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<PropertyCacheKey, PropertyInfo> s_propertyCache = new ConcurrentDictionary<PropertyCacheKey, PropertyInfo>();
        private static readonly ConcurrentDictionary<MethodCacheKey, MethodInfo> s_methodCache = new ConcurrentDictionary<MethodCacheKey, MethodInfo>();

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

            PropertyCacheKey key = new PropertyCacheKey(ownerType, propertyName);

            PropertyInfo cached;
            if (s_propertyCache.TryGetValue(key, out cached))
            {
                return cached;
            }

            // Explicit BindingFlags for clarity and predictability
            PropertyInfo property = ownerType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            // Only cache non-null values
            if (property != null)
            {
                s_propertyCache.TryAdd(key, property);
            }

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

            string paramKey = parameterTypes == null ? string.Empty : string.Join("|", parameterTypes.Select(t => t.AssemblyQualifiedName));
            MethodCacheKey key = new MethodCacheKey(ownerType, methodName, paramKey);

            MethodInfo cached;
            if (s_methodCache.TryGetValue(key, out cached))
            {
                return cached;
            }

            MethodInfo method = ownerType.GetMethod(methodName, parameterTypes);

            // Only cache non-null values
            if (method != null)
            {
                s_methodCache.TryAdd(key, method);
            }

            return method;
        }

        public static MethodInfo GetStringMethod(string methodName)
        {
            return GetMethod(typeof(string), methodName, new[] { typeof(string) });
        }

        // custom struct keys to avoid boxing
        private readonly struct PropertyCacheKey : IEquatable<PropertyCacheKey>
        {
            public readonly Type Owner;
            public readonly string Name;

            public PropertyCacheKey(Type owner, string name)
            {
                Owner = owner;
                Name = name ?? string.Empty;
            }

            public bool Equals(PropertyCacheKey other)
            {
                return Owner == other.Owner && string.Equals(Name, other.Name, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                if (obj is PropertyCacheKey)
                {
                    return Equals((PropertyCacheKey)obj);
                }

                return false;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + (Owner != null ? Owner.GetHashCode() : 0);
                    hash = hash * 31 + (Name != null ? Name.GetHashCode() : 0);
                    return hash;
                }
            }
        }

        private readonly struct MethodCacheKey : IEquatable<MethodCacheKey>
        {
            public readonly Type Owner;
            public readonly string MethodName;
            public readonly string ParamKey;

            public MethodCacheKey(Type owner, string methodName, string paramKey)
            {
                Owner = owner;
                MethodName = methodName ?? string.Empty;
                ParamKey = paramKey ?? string.Empty;
            }

            public bool Equals(MethodCacheKey other)
            {
                return Owner == other.Owner
                    && string.Equals(MethodName, other.MethodName, StringComparison.Ordinal)
                    && string.Equals(ParamKey, other.ParamKey, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                if (obj is MethodCacheKey)
                {
                    return Equals((MethodCacheKey)obj);
                }

                return false;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + (Owner != null ? Owner.GetHashCode() : 0);
                    hash = hash * 31 + (MethodName != null ? MethodName.GetHashCode() : 0);
                    hash = hash * 31 + (ParamKey != null ? ParamKey.GetHashCode() : 0);
                    return hash;
                }
            }
        }
    }
}
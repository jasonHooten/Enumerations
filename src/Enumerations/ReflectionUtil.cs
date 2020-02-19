using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Enumerations
{
    public static class ReflectionUtil
    {
        private static readonly IDictionary<Type, IDictionary<string, PropertyInfo>> CachedProperties =
            new ConcurrentDictionary<Type, IDictionary<string, PropertyInfo>>();

        public static string GetPropertyName<T>(Expression<Func<T, object>> expr)
        {
            var expression = (Expression) expr;
            while (true)
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                        expression = (expression as UnaryExpression)?.Operand;
                        break;
                    case ExpressionType.Lambda:
                        expression = ((LambdaExpression) expression).Body;
                        break;
                    case ExpressionType.MemberAccess:
                        return (expression as MemberExpression)?.Member.Name;
                    default:
                        var message = $"Unable to obtain MemberExpression from expression: {expr}";
                        throw new NotImplementedException(message);
                }
        }

        /// <summary> Reflection helper that returns PropertyInfo for the given property name on the type </summary>
        public static PropertyInfo GetPropertyInfo<T>(string propertyName)
        {
            return GetPropertyInfo<T>(propertyName, null);
        }

        /// <summary> Reflection helper that returns PropertyInfo for the given property name on the type </summary>
        public static PropertyInfo GetPropertyInfo<T>(string propertyName, BindingFlags? bindingFlags)
        {
            return GetPropertyInfo(typeof(T), propertyName, bindingFlags);
        }

        /// <summary> Reflection helper that returns PropertyInfo for the given property name on the type </summary>
        public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            return GetPropertyInfo(type, propertyName, null);
        }

        /// <summary> Reflection helper that returns PropertyInfo for the given property name on the type </summary>
        public static PropertyInfo GetPropertyInfo(Type type, string propertyName, BindingFlags? bindingFlags)
        {
            var propertyDictionary =
                CachedProperties.GetOrAdd(type, () => new ConcurrentDictionary<string, PropertyInfo>());
            var propertyInfo = propertyDictionary.GetOrAdd(propertyName,
                () => bindingFlags == null
                    ? type.GetProperty(propertyName)
                    : type.GetProperty(propertyName, bindingFlags.Value));
            return propertyInfo;
        }

        public static string GetMethodName<T>(Expression<Action<T>> expr)
        {
            return GetMethodInfo(expr).Name;
        }

        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expr)
        {
            dynamic body = expr.Body;
            return (MethodInfo) body.Method;
        }

        public static IEnumerable<FieldInfo> GetPrivateFieldRecursive(Type type, string fieldName)
        {
            foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.DeclaredOnly))
                if (field.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                    yield return field;

            if (type.BaseType != null)
                foreach (var field in GetPrivateFieldRecursive(type.BaseType, fieldName))
                    yield return field;
        }

        public static IEnumerable<PropertyInfo> GetPrivatePropertyRecursive(Type type, string fieldName)
        {
            foreach (var property in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance |
                                                        BindingFlags.DeclaredOnly))
                if (property.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                    yield return property;

            if (type.BaseType != null)
                foreach (var property in GetPrivatePropertyRecursive(type.BaseType, fieldName))
                    yield return property;
        }

        public static IEnumerable<Type> GetAllBaseClassesAndInterfaces(Type type)
        {
            foreach (var i in type.GetInterfaces())
                yield return i;

            var baseType = type;
            while (baseType != typeof(object) && baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
    }
}
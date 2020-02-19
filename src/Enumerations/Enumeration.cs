using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Enumerations
{
    /// <summary>
    ///     Best used when enums would be normally tied to business logic.
    /// </summary>
    /// <example>
    ///     public class EmployeeType : Enumeration
    ///     {
    ///         public static readonly EmployeeType Manager = new EmployeeType(0, "Manager");
    ///         public static readonly EmployeeType Servant = new EmployeeType(1, "Servant");
    ///         public static readonly EmployeeType AssistantToTheRegionalManager = new EmployeeType(2, "Assistant to the Regional Manager");
    
    ///         private EmployeeType() { }
    ///         private EmployeeType(int value, string displayName) : base(value, displayName) { }
    ///     }
    /// </example>
    [Serializable]
    [DataContract]
    public abstract class Enumeration : IComparable, IEquatable<Enumeration>, IComparable<Enumeration>,
        IComparer<Enumeration>, IConvertible
    {
        [DataMember(Order = 1)] public int Value { get; }
        
        public virtual string DisplayName { get; private set; }
        
        public string FieldName { get; private set; }


        [NonSerialized]
        private static readonly ConcurrentDictionary<Type, IList<Enumeration>> CachedGetAll =
            new ConcurrentDictionary<Type, IList<Enumeration>>();

        [NonSerialized]
        private static readonly ConcurrentDictionary<Type, MethodInfo> GuessFromMethod =
            new ConcurrentDictionary<Type, MethodInfo>();

        [NonSerialized]
        private static readonly ConcurrentDictionary<Type, MethodInfo> GetAllMethod =
            new ConcurrentDictionary<Type, MethodInfo>();

        /// ////////////////////////////////////////////////////////////////////////////////
        /// Constructors

        private Enumeration() {} //not used

        protected Enumeration(int value, string displayName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    "Unable to have an Enumeration with a value < 0 as it's sometimes casted to unsigned values");
            Value = value;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }

        public override string ToString() => DisplayName;

        protected virtual bool MatchesGuessFrom(object anything) => false;

        public bool IsAny(params Enumeration[] enumerations) => enumerations.Contains(this);

        #region IComparable
        public int CompareTo(object other)
        {
            return CompareTo((Enumeration)other);
        }

        public virtual int CompareTo(Enumeration other)
        {
            return Value.CompareTo(other.Value);
        }

        public int Compare(Enumeration x, Enumeration y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            return x.CompareTo(y);
        }
        #endregion

        #region IEquatable

        public bool Equals(Enumeration other)
        {
            if (ReferenceEquals(other, null))
                return false;

            var typeMatches = GetType() == other.GetType();
            var valueMatches = Value.Equals(other.Value);

            return typeMatches && valueMatches;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Enumeration);
        }


        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        private class InternalComparer : IComparer<Enumeration>
        {
            public int Compare(Enumeration x, Enumeration y)
            {
                var xVal = x?.Value ?? int.MinValue;
                var yVal = y?.Value ?? int.MinValue;
                return xVal.CompareTo(yVal);
            }
        }

        public static readonly IComparer<Enumeration> Comparer = new InternalComparer();

        #endregion

        #region IConvertible

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Int32;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(Value, provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(Value, provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(Value, provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(Value, provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(Value, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(Value, provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(Value, provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(Value, provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(Value, provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(Value, provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(Value, provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(Value, provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(Value, provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(Value, provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return Convert.ToString(Value, provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(Value, ((IConvertible) this).GetTypeCode(), provider);
        }

        #endregion


        #region Statics Methods on Enumeration

        /// ///////////////////////////////////////////////////////////////////////////////////////////
        /// Private Static Methods

        private static MethodInfo GetAllMethodInfo(Type type)
        {
            var getAllMethodName = ReflectionUtil.GetMethodName<object>(x => GetAll<Enumeration>(false));
            var getAllGenericMethod = typeof(Enumeration)
                .GetMethods()
                .Where(m => m.Name.Equals(getAllMethodName, StringComparison.InvariantCultureIgnoreCase))
                .First(m => m.ContainsGenericParameters)
                .GetGenericMethodDefinition()
                .MakeGenericMethod(type);
            return getAllGenericMethod;
        }

        private static IList<T> GetAll<T>(bool includeHidden) where T : Enumeration
        {
            var enumType = typeof(T);
            var allEnums = CachedGetAll.GetOrAdd(enumType, t => (IList<Enumeration>)GetAllDirect<T>());

            if (includeHidden || !typeof(OrderedEnumeration).IsAssignableFrom(enumType))
                return (IList<T>)allEnums;

            var orderedEnums = allEnums
                .Cast<OrderedEnumeration>()
                .Where(x => !x.HideInGetAll)
                .Cast<T>()
                .ToArray();
            return orderedEnums;
        }

        private static IList<T> GetAllDirect<T>() where T : Enumeration
        {
            var enumType = typeof(T);

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly |
                                              BindingFlags.FlattenHierarchy;
            // get a list of every field on this enum type as well as any Enumeration declaring types recursively
            IEnumerable<FieldInfo> enumerationFields = enumType.GetFields(bindingFlags);
            var declaringType = enumType;
            while ((declaringType = declaringType.DeclaringType) != null &&
                   typeof(Enumeration).IsAssignableFrom(typeof(Enumeration)))
                enumerationFields = enumerationFields.Concat(declaringType.GetFields(bindingFlags));
            // obtain the value of every enum along with its field name
            var enumerationFieldsAndNames =
                enumerationFields.Select(field => new { fieldName = field.Name, value = field.GetValue(null) as T });
            var enumerationFieldsAndNamesOfTypeT = enumerationFieldsAndNames.Where(x => x.value != null)
                .ToArray();
            // set the field name on the enumeration
            foreach (var enumField in enumerationFieldsAndNamesOfTypeT)
                enumField.value.FieldName = enumField.fieldName;

            // order enums if type matches
            IEnumerable<Enumeration> enumValues = enumerationFieldsAndNamesOfTypeT.Select(x => x.value);
            if (typeof(OrderedEnumeration).IsAssignableFrom(enumType))
                enumValues = enumValues.Cast<OrderedEnumeration>().OrderBy(x => x.Order);

            return enumValues.Cast<T>().ToArray();
        }

        private static MethodInfo GuessFromMethodInfo(Type type)
        {
            var guessFromMethodName = ReflectionUtil.GetMethodName<object>(x => GuessFrom<Enumeration>(x, true));
            var guessFromGenericMethod = typeof(Enumeration)
                .GetMethods()
                .Where(m => m.Name.Equals(guessFromMethodName, StringComparison.InvariantCultureIgnoreCase))
                .First(m => m.ContainsGenericParameters)
                .GetGenericMethodDefinition()
                .MakeGenericMethod(type);
            return guessFromGenericMethod;
        }

        internal static T Parse<T, TK>(TK value, string description, Func<T, bool> predicate, bool throwOnError = true)
            where T : Enumeration
        {
            var all = GetAll<T>(true);
            var matchingItem = all.FirstOrDefault(predicate);

            if (matchingItem == null && throwOnError)
            {
                var message = $"'{value}' is not a valid {description} in {typeof(T)}";
                throw new ApplicationException(message);
            }

            return matchingItem;
        }


        /// //////////////////////////////////////////////////////////////////////////////////////
        /// Public Static Methods

        public static IList<T> GetAll<T>() where T : Enumeration
        {
            return GetAll<T>(false);
        }

        public static IList<Enumeration> GetAll(Type type)
        {
            var getAllGenericMethod = GetAllMethod.GetOrAdd(type, GetAllMethodInfo);
            return (IList<Enumeration>)getAllGenericMethod.Invoke(null, null);
        }

        public static T FromValue<T>(int value) where T : Enumeration
        {
            var matchingItem = Parse<T, int>(value, "value", item => item.Value == value);
            return matchingItem;
        }

        public static T FromValue<T>(object value) where T : Enumeration
        {
            var valueAsInt = Convert.ToInt32(value);
            return FromValue<T>(valueAsInt);
        }

        public static T FromDisplayName<T>(string displayName) where T : Enumeration
        {
            var matchingItem = Parse<T, string>(displayName, "display name",
                item => item.DisplayName.Equals(displayName, StringComparison.Ordinal));
            return matchingItem;
        }

        public static T FromFieldName<T>(string fieldName) where T : Enumeration
        {
            var matchingItem = Parse<T, string>(fieldName, "field name",
                item => item.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            return matchingItem;
        }

        public static T GuessFrom<T>(object anything, bool throwOnError = true) where T : Enumeration
        {
            try
            {
                if (anything == null)
                    throw new ArgumentNullException(nameof(anything));

                if (anything is T)
                    return (T)anything;

                if (anything is int || anything is byte || anything is float || anything is double ||
                    anything is decimal)
                    return FromValue<T>(Convert.ToInt32(anything));

                var str = anything.ToString();
                if (int.TryParse(str, out var value))
                    return FromValue<T>(value);

                var customGuessFrom = GetAll<T>(true).FirstOrDefault(x => x.MatchesGuessFrom(anything));
                if (customGuessFrom != null)
                    return customGuessFrom;

                var trimmedStr = str.Trim().Replace(" ", "");

                bool Predicate(T item) =>
                    item.DisplayName.Equals(str, StringComparison.InvariantCultureIgnoreCase)
                    || item.FieldName.Equals(str, StringComparison.InvariantCultureIgnoreCase)
                    || item.DisplayName.Equals(trimmedStr, StringComparison.InvariantCultureIgnoreCase)
                    || item.FieldName.Equals(trimmedStr, StringComparison.InvariantCultureIgnoreCase);

                return Parse(str, "guess value", (Func<T, bool>)Predicate, throwOnError);
            }
            catch
            {
                if (throwOnError)
                    throw;
                return null;
            }
        }

        public static T GuessFromNullSafe<T>(object anything, bool throwOnError = true) where T : Enumeration
        {
            return anything == null ? null : GuessFrom<T>(anything, throwOnError);
        }

        public static object GuessFrom(object anything, Type type, bool throwOnError = true)
        {
            var guessFromGenericMethod = GuessFromMethod.GetOrAdd(type, () => GuessFromMethodInfo(type));
            return guessFromGenericMethod.Invoke(null, new[] { anything, throwOnError });
        }


        /// //////////////////////////////////////////////////////////////////////////////////
        /// Operators

        public static implicit operator int(Enumeration enumeration) => enumeration.Value;

        public static bool operator ==(Enumeration e1, Enumeration e2)
        {
            return e1?.Equals(e2) ?? ReferenceEquals(e2, null);
        }

        public static bool operator !=(Enumeration e1, Enumeration e2)
        {
            return !(e1 == e2);
        }

        public static bool operator ==(Enumeration e1, object e2)
        {
            return e1 == e2 as Enumeration;
        }

        public static bool operator !=(Enumeration e1, object e2)
        {
            return !(e1 == e2);
        }

        #endregion
    }
}
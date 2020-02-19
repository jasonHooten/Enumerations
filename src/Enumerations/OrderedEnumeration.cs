namespace Enumerations
{
    public abstract class OrderedEnumeration : Enumeration
    {
        public int Order { get; }
        public bool HideInGetAll { get; }

        protected OrderedEnumeration(int value, string displayName, int order, bool hideInGetAll = false)
            : base(value, displayName)
        {
            Order = order;
            HideInGetAll = hideInGetAll;
        }

        public static T FromOrder<T>(int order) where T : OrderedEnumeration
        {
            var matchingItem = Parse<T, int>(order, "order", item => item.Order == order);
            return matchingItem;
        }

        public override int CompareTo(Enumeration other)
        {
            var orderedEnumOther = other as OrderedEnumeration;
            return (orderedEnumOther != null)
                ? Order.CompareTo(orderedEnumOther.Order)
                : base.CompareTo(other);
        }
    }
}

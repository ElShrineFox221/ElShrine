namespace ElShrine.Common.DataStructure
{
    public class ValueStream<TValue>(List<TValue> values, TValue endSign) : ICloneable<ValueStream<TValue>>
    {
        protected readonly List<TValue> valuesSource = values;
        public IReadOnlyList<TValue> Values { get; set; } = values;
        public TValue EndSign { get; set; } = endSign;
        public int CurrentIndex { get; set; }
        public int Count => Values.Count;
        public bool IsEnd => Values.Count <= CurrentIndex;
        public TValue CurrentValue => IsEnd ? EndSign : Values[CurrentIndex];
        public TValue Peek() => CurrentValue;
        public TValue Consume() => IsEnd ? EndSign : Values[CurrentIndex++];
        public TValue this[int index] => index >= Values.Count ? EndSign : Values[index];

        public virtual object Clone()
        {
            var clone = new ValueStream<TValue>(valuesSource, EndSign)
            {
                CurrentIndex = CurrentIndex
            };
            return clone;
        }
    }
}

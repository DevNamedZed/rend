namespace Rend.Style.Internal
{
    /// <summary>
    /// Represents a single counter directive parsed from a <c>counter-reset</c>,
    /// <c>counter-increment</c>, or <c>counter-set</c> declaration.
    /// </summary>
    internal readonly struct CounterEntry
    {
        public CounterEntry(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public int Value { get; }
    }
}

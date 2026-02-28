namespace Rend.Css.Media.Internal
{
    /// <summary>
    /// A parsed media query with type and feature conditions.
    /// </summary>
    internal sealed class MediaQuery
    {
        public bool Negated { get; set; }
        public string? MediaType { get; set; }
        public MediaFeature[]? Features { get; set; }
    }

    /// <summary>
    /// A single media feature condition (e.g. "min-width: 768px").
    /// </summary>
    internal sealed class MediaFeature
    {
        public string Name { get; set; } = "";
        public float? Value { get; set; }
    }
}

namespace Rend.Css.Cascade.Internal
{
    /// <summary>
    /// The origin of a CSS declaration, for cascade sorting.
    /// </summary>
    internal enum CascadeOrigin : byte
    {
        /// <summary>Browser default styles.</summary>
        UserAgent = 0,

        /// <summary>User-provided styles.</summary>
        User = 1,

        /// <summary>Author styles (stylesheets and inline).</summary>
        Author = 2
    }
}

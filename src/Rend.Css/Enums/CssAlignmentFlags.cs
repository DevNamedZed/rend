namespace Rend.Css
{
    /// <summary>
    /// [CSS-ALIGN-3 §5.4] Bit flags for alignment overflow position.
    /// The safe bit is OR'd into the alignment enum value's integer storage.
    /// Use <see cref="IsSafe"/> to test and <see cref="StripSafe"/> to extract
    /// the base alignment value. The unset sentinel (255) is preserved as-is.
    /// </summary>
    public static class CssAlignmentFlags
    {
        public const int SafeBit = 128;
        private const int UnsetSentinel = 255;

        public static bool IsSafe(int value)
        {
            return value != UnsetSentinel && (value & SafeBit) != 0;
        }

        public static int StripSafe(int value)
        {
            if (value == UnsetSentinel)
            {
                return value;
            }
            return value & ~SafeBit;
        }
    }
}

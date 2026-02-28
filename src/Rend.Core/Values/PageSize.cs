namespace Rend.Core.Values
{
    /// <summary>
    /// Standard page sizes in PDF points (1 point = 1/72 inch).
    /// </summary>
    public static class PageSize
    {
        public static readonly SizeF A0 = new SizeF(2383.94f, 3370.39f);
        public static readonly SizeF A1 = new SizeF(1683.78f, 2383.94f);
        public static readonly SizeF A2 = new SizeF(1190.55f, 1683.78f);
        public static readonly SizeF A3 = new SizeF(841.89f, 1190.55f);
        public static readonly SizeF A4 = new SizeF(595.28f, 841.89f);
        public static readonly SizeF A5 = new SizeF(419.53f, 595.28f);
        public static readonly SizeF A6 = new SizeF(297.64f, 419.53f);

        public static readonly SizeF B0 = new SizeF(2834.65f, 4008.19f);
        public static readonly SizeF B1 = new SizeF(2004.09f, 2834.65f);
        public static readonly SizeF B2 = new SizeF(1417.32f, 2004.09f);
        public static readonly SizeF B3 = new SizeF(1000.63f, 1417.32f);
        public static readonly SizeF B4 = new SizeF(708.66f, 1000.63f);
        public static readonly SizeF B5 = new SizeF(498.90f, 708.66f);

        public static readonly SizeF Letter = new SizeF(612f, 792f);
        public static readonly SizeF Legal = new SizeF(612f, 1008f);
        public static readonly SizeF Tabloid = new SizeF(792f, 1224f);
        public static readonly SizeF Ledger = new SizeF(1224f, 792f);
        public static readonly SizeF Executive = new SizeF(521.86f, 756f);

        /// <summary>
        /// Returns the landscape orientation of a page size.
        /// </summary>
        public static SizeF Landscape(SizeF size)
        {
            return size.Width > size.Height ? size : new SizeF(size.Height, size.Width);
        }

        /// <summary>
        /// Returns the portrait orientation of a page size.
        /// </summary>
        public static SizeF Portrait(SizeF size)
        {
            return size.Height > size.Width ? size : new SizeF(size.Height, size.Width);
        }
    }
}

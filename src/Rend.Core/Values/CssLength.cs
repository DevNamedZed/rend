using System;
using System.Runtime.CompilerServices;

namespace Rend.Core.Values
{
    /// <summary>
    /// Represents a CSS length value with a unit. Immutable value type.
    /// </summary>
    public readonly struct CssLength : IEquatable<CssLength>
    {
        public float Value { get; }
        public CssLengthUnit Unit { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CssLength(float value, CssLengthUnit unit)
        {
            Value = value;
            Unit = unit;
        }

        public static readonly CssLength Zero = new CssLength(0, CssLengthUnit.Px);
        public static readonly CssLength Auto = new CssLength(0, CssLengthUnit.Auto);

        public bool IsAuto => Unit == CssLengthUnit.Auto;
        public bool IsNone => Unit == CssLengthUnit.None;
        public bool IsZero => Value == 0 && Unit != CssLengthUnit.Auto && Unit != CssLengthUnit.None;

        /// <summary>
        /// Resolve this length to an absolute pixel value.
        /// </summary>
        /// <param name="ctx">Resolution context providing font size, viewport, etc.</param>
        public float ToPx(CssResolutionContext ctx)
        {
            switch (Unit)
            {
                case CssLengthUnit.Px: return Value;
                case CssLengthUnit.Pt: return Value * 96f / 72f;
                case CssLengthUnit.Pc: return Value * 96f / 6f;
                case CssLengthUnit.In: return Value * 96f;
                case CssLengthUnit.Cm: return Value * 96f / 2.54f;
                case CssLengthUnit.Mm: return Value * 96f / 25.4f;
                case CssLengthUnit.Q: return Value * 96f / 101.6f;
                case CssLengthUnit.Em: return Value * ctx.FontSize;
                case CssLengthUnit.Rem: return Value * ctx.RootFontSize;
                case CssLengthUnit.Ex: return Value * ctx.FontSize * 0.5f; // approximate
                case CssLengthUnit.Ch: return Value * ctx.FontSize * 0.5f; // approximate
                case CssLengthUnit.Vw: return Value * ctx.ViewportWidth / 100f;
                case CssLengthUnit.Vh: return Value * ctx.ViewportHeight / 100f;
                case CssLengthUnit.Vmin: return Value * Math.Min(ctx.ViewportWidth, ctx.ViewportHeight) / 100f;
                case CssLengthUnit.Vmax: return Value * Math.Max(ctx.ViewportWidth, ctx.ViewportHeight) / 100f;
                case CssLengthUnit.Percent: return Value * ctx.PercentBase / 100f;
                case CssLengthUnit.Auto:
                case CssLengthUnit.None:
                    return 0;
                default:
                    return Value;
            }
        }

        /// <summary>
        /// Convert to PDF points (1pt = 1/72 inch). Used for PDF coordinate output.
        /// </summary>
        public float ToPt(CssResolutionContext ctx)
        {
            float px = ToPx(ctx);
            return px * 72f / 96f;
        }

        public static CssLength Px(float value) => new CssLength(value, CssLengthUnit.Px);
        public static CssLength Pt(float value) => new CssLength(value, CssLengthUnit.Pt);
        public static CssLength Em(float value) => new CssLength(value, CssLengthUnit.Em);
        public static CssLength Rem(float value) => new CssLength(value, CssLengthUnit.Rem);
        public static CssLength Percent(float value) => new CssLength(value, CssLengthUnit.Percent);
        public static CssLength Cm(float value) => new CssLength(value, CssLengthUnit.Cm);
        public static CssLength Mm(float value) => new CssLength(value, CssLengthUnit.Mm);
        public static CssLength In(float value) => new CssLength(value, CssLengthUnit.In);

        public bool Equals(CssLength other) => Value == other.Value && Unit == other.Unit;
        public override bool Equals(object? obj) => obj is CssLength other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Value, Unit);
        public static bool operator ==(CssLength left, CssLength right) => left.Equals(right);
        public static bool operator !=(CssLength left, CssLength right) => !left.Equals(right);

        public override string ToString()
        {
            if (Unit == CssLengthUnit.Auto) return "auto";
            if (Unit == CssLengthUnit.None) return "none";
            return $"{Value}{GetUnitSuffix(Unit)}";
        }

        private static string GetUnitSuffix(CssLengthUnit unit)
        {
            switch (unit)
            {
                case CssLengthUnit.Px: return "px";
                case CssLengthUnit.Pt: return "pt";
                case CssLengthUnit.Pc: return "pc";
                case CssLengthUnit.In: return "in";
                case CssLengthUnit.Cm: return "cm";
                case CssLengthUnit.Mm: return "mm";
                case CssLengthUnit.Q: return "Q";
                case CssLengthUnit.Em: return "em";
                case CssLengthUnit.Rem: return "rem";
                case CssLengthUnit.Ex: return "ex";
                case CssLengthUnit.Ch: return "ch";
                case CssLengthUnit.Vw: return "vw";
                case CssLengthUnit.Vh: return "vh";
                case CssLengthUnit.Vmin: return "vmin";
                case CssLengthUnit.Vmax: return "vmax";
                case CssLengthUnit.Percent: return "%";
                default: return "";
            }
        }
    }

    /// <summary>
    /// CSS length units.
    /// </summary>
    public enum CssLengthUnit : byte
    {
        Px, Em, Rem, Ex, Ch,
        Pt, Pc, Cm, Mm, In, Q,
        Vw, Vh, Vmin, Vmax,
        Percent,
        Auto, None
    }

    /// <summary>
    /// Context required to resolve relative CSS lengths to absolute pixel values.
    /// </summary>
    public readonly struct CssResolutionContext
    {
        /// <summary>Current element's computed font-size in px.</summary>
        public float FontSize { get; }

        /// <summary>Root element's computed font-size in px (for rem).</summary>
        public float RootFontSize { get; }

        /// <summary>Viewport width in px (for vw, vmin, vmax).</summary>
        public float ViewportWidth { get; }

        /// <summary>Viewport height in px (for vh, vmin, vmax).</summary>
        public float ViewportHeight { get; }

        /// <summary>The base value for percentage resolution (e.g. containing block width).</summary>
        public float PercentBase { get; }

        public CssResolutionContext(float fontSize, float rootFontSize,
                                     float viewportWidth, float viewportHeight,
                                     float percentBase = 0)
        {
            FontSize = fontSize;
            RootFontSize = rootFontSize;
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            PercentBase = percentBase;
        }

        /// <summary>Default context: 16px font, 1920x1080 viewport.</summary>
        public static readonly CssResolutionContext Default =
            new CssResolutionContext(16, 16, 1920, 1080);
    }
}

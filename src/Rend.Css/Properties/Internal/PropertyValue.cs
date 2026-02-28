using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rend.Core.Values;

namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// A resolved property value. Union struct that can hold different value types
    /// without boxing. Indexed by PropertyId in ComputedStyle.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct PropertyValue
    {
        /// <summary>Whether this property has been set (vs default/unset).</summary>
        [FieldOffset(0)]
        public bool IsSet;

        /// <summary>The type of value stored.</summary>
        [FieldOffset(1)]
        public PropertyValueType Type;

        /// <summary>Integer value (for keyword enums, z-index, etc.).</summary>
        [FieldOffset(4)]
        public int IntValue;

        /// <summary>Float value (for lengths in px, numbers, opacity, etc.).</summary>
        [FieldOffset(4)]
        public float FloatValue;

        /// <summary>Color packed as ARGB uint.</summary>
        [FieldOffset(4)]
        public uint ColorValue;

        // String/CssValue are stored in a separate side array since they're reference types
        // and cannot overlap with value types in a StructLayout(Explicit).

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyValue FromKeyword(int keyword)
        {
            var pv = new PropertyValue();
            pv.IsSet = true;
            pv.Type = PropertyValueType.Keyword;
            pv.IntValue = keyword;
            return pv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyValue FromLength(float px)
        {
            var pv = new PropertyValue();
            pv.IsSet = true;
            pv.Type = PropertyValueType.Length;
            pv.FloatValue = px;
            return pv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyValue FromColor(CssColor color)
        {
            var pv = new PropertyValue();
            pv.IsSet = true;
            pv.Type = PropertyValueType.Color;
            pv.ColorValue = (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
            return pv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyValue FromNumber(float value)
        {
            var pv = new PropertyValue();
            pv.IsSet = true;
            pv.Type = PropertyValueType.Number;
            pv.FloatValue = value;
            return pv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyValue FromInt(int value)
        {
            var pv = new PropertyValue();
            pv.IsSet = true;
            pv.Type = PropertyValueType.Keyword;
            pv.IntValue = value;
            return pv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CssColor ToColor()
        {
            return new CssColor(
                (byte)((ColorValue >> 16) & 0xFF),
                (byte)((ColorValue >> 8) & 0xFF),
                (byte)(ColorValue & 0xFF),
                (byte)((ColorValue >> 24) & 0xFF));
        }
    }
}

namespace Rend.Css.Properties.Internal
{
    /// <summary>
    /// Describes a CSS property: its name, whether it's inherited, and its value type.
    /// </summary>
    internal sealed class PropertyDescriptor
    {
        /// <summary>The CSS property name (lowercase).</summary>
        public string Name { get; }

        /// <summary>The property ID (index into PropertyValue arrays).</summary>
        public int Id { get; }

        /// <summary>Whether this property is inherited by default.</summary>
        public bool Inherited { get; }

        /// <summary>The expected value type for resolved values.</summary>
        public PropertyValueType ValueType { get; }

        public PropertyDescriptor(string name, int id, bool inherited, PropertyValueType valueType)
        {
            Name = name;
            Id = id;
            Inherited = inherited;
            ValueType = valueType;
        }
    }

    /// <summary>
    /// The type of value stored in a PropertyValue.
    /// </summary>
    internal enum PropertyValueType : byte
    {
        /// <summary>A keyword enum value (stored as int).</summary>
        Keyword,

        /// <summary>A length value in px (stored as float).</summary>
        Length,

        /// <summary>A color value (stored as RGBA uint).</summary>
        Color,

        /// <summary>A numeric value (stored as float).</summary>
        Number,

        /// <summary>A string value (font-family, content, etc.).</summary>
        String,

        /// <summary>A raw CssValue (for complex values not yet resolved).</summary>
        Raw
    }
}

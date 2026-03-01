using Rend.Css;

namespace Rend.Style
{
    /// <summary>
    /// A pseudo-element node (::before or ::after) in the styled tree.
    /// Contains the generated content text and its own computed style.
    /// </summary>
    public sealed class StyledPseudoElement : StyledNode
    {
        public StyledPseudoElement(string pseudoType, string content, ComputedStyle style)
            : base(style)
        {
            PseudoType = pseudoType;
            Content = content;
        }

        public override bool IsText => false;

        /// <summary>"before" or "after".</summary>
        public string PseudoType { get; }

        /// <summary>The generated content text.</summary>
        public string Content { get; }

        /// <summary>Whether this is a ::before pseudo-element.</summary>
        public bool IsBefore => PseudoType == "before";

        /// <summary>Whether this is a ::after pseudo-element.</summary>
        public bool IsAfter => PseudoType == "after";
    }
}

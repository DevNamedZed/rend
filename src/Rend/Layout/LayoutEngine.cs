using System.Collections.Generic;
using Rend.Core.Values;
using Rend.Css;
using Rend.Fonts;
using Rend.Layout.Internal;
using Rend.Style;
using Rend.Text;

namespace Rend.Layout
{
    /// <summary>
    /// Public entry point for layout: takes a StyledTree + options → produces a LayoutDocument.
    /// </summary>
    public sealed class LayoutEngine
    {
        private readonly IFontProvider? _fontProvider;
        private readonly ITextShaper? _textShaper;

        public LayoutEngine(IFontProvider? fontProvider = null, ITextShaper? textShaper = null)
        {
            _fontProvider = fontProvider;
            _textShaper = textShaper;
        }

        /// <summary>
        /// Layout the styled tree into a paginated LayoutDocument.
        /// </summary>
        public LayoutDocument Layout(StyledTree styledTree, LayoutOptions? options = null)
        {
            options = options ?? LayoutOptions.Default;
            var context = new LayoutContext(options, _fontProvider, _textShaper);

            // Calculate content area from page style
            var pageStyle = styledTree.PageStyle;
            float contentWidth = pageStyle.PageSize.Width - pageStyle.MarginLeft - pageStyle.MarginRight;
            float contentHeight = pageStyle.PageSize.Height - pageStyle.MarginTop - pageStyle.MarginBottom;

            // Override from options if set differently
            if (options != LayoutOptions.Default)
            {
                contentWidth = options.PageSize.Width - options.MarginLeft - options.MarginRight;
                contentHeight = options.PageSize.Height - options.MarginTop - options.MarginBottom;
            }

            context.ContainingBlockWidth = contentWidth;
            context.ContainingBlockHeight = contentHeight;

            // Create root layout box
            var rootBox = new LayoutBox(styledTree.Root, BoxType.Block);
            rootBox.ContentRect = new RectF(
                pageStyle.MarginLeft,
                pageStyle.MarginTop,
                contentWidth,
                0); // Height determined by content

            // Apply box model to root
            BoxModelCalculator.ApplyBoxModel(rootBox, styledTree.Root.Style, contentWidth);

            // Layout the tree
            BlockFormattingContext.Layout(rootBox, context);

            // Apply positioning to all boxes
            ApplyPositioningRecursive(rootBox, rootBox);

            // Build stacking contexts (sets ZIndex and EstablishesStackingContext on boxes)
            StackingContext.Build(rootBox);

            // Calculate final root height
            float rootHeight = CalculateAutoHeight(rootBox);
            rootBox.ContentRect = new RectF(
                rootBox.ContentRect.X, rootBox.ContentRect.Y,
                rootBox.ContentRect.Width, rootHeight);

            // Paginate
            List<LayoutPage> pages;
            if (options.Paginate)
            {
                pages = Paginator.Paginate(rootBox, options, pageStyle);
            }
            else
            {
                var page = new LayoutPage(
                    pageStyle.PageSize.Width,
                    rootHeight + pageStyle.MarginTop + pageStyle.MarginBottom,
                    rootBox)
                { PageIndex = 0 };
                pages = new List<LayoutPage> { page };
            }

            return new LayoutDocument(rootBox, pages);
        }

        private static void ApplyPositioningRecursive(LayoutBox box, LayoutBox containingBlock, LayoutBox? rootBox = null)
        {
            var root = rootBox ?? box;
            var style = box.StyledNode?.Style;
            if (style != null && style.Position != CssPosition.Static)
            {
                PositionedLayout.ApplyPositioning(box, containingBlock, root);
            }

            var newContaining = (style != null && style.Position != CssPosition.Static) ? box : containingBlock;

            for (int i = 0; i < box.Children.Count; i++)
            {
                ApplyPositioningRecursive(box.Children[i], newContaining, root);
            }
        }

        private static float CalculateAutoHeight(LayoutBox box)
        {
            float height = 0;

            for (int i = 0; i < box.Children.Count; i++)
            {
                var child = box.Children[i];
                float childBottom = child.ContentRect.Y + child.ContentRect.Height
                                  + child.PaddingBottom + child.BorderBottomWidth + child.MarginBottom
                                  - box.ContentRect.Y;
                if (childBottom > height) height = childBottom;
            }

            if (box.LineBoxes != null)
            {
                for (int i = 0; i < box.LineBoxes.Count; i++)
                {
                    var line = box.LineBoxes[i];
                    float lineBottom = line.Y + line.Height - box.ContentRect.Y;
                    if (lineBottom > height) height = lineBottom;
                }
            }

            return height;
        }
    }
}

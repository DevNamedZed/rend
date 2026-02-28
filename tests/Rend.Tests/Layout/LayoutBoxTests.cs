using Rend.Core.Values;
using Rend.Layout;
using Xunit;

namespace Rend.Tests.Layout
{
    public class LayoutBoxTests
    {
        [Fact]
        public void Constructor_SetsStyledNodeAndBoxType()
        {
            var box = new LayoutBox(null, BoxType.Block);

            Assert.Null(box.StyledNode);
            Assert.Equal(BoxType.Block, box.BoxType);
        }

        [Fact]
        public void ContentRect_DefaultIsZero()
        {
            var box = new LayoutBox(null, BoxType.Block);

            Assert.Equal(0f, box.ContentRect.X);
            Assert.Equal(0f, box.ContentRect.Y);
            Assert.Equal(0f, box.ContentRect.Width);
            Assert.Equal(0f, box.ContentRect.Height);
        }

        [Fact]
        public void ContentRect_CanBeSet()
        {
            var box = new LayoutBox(null, BoxType.Block);
            box.ContentRect = new RectF(10f, 20f, 100f, 50f);

            Assert.Equal(10f, box.ContentRect.X);
            Assert.Equal(20f, box.ContentRect.Y);
            Assert.Equal(100f, box.ContentRect.Width);
            Assert.Equal(50f, box.ContentRect.Height);
        }

        [Fact]
        public void PaddingRect_IncludesPadding()
        {
            var box = new LayoutBox(null, BoxType.Block);
            box.ContentRect = new RectF(100f, 100f, 200f, 100f);
            box.PaddingTop = 10f;
            box.PaddingRight = 20f;
            box.PaddingBottom = 15f;
            box.PaddingLeft = 5f;

            var paddingRect = box.PaddingRect;
            Assert.Equal(95f, paddingRect.X);     // 100 - 5
            Assert.Equal(90f, paddingRect.Y);     // 100 - 10
            Assert.Equal(225f, paddingRect.Width); // 200 + 5 + 20
            Assert.Equal(125f, paddingRect.Height); // 100 + 10 + 15
        }

        [Fact]
        public void BorderRect_IncludesPaddingAndBorder()
        {
            var box = new LayoutBox(null, BoxType.Block);
            box.ContentRect = new RectF(100f, 100f, 200f, 100f);
            box.PaddingTop = 10f;
            box.PaddingRight = 10f;
            box.PaddingBottom = 10f;
            box.PaddingLeft = 10f;
            box.BorderTopWidth = 2f;
            box.BorderRightWidth = 2f;
            box.BorderBottomWidth = 2f;
            box.BorderLeftWidth = 2f;

            var borderRect = box.BorderRect;
            Assert.Equal(88f, borderRect.X);       // 100 - 10 - 2
            Assert.Equal(88f, borderRect.Y);       // 100 - 10 - 2
            Assert.Equal(224f, borderRect.Width);   // 200 + 10 + 10 + 2 + 2
            Assert.Equal(124f, borderRect.Height);  // 100 + 10 + 10 + 2 + 2
        }

        [Fact]
        public void MarginRect_IncludesPaddingBorderAndMargin()
        {
            var box = new LayoutBox(null, BoxType.Block);
            box.ContentRect = new RectF(100f, 100f, 200f, 100f);
            box.PaddingTop = 10f;
            box.PaddingRight = 10f;
            box.PaddingBottom = 10f;
            box.PaddingLeft = 10f;
            box.BorderTopWidth = 1f;
            box.BorderRightWidth = 1f;
            box.BorderBottomWidth = 1f;
            box.BorderLeftWidth = 1f;
            box.MarginTop = 20f;
            box.MarginRight = 20f;
            box.MarginBottom = 20f;
            box.MarginLeft = 20f;

            var marginRect = box.MarginRect;
            Assert.Equal(69f, marginRect.X);       // 100 - 10 - 1 - 20
            Assert.Equal(69f, marginRect.Y);       // 100 - 10 - 1 - 20
            Assert.Equal(262f, marginRect.Width);   // 200 + 10+10 + 1+1 + 20+20
            Assert.Equal(162f, marginRect.Height);  // 100 + 10+10 + 1+1 + 20+20
        }

        [Fact]
        public void AddChild_AddsToChildrenList()
        {
            var parent = new LayoutBox(null, BoxType.Block);
            var child1 = new LayoutBox(null, BoxType.Inline);
            var child2 = new LayoutBox(null, BoxType.Block);

            parent.AddChild(child1);
            parent.AddChild(child2);

            Assert.Equal(2, parent.Children.Count);
            Assert.Same(child1, parent.Children[0]);
            Assert.Same(child2, parent.Children[1]);
        }

        [Fact]
        public void AddChild_SetsParentReference()
        {
            var parent = new LayoutBox(null, BoxType.Block);
            var child = new LayoutBox(null, BoxType.Inline);

            parent.AddChild(child);

            Assert.Same(parent, child.Parent);
        }

        [Fact]
        public void Children_EmptyByDefault()
        {
            var box = new LayoutBox(null, BoxType.Block);
            Assert.Empty(box.Children);
        }

        [Fact]
        public void BoxType_CanBeChanged()
        {
            var box = new LayoutBox(null, BoxType.Block);
            box.BoxType = BoxType.Inline;

            Assert.Equal(BoxType.Inline, box.BoxType);
        }

        [Fact]
        public void PaddingRect_NoPadding_EqualsContentRect()
        {
            var box = new LayoutBox(null, BoxType.Block);
            box.ContentRect = new RectF(10f, 10f, 100f, 50f);

            var paddingRect = box.PaddingRect;
            Assert.Equal(box.ContentRect.X, paddingRect.X);
            Assert.Equal(box.ContentRect.Y, paddingRect.Y);
            Assert.Equal(box.ContentRect.Width, paddingRect.Width);
            Assert.Equal(box.ContentRect.Height, paddingRect.Height);
        }

        [Fact]
        public void ZIndex_DefaultIsZero()
        {
            var box = new LayoutBox(null, BoxType.Block);
            Assert.Equal(0f, box.ZIndex);
        }

        [Fact]
        public void EstablishesStackingContext_DefaultIsFalse()
        {
            var box = new LayoutBox(null, BoxType.Block);
            Assert.False(box.EstablishesStackingContext);
        }

        [Fact]
        public void AllBoxTypes_AreValid()
        {
            // Verify key BoxType values exist
            Assert.Equal(BoxType.Block, (BoxType)0);
            Assert.Equal(BoxType.Inline, (BoxType)1);
            Assert.Equal(BoxType.InlineBlock, (BoxType)2);
            Assert.Equal(BoxType.Flex, (BoxType)3);
        }
    }
}

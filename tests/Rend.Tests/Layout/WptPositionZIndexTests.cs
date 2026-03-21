using Rend.Layout.Internal;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptPositionZIndexTests
    {
        private readonly ITestOutputHelper _output;

        public WptPositionZIndexTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ZIndex_ParsedOnPositionedElement()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;z-index:5;width:50px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var styled = (box.StyledNode as StyledElement)!;
            Assert.Equal(5, styled.Style.ZIndex);
        }

        [Fact]
        public void ZIndex_AutoIsNaN()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='position:relative;width:50px;height:50px'></div>
                </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var styled = (box.StyledNode as StyledElement)!;
            Assert.True(float.IsNaN(styled.Style.ZIndex), "z-index:auto should be NaN");
        }

        [Fact]
        public void ZIndex_Zero_CreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;z-index:0;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "z-index:0 on positioned element should create stacking context");
        }

        [Fact]
        public void ZIndex_Auto_NoStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.False(box.EstablishesStackingContext, "z-index:auto on positioned element should not create stacking context");
        }

        [Fact]
        public void ZIndex_Negative_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;z-index:-1;width:50px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var styled = (box.StyledNode as StyledElement)!;
            Assert.Equal(-1, styled.Style.ZIndex);
        }

        [Fact]
        public void ZIndex_LargePositiveValue()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;z-index:999999;width:50px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var styled = (box.StyledNode as StyledElement)!;
            Assert.Equal(999999, styled.Style.ZIndex);
        }

        [Fact]
        public void ZIndex_LargeNegativeValue()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;z-index:-999999;width:50px;height:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var styled = (box.StyledNode as StyledElement)!;
            Assert.Equal(-999999, styled.Style.ZIndex);
        }

        [Fact]
        public void ZIndex_DoesNotAffectLayoutPosition_Absolute()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='a' style='position:absolute;top:10px;left:20px;z-index:100;width:50px;height:50px'></div>
                    <div id='b' style='position:absolute;top:10px;left:20px;z-index:1;width:50px;height:50px'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(boxA.ContentRect.X - 20) < 2, $"z-index should not affect X (got {boxA.ContentRect.X})");
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - 10) < 2, $"z-index should not affect Y (got {boxA.ContentRect.Y})");
            Assert.True(System.Math.Abs(boxB.ContentRect.X - 20) < 2, $"z-index should not affect X (got {boxB.ContentRect.X})");
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 10) < 2, $"z-index should not affect Y (got {boxB.ContentRect.Y})");
        }

        [Fact]
        public void ZIndex_DoesNotAffectLayoutPosition_Relative()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='position:relative;top:10px;left:5px;z-index:10;width:80px;height:40px'></div>
                    <div id='sibling' style='width:80px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            Assert.True(System.Math.Abs(box.ContentRect.X - 5) < 2, $"Relative left offset (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 10) < 2, $"Relative top offset (got {box.ContentRect.Y})");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 40) < 2, $"Sibling at normal flow Y (got {sibling.ContentRect.Y})");
        }

        [Fact]
        public void ZIndex_DoesNotAffectDimensions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;z-index:50;width:80px;height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2, $"Width unaffected by z-index (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2, $"Height unaffected by z-index (got {box.ContentRect.Height})");
        }

        [Fact]
        public void ZIndex_RelativePositioning_CreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='position:relative;z-index:3;width:100px;height:100px'></div>
                </body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "position:relative with z-index should create stacking context");
            Assert.Equal(3, box.ZIndex);
        }

        [Fact]
        public void ZIndex_AbsolutePositioning_CreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;z-index:7;width:100px;height:100px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "position:absolute with z-index should create stacking context");
            Assert.Equal(7, box.ZIndex);
        }

        [Fact]
        public void ZIndex_StaticPosition_NoStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='z-index:5;width:100px;height:100px'></div>
                </body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.False(box.EstablishesStackingContext, "Static position with z-index should not create stacking context");
        }

        [Fact]
        public void ZIndex_FlexItem_CreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:200px;height:100px'>
                    <div id='t' style='z-index:2;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "Flex item with z-index should create stacking context per CSS Flexbox §4");
        }

        [Fact]
        public void ZIndex_GridItem_CreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px;height:100px'>
                    <div id='t' style='z-index:1;width:50px;height:50px'></div>
                    <div style='width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "Grid item with z-index should create stacking context per CSS Grid §5.4");
        }

        [Fact]
        public void ZIndex_FlexItem_AutoZIndex_NoStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:200px;height:100px'>
                    <div id='t' style='width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.False(box.EstablishesStackingContext, "Flex item with z-index:auto should not create stacking context");
        }

        [Fact]
        public void ZIndex_DoesNotInherit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;z-index:10;width:200px;height:200px'>
                    <div id='child' style='position:absolute;width:50px;height:50px'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child")!;
            var styled = (child.StyledNode as StyledElement)!;
            Assert.True(float.IsNaN(styled.Style.ZIndex), "z-index should not inherit from parent (should be auto/NaN)");
        }

        [Fact]
        public void ZIndex_OverlappingElements_SameLayoutPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='back' style='position:absolute;top:0;left:0;z-index:1;width:100px;height:100px'></div>
                    <div id='front' style='position:absolute;top:0;left:0;z-index:10;width:100px;height:100px'></div>
                </div></body>");
            var back = LayoutTestHelper.FindById(root, "back")!;
            var front = LayoutTestHelper.FindById(root, "front")!;
            Assert.True(System.Math.Abs(back.ContentRect.X - front.ContentRect.X) < 2, "Overlapping elements have same X");
            Assert.True(System.Math.Abs(back.ContentRect.Y - front.ContentRect.Y) < 2, "Overlapping elements have same Y");
        }

        [Fact]
        public void ZIndex_PaintOrder_NegativeBeforePositive()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='pos' style='position:absolute;z-index:1;width:50px;height:50px'></div>
                    <div id='neg' style='position:absolute;z-index:-1;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var paintOrder = stackingRoot.GetPaintOrder();
            var negBox = LayoutTestHelper.FindById(root, "neg")!;
            var posBox = LayoutTestHelper.FindById(root, "pos")!;
            int negIndex = paintOrder.IndexOf(negBox);
            int posIndex = paintOrder.IndexOf(posBox);
            Assert.True(negIndex < posIndex, $"Negative z-index should paint before positive (neg={negIndex}, pos={posIndex})");
        }

        [Fact]
        public void ZIndex_PaintOrder_SortedByValue()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='z3' style='position:absolute;z-index:3;width:50px;height:50px'></div>
                    <div id='z1' style='position:absolute;z-index:1;width:50px;height:50px'></div>
                    <div id='z2' style='position:absolute;z-index:2;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var paintOrder = stackingRoot.GetPaintOrder();
            var z1Box = LayoutTestHelper.FindById(root, "z1")!;
            var z2Box = LayoutTestHelper.FindById(root, "z2")!;
            var z3Box = LayoutTestHelper.FindById(root, "z3")!;
            int idx1 = paintOrder.IndexOf(z1Box);
            int idx2 = paintOrder.IndexOf(z2Box);
            int idx3 = paintOrder.IndexOf(z3Box);
            Assert.True(idx1 < idx2, $"z-index:1 before z-index:2 (got {idx1} vs {idx2})");
            Assert.True(idx2 < idx3, $"z-index:2 before z-index:3 (got {idx2} vs {idx3})");
        }

        [Fact]
        public void ZIndex_StackingContextIsolation()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='parent1' style='position:absolute;z-index:1;width:100px;height:100px'>
                        <div id='child1' style='position:absolute;z-index:999;width:50px;height:50px'></div>
                    </div>
                    <div id='parent2' style='position:absolute;z-index:2;width:100px;height:100px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var paintOrder = stackingRoot.GetPaintOrder();
            var child1 = LayoutTestHelper.FindById(root, "child1")!;
            var parent2 = LayoutTestHelper.FindById(root, "parent2")!;
            int childIdx = paintOrder.IndexOf(child1);
            int parent2Idx = paintOrder.IndexOf(parent2);
            Assert.True(childIdx < parent2Idx,
                $"Child z-index:999 inside parent z-index:1 paints before parent2 z-index:2 due to stacking context isolation (child={childIdx}, parent2={parent2Idx})");
        }

        [Fact]
        public void ZIndex_MultipleNegative_OrderedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='z_neg2' style='position:absolute;z-index:-2;width:50px;height:50px'></div>
                    <div id='z_neg5' style='position:absolute;z-index:-5;width:50px;height:50px'></div>
                    <div id='z_neg1' style='position:absolute;z-index:-1;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var paintOrder = stackingRoot.GetPaintOrder();
            var neg5 = LayoutTestHelper.FindById(root, "z_neg5")!;
            var neg2 = LayoutTestHelper.FindById(root, "z_neg2")!;
            var neg1 = LayoutTestHelper.FindById(root, "z_neg1")!;
            int idx5 = paintOrder.IndexOf(neg5);
            int idx2 = paintOrder.IndexOf(neg2);
            int idx1 = paintOrder.IndexOf(neg1);
            Assert.True(idx5 < idx2, $"z-index:-5 before z-index:-2 (got {idx5} vs {idx2})");
            Assert.True(idx2 < idx1, $"z-index:-2 before z-index:-1 (got {idx2} vs {idx1})");
        }

        [Fact]
        public void ZIndex_FixedPosition_WithZIndex()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='position:fixed;z-index:10;top:5px;left:5px;width:50px;height:50px'></div>
                </body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            var styled = (box.StyledNode as StyledElement)!;
            Assert.Equal(10, styled.Style.ZIndex);
            Assert.True(box.EstablishesStackingContext, "Fixed position with z-index should create stacking context");
            Assert.True(System.Math.Abs(box.ContentRect.X - 5) < 2, $"Fixed X position (got {box.ContentRect.X})");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 5) < 2, $"Fixed Y position (got {box.ContentRect.Y})");
        }

        [Fact]
        public void ZIndex_FlexItem_NegativeZIndex()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:200px;height:100px'>
                    <div id='t' style='z-index:-3;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "Flex item with negative z-index should create stacking context");
            Assert.Equal(-3, box.ZIndex);
        }

        [Fact]
        public void ZIndex_GridItem_ZeroZIndex()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px;width:100px;height:100px'>
                    <div id='t' style='z-index:0;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "Grid item with z-index:0 should create stacking context");
            Assert.Equal(0, box.ZIndex);
        }

        [Fact]
        public void ZIndex_NestedStackingContexts_PaintOrder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='outer' style='position:absolute;z-index:1;width:200px;height:200px'>
                        <div id='inner_neg' style='position:absolute;z-index:-1;width:50px;height:50px'></div>
                        <div id='inner_pos' style='position:absolute;z-index:2;width:50px;height:50px'></div>
                    </div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var paintOrder = stackingRoot.GetPaintOrder();
            var outer = LayoutTestHelper.FindById(root, "outer")!;
            var innerNeg = LayoutTestHelper.FindById(root, "inner_neg")!;
            var innerPos = LayoutTestHelper.FindById(root, "inner_pos")!;
            int outerIdx = paintOrder.IndexOf(outer);
            int innerNegIdx = paintOrder.IndexOf(innerNeg);
            int innerPosIdx = paintOrder.IndexOf(innerPos);
            Assert.True(innerNegIdx < innerPosIdx, $"Negative child paints before positive child (neg={innerNegIdx}, pos={innerPosIdx})");
            Assert.True(outerIdx < innerNegIdx, $"Parent paints before its negative children (parent={outerIdx}, neg={innerNegIdx})");
        }

        [Fact]
        public void ZIndex_SameValue_DocumentOrderPreserved()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='first' style='position:absolute;z-index:1;width:50px;height:50px'></div>
                    <div id='second' style='position:absolute;z-index:1;width:50px;height:50px'></div>
                    <div id='third' style='position:absolute;z-index:1;width:50px;height:50px'></div>
                </div></body>");
            var stackingRoot = StackingContext.Build(root);
            var paintOrder = stackingRoot.GetPaintOrder();
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            int idx1 = paintOrder.IndexOf(first);
            int idx2 = paintOrder.IndexOf(second);
            int idx3 = paintOrder.IndexOf(third);
            Assert.True(idx1 < idx2, $"Same z-index: first element before second (got {idx1} vs {idx2})");
            Assert.True(idx2 < idx3, $"Same z-index: second element before third (got {idx2} vs {idx3})");
        }

        [Fact]
        public void ZIndex_OpacityCreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='opacity:0.5;width:100px;height:100px'></div>
                </body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "opacity < 1 should create stacking context");
        }

        [Fact]
        public void ZIndex_IsolationCreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='t' style='isolation:isolate;width:100px;height:100px'></div>
                </body>");
            var stackingRoot = StackingContext.Build(root);
            var box = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(box.EstablishesStackingContext, "isolation:isolate should create stacking context");
        }

        [Fact]
        public void ZIndex_RelativeOffset_WithZIndex_LayoutUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:20px'></div>
                    <div id='t' style='position:relative;top:15px;z-index:5;width:100px;height:40px'></div>
                    <div id='after' style='width:100px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            Assert.True(System.Math.Abs(box.ContentRect.Y - 35) < 2, $"Relative top:15 from flow Y=20 (got {box.ContentRect.Y})");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 60) < 2, $"Sibling at flow position ignoring relative offset (got {after.ContentRect.Y})");
        }
    }
}

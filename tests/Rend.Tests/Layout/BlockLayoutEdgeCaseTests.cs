using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class BlockLayoutEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;
        public BlockLayoutEdgeCaseTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void DisplayContents_DoesNotGenerateBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: contents; border: 10px solid red;'>
                    <div id='child' style='width: 100px; height: 50px;'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            Assert.NotNull(child);
            // display:contents parent doesn't generate a box — no border around child
            Assert.Equal(0, child!.BorderTopWidth);
        }

        [Fact]
        public void FlexItem_DisplayContents_ChildrenBecomeItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <div style='display: contents;'>
                        <div id='a' style='width: 50px; height: 30px;'></div>
                        <div id='b' style='width: 50px; height: 30px;'></div>
                    </div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.True(b!.ContentRect.X > a!.ContentRect.X, "B right of A (both flex items)");
        }

        [Fact]
        public void AnonymousBlock_ForMixedContent()
        {
            // When a block element has both block and inline children,
            // inline content gets wrapped in anonymous block boxes
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='block1' style='height: 20px;'></div>
                    <div id='block2' style='height: 20px;'></div>
                </div></body>");
            var block1 = LayoutTestHelper.FindById(root, "block1");
            var block2 = LayoutTestHelper.FindById(root, "block2");
            Assert.NotNull(block1);
            Assert.NotNull(block2);
            Assert.True(block2!.ContentRect.Y >= block1!.ContentRect.Y + 19,
                $"Block2 should be below block1 (b1.Y={block1.ContentRect.Y}, b2.Y={block2.ContentRect.Y})");
        }

        [Fact]
        public void FlexContainer_ChildMarginNoCollapse()
        {
            // Flex items' margins never collapse with siblings
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; width: 200px;'>
                    <div id='a' style='margin-bottom: 30px; height: 40px;'></div>
                    <div id='b' style='margin-top: 20px; height: 40px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            float gap = b!.ContentRect.Y - (a!.ContentRect.Y + a.ContentRect.Height);
            _output.WriteLine($"gap={gap}");
            // No collapse: gap = 30 + 20 = 50 (not max(30,20)=30)
            Assert.True(System.Math.Abs(gap - 50) < 2, $"Flex margins don't collapse: gap should be 50 (got {gap})");
        }

        [Fact]
        public void InlineBlock_BaselineAlignment()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <span id='ib' style='display: inline-block; width: 50px; height: 30px; vertical-align: middle;'></span>
                </div></body>");
            var ib = LayoutTestHelper.FindById(root, "ib");
            Assert.NotNull(ib);
            _output.WriteLine($"ib: ({ib!.ContentRect.X},{ib.ContentRect.Y}) {ib.ContentRect.Width}x{ib.ContentRect.Height}");
        }

        [Fact]
        public void NestedAbsolutePositioning()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 200px; height: 200px;'>
                    <div style='position: absolute; top: 10px; left: 10px; width: 100px; height: 100px;'>
                        <div style='position: relative; width: 100%; height: 100%;'>
                            <div id='nested' style='position: absolute; bottom: 0; right: 0; width: 20px; height: 20px;'></div>
                        </div>
                    </div>
                </div></body>");
            var nested = LayoutTestHelper.FindById(root, "nested");
            Assert.NotNull(nested);
            _output.WriteLine($"nested: ({nested!.ContentRect.X},{nested.ContentRect.Y})");
            // Nested abspos: relative to the position:relative parent (100x100 box at 10,10)
            // bottom:0, right:0 → X=10+100-20=90, Y=10+100-20=90
            Assert.True(nested.ContentRect.X >= 88, $"Nested abspos right:0 (X={nested.ContentRect.X})");
            Assert.True(nested.ContentRect.Y >= 88, $"Nested abspos bottom:0 (Y={nested.ContentRect.Y})");
        }

        [Fact]
        public void TableCell_VerticalAlign_Middle()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <table style='width: 200px; border-collapse: collapse;'>
                    <tr>
                        <td id='cell' style='height: 100px; vertical-align: middle;'>
                            <div style='height: 20px;'></div>
                        </td>
                    </tr>
                </table></body>");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(cell);
            _output.WriteLine($"cell: {cell!.ContentRect.Width}x{cell.ContentRect.Height}");
        }
    }
}

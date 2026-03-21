using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockNestingTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockNestingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BlocksStackVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='a' style='width:200px;height:40px'></div>
                <div id='b' style='width:200px;height:60px'></div>
                <div id='c' style='width:200px;height:30px'></div>
            </body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            var boxC = LayoutTestHelper.FindById(root, "c")!;
            _output.WriteLine($"a.Y={boxA.ContentRect.Y} b.Y={boxB.ContentRect.Y} c.Y={boxC.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 40) < 2);
            Assert.True(System.Math.Abs(boxC.ContentRect.Y - 100) < 2);
        }

        [Fact]
        public void NestedBlockInheritsParentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='height:20px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void DeepNestingFourLevels_WidthInherited()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div>
                        <div>
                            <div id='t' style='height:10px'></div>
                        </div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2);
        }

        [Fact]
        public void NestingWithPaddingReducesChildWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;padding:20px'>
                    <div id='t' style='height:10px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2,
                $"Child width should be parent content width 300 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void NestingWithBorderReducesChildWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;border:10px solid black'>
                    <div id='t' style='height:10px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2,
                $"Child width should be parent content width 300 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void NestingWithPaddingAndBorderReducesChildWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;padding:15px;border:5px solid black'>
                    <div id='t' style='height:10px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2,
                $"Child should fill parent content-box of 400px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void SiblingBlocksYPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='first' style='height:25px'></div>
                    <div id='second' style='height:35px'></div>
                    <div id='third' style='height:45px'></div>
                </div>
            </body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            var third = LayoutTestHelper.FindById(root, "third")!;
            _output.WriteLine($"first.Y={first.ContentRect.Y} second.Y={second.ContentRect.Y} third.Y={third.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 25) < 2);
            Assert.True(System.Math.Abs(third.ContentRect.Y - 60) < 2);
        }

        [Fact]
        public void MultipleChildrenAutoHeightSums()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:30px'></div>
                    <div style='height:50px'></div>
                </div>
            </body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MixedBlockAndInlineBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='block' style='height:40px'></div>
                    <span id='inlineBlock' style='display:inline-block;width:100px;height:30px'></span>
                </div>
            </body>");
            var block = LayoutTestHelper.FindById(root, "block")!;
            var inlineBlock = LayoutTestHelper.FindById(root, "inlineBlock")!;
            _output.WriteLine($"block.Y={block.ContentRect.Y} inlineBlock.Y={inlineBlock.ContentRect.Y}");
            Assert.True(inlineBlock.ContentRect.Y >= block.ContentRect.Y + block.ContentRect.Height - 2,
                $"inline-block should appear after block (block bottom={block.ContentRect.Y + block.ContentRect.Height}, ib.Y={inlineBlock.ContentRect.Y})");
        }

        [Fact]
        public void BlockInsideFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='width:200px'>
                        <div id='t' style='height:50px'></div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void BlockInsideGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:250px;width:400px'>
                    <div>
                        <div id='t' style='height:60px'></div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void PercentageWidthResolvesAgainstParentContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;padding:20px'>
                    <div id='t' style='width:50%;height:10px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"50% of 400px content width = 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AutoWidthFillsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:350px'>
                    <div id='t' style='height:20px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2);
        }

        [Fact]
        public void BlocksDoNotOverlap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:40px'></div>
                    <div id='b' style='height:60px'></div>
                </div>
            </body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            float bottomA = boxA.ContentRect.Y + boxA.ContentRect.Height;
            _output.WriteLine($"a bottom={bottomA} b top={boxB.ContentRect.Y}");
            Assert.True(boxB.ContentRect.Y >= bottomA - 1,
                $"Blocks should not overlap: a bottom={bottomA}, b top={boxB.ContentRect.Y}");
        }

        [Fact]
        public void DisplayNoneChildDoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='display:none;height:500px'></div>
                    <div id='t' style='height:20px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2,
                $"display:none should not take space (Y={box.ContentRect.Y})");
        }

        [Fact]
        public void VisibilityHiddenChildTakesSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='visibility:hidden;height:80px'></div>
                    <div id='t' style='height:20px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 80) < 2,
                $"visibility:hidden should still take space (Y={box.ContentRect.Y})");
        }

        [Fact]
        public void NegativeMarginOverlap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='margin-top:-20px;height:30px'></div>
                </div>
            </body>");
            var boxA = LayoutTestHelper.FindById(root, "a")!;
            var boxB = LayoutTestHelper.FindById(root, "b")!;
            float bottomA = boxA.ContentRect.Y + boxA.ContentRect.Height;
            _output.WriteLine($"a bottom={bottomA} b top={boxB.ContentRect.Y}");
            Assert.True(boxB.ContentRect.Y < bottomA,
                $"Negative margin should cause overlap: a bottom={bottomA}, b top={boxB.ContentRect.Y}");
            Assert.True(System.Math.Abs(boxB.ContentRect.Y - 30) < 2,
                $"b.Y should be 50 - 20 = 30 (got {boxB.ContentRect.Y})");
        }

        [Fact]
        public void NestedPaddingAccumulates()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;padding:10px'>
                    <div style='padding:15px'>
                        <div id='t' style='height:10px'></div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 370) < 2,
                $"Inner width = 400 - 2*15 = 370 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 25) < 2,
                $"X offset = 10 + 15 = 25 (got {box.ContentRect.X})");
        }

        [Fact]
        public void NestedBorderAccumulates()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;border:5px solid black'>
                    <div style='border:10px solid red'>
                        <div id='t' style='height:10px'></div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 380) < 2,
                $"Inner width = 400 - 2*10 = 380 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 15) < 2,
                $"X offset = 5 + 10 = 15 (got {box.ContentRect.X})");
        }

        [Fact]
        public void AutoHeightWithNestedBlocks()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div>
                        <div style='height:30px'></div>
                        <div style='height:40px'></div>
                    </div>
                </div>
            </body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 70) < 2,
                $"Auto height should sum nested children: 30+40=70 (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void ChildWithMarginAffectsParentAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='margin-top:10px;height:30px'></div>
                    <div style='margin-bottom:15px;height:20px'></div>
                </div>
            </body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"height={parent.ContentRect.Height}");
            Assert.True(parent.ContentRect.Height >= 74,
                $"Auto height should include child margins (got {parent.ContentRect.Height})");
        }

        [Fact]
        public void FixedWidthChildDoesNotExpandParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div id='child' style='width:100px;height:20px'></div>
                </div>
            </body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"parent.W={parent.ContentRect.Width} child.W={child.ContentRect.Width}");
            Assert.True(System.Math.Abs(parent.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void ChildWithMarginReducesAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='margin-left:30px;margin-right:20px;height:10px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width} X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2,
                $"Auto width = 300 - 30 - 20 = 250 (got {box.ContentRect.Width})");
            Assert.True(System.Math.Abs(box.ContentRect.X - 30) < 2,
                $"X should be margin-left=30 (got {box.ContentRect.X})");
        }

        [Fact]
        public void PercentageHeightResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:400px'>
                    <div id='t' style='height:25%'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2,
                $"25% of 400 = 100 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void BlockInsideBlockInsideFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='width:200px'>
                        <div>
                            <div id='t' style='height:30px'></div>
                        </div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"Deeply nested block in flex item should inherit width (got {box.ContentRect.Width})");
        }

        [Fact]
        public void BlockInsideBlockInsideGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:180px;width:400px'>
                    <div>
                        <div>
                            <div id='t' style='height:30px'></div>
                        </div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={box.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2,
                $"Deeply nested block in grid item should inherit track width (got {box.ContentRect.Width})");
        }

        [Fact]
        public void MultipleDisplayNoneChildrenIgnored()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='display:none;height:100px'></div>
                    <div style='height:25px'></div>
                    <div style='display:none;height:200px'></div>
                    <div style='display:none;height:300px'></div>
                    <div id='last' style='height:15px'></div>
                </div>
            </body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var last = LayoutTestHelper.FindById(root, "last")!;
            _output.WriteLine($"parent.H={parent.ContentRect.Height} last.Y={last.ContentRect.Y}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 40) < 2,
                $"Only visible children count: 25+15=40 (got {parent.ContentRect.Height})");
            Assert.True(System.Math.Abs(last.ContentRect.Y - 25) < 2,
                $"Last child Y should be 25 (got {last.ContentRect.Y})");
        }

        [Fact]
        public void NestedBlockXPositionInheritsParentContentX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;margin-left:50px'>
                    <div id='t' style='height:10px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={box.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - 50) < 2,
                $"Child X should match parent content X=50 (got {box.ContentRect.X})");
        }

        [Fact]
        public void DeepNestingFourLevelsYPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:10px'></div>
                    <div>
                        <div style='height:15px'></div>
                        <div>
                            <div id='t' style='height:20px'></div>
                        </div>
                    </div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 25) < 2,
                $"Deeply nested Y = 10 + 15 = 25 (got {box.ContentRect.Y})");
        }

        [Fact]
        public void FixedHeightParentDoesNotClipChildPositioning()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px;height:50px'>
                    <div style='height:30px'></div>
                    <div id='t' style='height:30px'></div>
                </div>
            </body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 30) < 2,
                $"Child still positioned normally even if parent has fixed height (Y={box.ContentRect.Y})");
        }
    }
}

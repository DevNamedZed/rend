using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class FlexEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;
        public FlexEdgeCaseTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void FlexBasis_Percentage_ResolvesAgainstContainer()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <div id='item' style='flex-basis: 50%; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"flex-basis 50% of 200 = 100 (got {item.ContentRect.Width})");
        }

        [Fact]
        public void FlexWrap_ReverseOrder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-wrap: wrap-reverse; width: 100px; height: 100px;'>
                    <div id='a' style='width: 60px; height: 30px;'></div>
                    <div id='b' style='width: 60px; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.Y={a!.ContentRect.Y} b.Y={b!.ContentRect.Y}");
            // wrap-reverse: first line at bottom, second line above
            Assert.True(a.ContentRect.Y > b.ContentRect.Y,
                $"wrap-reverse: A should be below B (a.Y={a.ContentRect.Y}, b.Y={b.ContentRect.Y})");
        }

        [Fact]
        public void FlexDirection_RowReverse()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: row-reverse; width: 200px;'>
                    <div id='a' style='width: 50px; height: 30px;'></div>
                    <div id='b' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.X={a!.ContentRect.X} b.X={b!.ContentRect.X}");
            // row-reverse: A at right, B to its left
            Assert.True(a.ContentRect.X > b.ContentRect.X,
                $"row-reverse: A right of B (a.X={a.ContentRect.X}, b.X={b.ContentRect.X})");
        }

        [Fact]
        public void FlexDirection_ColumnReverse()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column-reverse; width: 200px; height: 200px;'>
                    <div id='a' style='height: 50px;'></div>
                    <div id='b' style='height: 50px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.Y={a!.ContentRect.Y} b.Y={b!.ContentRect.Y}");
            // column-reverse: A at bottom, B above
            Assert.True(a.ContentRect.Y > b.ContentRect.Y,
                $"column-reverse: A below B (a.Y={a.ContentRect.Y}, b.Y={b.ContentRect.Y})");
        }

        [Fact]
        public void JustifyContent_SpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; justify-content: space-between; width: 200px;'>
                    <div id='a' style='width: 30px; height: 30px;'></div>
                    <div id='b' style='width: 30px; height: 30px;'></div>
                    <div id='c' style='width: 30px; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(a);
            Assert.NotNull(c);
            _output.WriteLine($"a.X={a!.ContentRect.X} c.X={c!.ContentRect.X}");
            // space-between: first at start, last at end
            Assert.True(a.ContentRect.X < 2, $"First item at start (got {a.ContentRect.X})");
            float expectedCX = 200 - 30; // last item at right edge
            Assert.True(System.Math.Abs(c.ContentRect.X - expectedCX) < 2,
                $"Last item at end (got {c.ContentRect.X}, expected ~{expectedCX})");
        }

        [Fact]
        public void JustifyContent_SpaceEvenly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; justify-content: space-evenly; width: 200px;'>
                    <div id='a' style='width: 40px; height: 30px;'></div>
                    <div id='b' style='width: 40px; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.X={a!.ContentRect.X} b.X={b!.ContentRect.X}");
            // space-evenly: free=200-80=120, gaps=3, each=40
            // a.X=40, b.X=40+40+40=120
            float expectedGap = 120f / 3;
            Assert.True(System.Math.Abs(a.ContentRect.X - expectedGap) < 2,
                $"space-evenly gap ~40 (a.X={a.ContentRect.X})");
        }

        [Fact]
        public void Flex_MinHeight_Zero_OverridesContent()
        {
            // min-height: 0 should allow flex items to shrink below content
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; height: 50px; width: 100px;'>
                    <div id='item' style='flex-shrink: 1; min-height: 0;'>
                        <div style='height: 200px;'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height <= 51,
                $"min-height:0 should allow shrink (got {item.ContentRect.Height})");
        }
        [Fact]
        public void FlexItem_MarginsDoNotCollapse()
        {
            // CSS Flexbox §4: margins of flex items never collapse
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; width: 200px;'>
                    <div id='box1' style='margin: 50px 0; width: 100px; height: 100px;'></div>
                    <div id='box2' style='width: 100px; height: 100px;'></div>
                </div></body>");
            var box1 = LayoutTestHelper.FindById(root, "box1");
            var box2 = LayoutTestHelper.FindById(root, "box2");
            Assert.NotNull(box1);
            Assert.NotNull(box2);
            float gap = box2!.ContentRect.Y - (box1!.ContentRect.Y + box1.ContentRect.Height);
            _output.WriteLine($"box1.Y={box1.ContentRect.Y} box2.Y={box2.ContentRect.Y} gap={gap}");
            // box1 margin-bottom(50) + box2 margin-top(0) = 50px gap (no collapse)
            // But box1 has margin: 50px 0 → margin-top:50, margin-bottom:50
            // Gap = box1 margin-bottom (50) = 50px (no margin-top on box2)
            Assert.True(gap >= 49, $"Flex margins should not collapse (gap={gap})");
        }

        [Fact]
        public void FlexItem_PositionRelative_Offsets()
        {
            // WPT flex-item-position-relative-001
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 100px; height: 100px;'>
                    <div id='item' style='width: 50px; height: 50px; position: relative; top: 50px; left: 50px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item: ({item!.ContentRect.X},{item.ContentRect.Y}) {item.ContentRect.Width}x{item.ContentRect.Height}");
            // Relative positioning should offset visually
            Assert.True(item.ContentRect.X >= 49, $"left:50px (got X={item.ContentRect.X})");
            Assert.True(item.ContentRect.Y >= 49, $"top:50px (got Y={item.ContentRect.Y})");
        }

        [Fact]
        public void FlexItem_PositionRelative_ContainingBlockForAbspos()
        {
            // position:relative flex item should be containing block for abspos children
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px; height: 100px;'>
                    <div id='rel' style='width: 100px; height: 100px; position: relative;'>
                        <div id='abs' style='position: absolute; left: 0; top: 0; width: 30px; height: 30px;'></div>
                    </div>
                </div></body>");
            var rel = LayoutTestHelper.FindById(root, "rel");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(rel);
            Assert.NotNull(abs);
            _output.WriteLine($"rel: ({rel!.ContentRect.X},{rel.ContentRect.Y})");
            _output.WriteLine($"abs: ({abs!.ContentRect.X},{abs.ContentRect.Y})");
            // Abspos child should be at (0,0) of the relative flex item
            Assert.True(System.Math.Abs(abs.ContentRect.X - rel.ContentRect.X) < 2,
                $"Abs should align with rel (abs.X={abs.ContentRect.X}, rel.X={rel.ContentRect.X})");
        }

        [Fact]
        public void FlexItem_AbsposChild_PercentPosition()
        {
            // WPT flex-item-position-relative-001: left:100% top:100% on abspos child
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 100px; height: 100px;'>
                    <div style='width: 50px; height: 50px; position: relative; top: 50px; left: 50px;'>
                        <div id='abs' style='position: absolute; left: 100%; top: 100%; width: 50px; height: 50px;'></div>
                    </div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            _output.WriteLine($"abs: ({abs!.ContentRect.X},{abs.ContentRect.Y}) {abs.ContentRect.Width}x{abs.ContentRect.Height}");
            // Parent is 50x50 at (50,50). left:100% = 50px right of parent = X=100
            // top:100% = 50px below parent = Y=100
            Assert.True(System.Math.Abs(abs.ContentRect.X - 100) < 2,
                $"left:100% of 50px parent → X=100 (got {abs.ContentRect.X})");
            Assert.True(System.Math.Abs(abs.ContentRect.Y - 100) < 2,
                $"top:100% of 50px parent → Y=100 (got {abs.ContentRect.Y})");
        }

        [Fact]
        public void ColumnWrap_MaxContentWidth()
        {
            // WPT col-wrap-001: column-wrap container with max-content width
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='flex' style='display: flex; flex-flow: column wrap; height: 100px; width: max-content;'>
                        <div style='width: 50px; flex: 0 0 100px; min-height: 0;'></div>
                        <div style='width: 50px; flex: 0 0 100px; min-height: 0;'></div>
                    </div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex: {flex!.ContentRect.Width}x{flex.ContentRect.Height}");
            // Two 50px-wide columns → max-content width = 100px
            Assert.True(System.Math.Abs(flex.ContentRect.Width - 100) < 2,
                $"max-content of column-wrap should be 100 (got {flex.ContentRect.Width})");
        }

        [Fact]
        public void FloatedFlex_ShrinkToFit_Width()
        {
            // WPT row-001: floated flex container uses min-content width
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 0;'>
                    <div id='flex' style='display: flex; height: 100px; float: left;'>
                        <div style='flex: 0 0 auto;'>
                            <div style='float: left; width: 100px;'></div>
                            <div style='float: left; width: 100px;'></div>
                        </div>
                    </div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex: {flex!.ContentRect.Width}x{flex.ContentRect.Height}");
            // Floated flex should shrink-to-fit: min-content = max(child widths) = 100
            Assert.True(flex.ContentRect.Width >= 99,
                $"Floated flex min-content should be ≥100 (got {flex.ContentRect.Width})");
        }

        [Fact]
        public void AlignContent_Center_WithOverflow()
        {
            // WPT flex-align-content-center: align-content:center with 2 wrapped lines
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; flex-wrap: wrap; align-content: center; width: 200px; height: 100px;'>
                    <div id='a' style='width: 120px; height: 30px; margin: 5px; flex: none;'></div>
                    <div id='b' style='width: 120px; height: 30px; margin: 5px; flex: none;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            _output.WriteLine($"a.Y={a!.ContentRect.Y} b.Y={b!.ContentRect.Y}");
            // Each item: 30px + 10px margin = 40px. Two lines = 80px. Container = 100px.
            // Free space = 20px. Center offset = 10px.
            // First line Y should be ~10 + margin(5) = 15
            Assert.True(a.ContentRect.Y > 5, $"Center offset should push A down (a.Y={a.ContentRect.Y})");
            Assert.True(b.ContentRect.Y > a.ContentRect.Y, $"B should be below A");
        }

        [Fact]
        public void FlexItem_Overflow_Hidden_AllowsShrink()
        {
            // overflow:hidden/auto/scroll cancels automatic minimum size
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 50px;'>
                    <div id='item' style='flex-shrink: 1; overflow: hidden;'>
                        <div style='width: 100px; height: 20px;'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.width={item!.ContentRect.Width}");
            Assert.True(item.ContentRect.Width <= 51,
                $"overflow:hidden should allow shrink (got {item.ContentRect.Width})");
        }
    }
}


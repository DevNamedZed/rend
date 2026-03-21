using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexContainerHeightValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexContainerHeightValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.4] Row auto height equals tallest of three items (30,50,40)=50
        [Fact]
        public void RowAutoHeight_TallestOfThreeItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px'>
                    <div style='width:80px;height:30px'></div>
                    <div style='width:80px;height:50px'></div>
                    <div style='width:80px;height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2,
                $"Row auto height should be tallest item 50 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Row auto height with single 60px item
        [Fact]
        public void RowAutoHeight_SingleItem60()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px'>
                    <div style='width:100px;height:60px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2,
                $"Single item height 60 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height equals sum of items (30+40+50)=120
        [Fact]
        public void ColumnAutoHeight_SumThreeItems120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:100px'>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 120) < 2,
                $"Column auto height should be sum 30+40+50=120 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with gap (30+10+40+10+50)=140
        [Fact]
        public void ColumnAutoHeight_WithGap140()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;gap:10px;width:100px'>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                    <div style='height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 140) < 2,
                $"Column auto height with gap: 30+10+40+10+50=140 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Explicit height:200 overrides auto content height
        [Fact]
        public void ExplicitHeight200_OverridesAutoContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:200px'>
                    <div style='width:50px;height:30px'></div>
                    <div style='width:50px;height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 200) < 2,
                $"Explicit height:200 should override auto (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.5] min-height:150 enforced when content is smaller
        [Fact]
        public void MinHeight150_EnforcedOverSmallerContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;min-height:150px'>
                    <div style='width:80px;height:40px'></div>
                    <div style='width:80px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 148,
                $"min-height:150 should be enforced over content 40 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.5] max-height:80 clamps column auto height 120
        [Fact]
        public void MaxHeight80_ClampsColumnAutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:200px;max-height:80px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height <= 82,
                $"max-height:80 should clamp column auto 120 (got {container.ContentRect.Height})");
        }

        // [CSS2 §10.5] Percentage height:50% of parent 400 = 200
        [Fact]
        public void PercentageHeight_50PercentOf400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='t' style='display:flex;width:200px;height:50%'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 200) < 2,
                $"50% of 400 = 200 (got {container.ContentRect.Height})");
        }

        // [CSS-VALUES §5.1.2] vh height: 50vh at viewport 300 = 150
        [Fact]
        public void VhHeight_50vhAtViewport300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:50vh'>
                    <div style='width:50px;height:20px'></div>
                </div></body>", viewportHeight: 300);
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 150) < 2,
                $"50vh of 300 = 150 (got {container.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc(100px + 50px) = 150
        [Fact]
        public void CalcHeight_SimpleAddition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:calc(100px + 50px)'>
                    <div style='width:50px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 150) < 2,
                $"calc(100px + 50px) = 150 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Empty flex container has zero height
        [Fact]
        public void AutoHeight_EmptyContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px'></div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height < 2,
                $"Empty flex container should have ~0 height (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Absolutely positioned children excluded from auto height
        [Fact]
        public void AutoHeight_AbsposChildrenExcluded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;position:relative;width:200px'>
                    <div style='width:60px;height:30px'></div>
                    <div style='position:absolute;width:60px;height:300px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 30) < 2,
                $"Abspos excluded from auto height: should be 30 not 300 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Wrap auto height = line1 + line2
        [Fact]
        public void WrapAutoHeight_TwoLines()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;width:150px'>
                    <div style='width:100px;height:35px'></div>
                    <div style='width:100px;height:45px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 80) < 2,
                $"Wrap two lines: 35+45=80 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Wrap auto height with row-gap between lines
        [Fact]
        public void WrapAutoHeight_WithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;row-gap:15px;width:150px'>
                    <div style='width:100px;height:40px'></div>
                    <div style='width:100px;height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 95) < 2,
                $"Wrap with row-gap: 40+15+40=95 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Auto height with padding: content unchanged, border rect grows
        [Fact]
        public void AutoHeight_WithPadding_ContentUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;padding:25px'>
                    <div style='width:50px;height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2,
                $"Content height unchanged at 50 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 100) < 2,
                $"Border height 50+25+25=100 (got {container.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Auto height with border: content unchanged, border rect grows
        [Fact]
        public void AutoHeight_WithBorder_ContentUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;border:8px solid red'>
                    <div style='width:50px;height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2,
                $"Content height unchanged at 50 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 66) < 2,
                $"Border height 50+8+8=66 (got {container.BorderRect.Height})");
        }

        // [CSS-BOX §4.1] border-box height:200 with padding:20 yields content 160
        [Fact]
        public void BorderBoxHeight200_Padding20_Content160()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:200px;padding:20px;box-sizing:border-box'>
                    <div style='width:50px;height:30px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 160) < 2,
                $"border-box height:200 padding:20 each side -> content=160 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 200) < 2,
                $"border-box border rect should be 200 (got {container.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Auto height with margin on items (row direction)
        [Fact]
        public void AutoHeight_WithItemMargins_Row()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px'>
                    <div style='width:80px;height:30px;margin-top:15px;margin-bottom:15px'></div>
                    <div style='width:80px;height:50px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // First item occupies 15+30+15=60 margin-box, second 50. Tallest margin-box = 60.
            Assert.True(System.Math.Abs(container.ContentRect.Height - 60) < 2,
                $"Row auto height with margins: max(15+30+15, 50)=60 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with two items
        [Fact]
        public void ColumnAutoHeight_TwoItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:100px'>
                    <div style='height:45px'></div>
                    <div style='height:55px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"Column auto height: 45+55=100 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with four items
        [Fact]
        public void ColumnAutoHeight_FourItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:100px'>
                    <div style='height:20px'></div>
                    <div style='height:25px'></div>
                    <div style='height:30px'></div>
                    <div style='height:35px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 110) < 2,
                $"Column auto height: 20+25+30+35=110 (got {container.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc(50% - 30px) of parent 400 = 170
        [Fact]
        public void CalcHeight_PercentMinusPx()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:400px'>
                    <div id='t' style='display:flex;width:200px;height:calc(50% - 30px)'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 170) < 2,
                $"calc(50% - 30px) of 400 = 170 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Row auto height: tallest with five items (10,20,50,30,40)=50
        [Fact]
        public void RowAutoHeight_TallestOfFiveItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:400px'>
                    <div style='width:60px;height:10px'></div>
                    <div style='width:60px;height:20px'></div>
                    <div style='width:60px;height:50px'></div>
                    <div style='width:60px;height:30px'></div>
                    <div style='width:60px;height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 50) < 2,
                $"Row auto height of 5 items: tallest=50 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.5] min-height larger than explicit height wins
        [Fact]
        public void MinHeight_OverridesExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:80px;min-height:150px'>
                    <div style='width:50px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height >= 148,
                $"min-height:150 should override height:80 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.5] max-height clamps auto content height
        [Fact]
        public void MaxHeight_ClampsAutoContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;max-height:60px'>
                    <div style='width:50px;height:100px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(container.ContentRect.Height <= 62,
                $"max-height:60 should clamp auto 100 (got {container.ContentRect.Height})");
        }

        // [CSS-BOX §4.1] border-box height:200 with border:10px yields content 180
        [Fact]
        public void BorderBoxHeight200_Border10_Content180()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:200px;border:10px solid blue;box-sizing:border-box'>
                    <div style='width:50px;height:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 180) < 2,
                $"border-box height:200 border:10 each -> content=180 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 200) < 2,
                $"border-box border rect should be 200 (got {container.BorderRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with margin on items (no collapse in flex)
        [Fact]
        public void ColumnAutoHeight_ItemMarginsNoCollapse()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;width:100px'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div style='height:30px;margin-top:20px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // Flex does not collapse margins: 30+20+20+30=100
            Assert.True(System.Math.Abs(container.ContentRect.Height - 100) < 2,
                $"Column flex no margin collapse: 30+20+20+30=100 (got {container.ContentRect.Height})");
        }

        // [CSS2 §10.5] Percentage height:25% of parent 600 = 150
        [Fact]
        public void PercentageHeight_25PercentOf600()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='height:600px'>
                    <div id='t' style='display:flex;width:200px;height:25%'>
                        <div style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 150) < 2,
                $"25% of 600 = 150 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Wrap three lines with row-gap
        [Fact]
        public void WrapAutoHeight_ThreeLinesWithRowGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-wrap:wrap;row-gap:10px;width:100px'>
                    <div style='width:80px;height:20px'></div>
                    <div style='width:80px;height:30px'></div>
                    <div style='width:80px;height:25px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            // 3 lines: 20 + 10 + 30 + 10 + 25 = 95
            Assert.True(System.Math.Abs(container.ContentRect.Height - 95) < 2,
                $"3 wrap lines with gap: 20+10+30+10+25=95 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Column auto height with gap and border combined
        [Fact]
        public void ColumnAutoHeight_GapAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;flex-direction:column;gap:10px;border:5px solid green;width:200px'>
                    <div style='height:25px'></div>
                    <div style='height:35px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"contentH={container!.ContentRect.Height} borderH={container.BorderRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 70) < 2,
                $"Content: 25+10+35=70 (got {container.ContentRect.Height})");
            Assert.True(System.Math.Abs(container.BorderRect.Height - 80) < 2,
                $"Border: 70+5+5=80 (got {container.BorderRect.Height})");
        }

        // [CSS-VALUES §5.1.2] 100vh at viewport 300 = 300
        [Fact]
        public void VhHeight_100vhAtViewport300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:200px;height:100vh'>
                    <div style='width:50px;height:20px'></div>
                </div></body>", viewportHeight: 300);
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 300) < 2,
                $"100vh at viewport 300 = 300 (got {container.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.4] Row auto height all same height items
        [Fact]
        public void RowAutoHeight_AllSameHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flex;width:300px'>
                    <div style='width:80px;height:40px'></div>
                    <div style='width:80px;height:40px'></div>
                    <div style='width:80px;height:40px'></div>
                </div></body>");
            var container = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(container);
            _output.WriteLine($"height={container!.ContentRect.Height}");
            Assert.True(System.Math.Abs(container.ContentRect.Height - 40) < 2,
                $"All items same height 40 (got {container.ContentRect.Height})");
        }
    }
}

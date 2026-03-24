using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS absolute positioning width and height resolution.
    /// Covers explicit sizing, inset-derived sizing, percentages,
    /// shrink-to-fit, padding/border interactions, box-sizing,
    /// calc/vw units, min/max constraints, zero dimensions,
    /// overconstrained scenarios, and flex/grid containing blocks.
    /// </summary>
    public class WptAbsposWidthHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsposWidthHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.7] Explicit width on abspos element
        [Fact]
        public void AbsPos_ExplicitWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;width:200px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Explicit width:200px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] Explicit height on abspos element
        [Fact]
        public void AbsPos_ExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:400px'>
                    <div id='t' style='position:absolute;width:50px;height:150px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2,
                $"Explicit height:150px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Width derived from left+right insets
        [Fact]
        public void AbsPos_WidthFromLeftRightInsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:30px;right:70px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width from left+right={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 2,
                $"400 - 30 - 70 = 300px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] Height derived from top+bottom insets
        [Fact]
        public void AbsPos_HeightFromTopBottomInsets()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:40px;bottom:60px;width:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height from top+bottom={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 300) < 2,
                $"400 - 40 - 60 = 300px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3] width:50% resolves against containing block width
        [Fact]
        public void AbsPos_Width50PercentOfCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:360px;height:200px'>
                    <div id='t' style='position:absolute;width:50%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width:50%={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"50% of 360px = 180px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.5] height:25% resolves against containing block height
        [Fact]
        public void AbsPos_Height25PercentOfCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:480px'>
                    <div id='t' style='position:absolute;width:50px;height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height:25%={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"25% of 480px = 120px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Shrink-to-fit auto width wraps content
        [Fact]
        public void AbsPos_ShrinkToFitAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute'>
                        <div style='width:130px;height:25px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"shrink-to-fit width={target.ContentRect.Width}");
            Assert.True(target.ContentRect.Width <= 131,
                $"Auto width should shrink to content (~130px, got {target.ContentRect.Width})");
            Assert.True(target.ContentRect.Width >= 129,
                $"Auto width should not be smaller than content (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] Auto height from content
        [Fact]
        public void AbsPos_AutoHeightFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;width:100px'>
                        <div style='height:75px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"auto height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 75) < 2,
                $"Auto height from 75px child (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Width with padding (content-box default)
        [Fact]
        public void AbsPos_WidthWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;width:160px;padding:15px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content width={target.ContentRect.Width}, padding={target.PaddingLeft}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2,
                $"Content width:160px preserved with padding (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.PaddingLeft - 15) < 2,
                $"Padding 15px applied (got {target.PaddingLeft})");
        }

        // [CSS2 §10.6.4] Height with border (content-box default)
        [Fact]
        public void AbsPos_HeightWithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;width:80px;height:100px;border:8px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"content height={target.ContentRect.Height}, border width={target.BorderRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Content height:100px preserved with border (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.BorderRect.Height - 116) < 2,
                $"Border rect height = 100 + 2*8 = 116px (got {target.BorderRect.Height})");
        }

        // [CSS-SIZING §4.1] border-box: width includes padding and border
        [Fact]
        public void AbsPos_BorderBoxWidthIncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;box-sizing:border-box;width:200px;padding:15px;border:5px solid;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var expectedContent = 200 - 2 * 15 - 2 * 5;
            _output.WriteLine($"border-box content width={target.ContentRect.Width}, expected={expectedContent}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - expectedContent) < 2,
                $"border-box: 200 - 30 - 10 = {expectedContent}px content (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.BorderRect.Width - 200) < 2,
                $"border-box: border rect = 200px (got {target.BorderRect.Width})");
        }

        // [CSS-VALUES §8.1] calc() width on abspos
        [Fact]
        public void AbsPos_CalcWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;width:calc(50% - 10px);height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"calc width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 140) < 2,
                $"calc(50% - 10px) of 300px = 140px (got {target.ContentRect.Width})");
        }

        // [CSS-VALUES §5.1.2] vw unit width on abspos
        [Fact]
        public void AbsPos_VwWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;width:25vw;height:30px'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"25vw width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"25vw with 400px viewport = 100px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.3] width:100% fills containing block
        [Fact]
        public void AbsPos_Width100PercentFillsCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:320px;height:150px'>
                    <div id='t' style='position:absolute;width:100%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width:100%={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 2,
                $"100% of 320px = 320px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.5] height:100% fills containing block
        [Fact]
        public void AbsPos_Height100PercentFillsCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:250px'>
                    <div id='t' style='position:absolute;width:50px;height:100%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height:100%={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 250) < 2,
                $"100% of 250px = 250px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.4] min-width clamps width upward
        [Fact]
        public void AbsPos_MinWidthClampsUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;width:60px;min-width:180px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"min-width width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"min-width:180px overrides width:60px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.4] max-width clamps width downward
        [Fact]
        public void AbsPos_MaxWidthClampsDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;width:300px;max-width:120px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"max-width width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2,
                $"max-width:120px clamps width:300px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.7] min-height clamps height upward
        [Fact]
        public void AbsPos_MinHeightClampsUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:400px'>
                    <div id='t' style='position:absolute;width:50px;height:30px;min-height:200px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"min-height height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2,
                $"min-height:200px overrides height:30px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.7] max-height clamps height downward
        [Fact]
        public void AbsPos_MaxHeightClampsDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:400px'>
                    <div id='t' style='position:absolute;width:50px;height:350px;max-height:90px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"max-height height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 90) < 2,
                $"max-height:90px clamps height:350px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3] width:0 produces zero-width box
        [Fact]
        public void AbsPos_WidthZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;width:0;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width:0={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width) < 2,
                $"width:0 => 0px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.5] height:0 produces zero-height box
        [Fact]
        public void AbsPos_HeightZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;width:50px;height:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height:0={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height) < 2,
                $"height:0 => 0px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Overconstrained: left + right + width in LTR, right is ignored
        [Fact]
        public void AbsPos_OverconstrainedLeftRightWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:20px;right:30px;width:150px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"overconstrained width={target.ContentRect.Width}, x={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"Explicit width:150px wins (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"left:20px applied, right ignored in LTR (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] Overconstrained: top + bottom + height, bottom is ignored
        [Fact]
        public void AbsPos_OverconstrainedTopBottomHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:25px;bottom:35px;width:50px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"overconstrained height={target.ContentRect.Height}, y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Explicit height:100px wins (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2,
                $"top:25px applied, bottom ignored (got {target.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §4.1] Abspos width in flex containing block
        [Fact]
        public void AbsPos_WidthInFlexCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:350px;height:200px'>
                    <div id='t' style='position:absolute;width:50%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"flex CB width:50%={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 175) < 2,
                $"50% of 350px flex CB = 175px (got {target.ContentRect.Width})");
        }

        // [CSS-GRID §6.1] Abspos width in grid containing block
        [Fact]
        public void AbsPos_WidthInGridCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:280px;height:200px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;width:50%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"grid CB width:50%={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 140) < 2,
                $"50% of 280px grid CB = 140px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] Height from top+bottom with padding on CB
        [Fact]
        public void AbsPos_HeightFromInsetsWithPaddedCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:20px'>
                    <div id='t' style='position:absolute;top:10px;bottom:10px;width:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var expectedHeight = 240 - 10 - 10;
            _output.WriteLine($"padded CB height from insets={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedHeight) < 2,
                $"CB padding box 240px - 10 - 10 = {expectedHeight}px (got {target.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc() height on abspos
        [Fact]
        public void AbsPos_CalcHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;width:50px;height:calc(25% + 30px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"calc height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 130) < 2,
                $"calc(25% + 30px) of 400px = 130px (got {target.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] Nested calc() on abspos height with no positioned ancestor
        [Fact]
        public void AbsPos_NestedCalcHeight_ViewportFallback()
        {
            var root = LayoutTestHelper.Layout(@"
                <html><head><style>
                html, body { margin: 0; padding: 0; }
                html { overflow: hidden; }
                #outer { position: absolute; top: 0; left: 0; width: 100%; height: calc(calc(100%)); }
                </style></head><body><div id='outer'></div></body></html>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "outer")!;
            _output.WriteLine($"nested calc height={target.ContentRect.Height}");
            // Without a positioned ancestor, % height should resolve against viewport (600px)
            Assert.True(target.ContentRect.Height >= 590,
                $"calc(calc(100%)) of viewport 600px should be ~600px (got {target.ContentRect.Height})");
        }

        // [CSS-VALUES §6.3] calc() mixing viewport + percentage units on abspos
        [Fact]
        public void AbsPos_VhCalcPlusPct_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"
                <html><head><style>
                html { background: red; }
                #target { position: absolute; background: green;
                    width: calc(100vw + 50%); height: calc(100vh + 50%);
                    top: -50%; left: -50%; }
                </style></head><body><div id='target'></div></body></html>", 800, 600);
            var target = LayoutTestHelper.FindById(root, "target")!;
            _output.WriteLine($"vh+pct: w={target.ContentRect.Width} h={target.ContentRect.Height} x={target.ContentRect.X} y={target.ContentRect.Y}");
            // width = calc(100vw + 50%) should be 800+400=1200 (or close)
            // height = calc(100vh + 50%) should be 600+300=900
            Assert.True(target.ContentRect.Height >= 850,
                $"calc(100vh + 50%) should be ~900px (got {target.ContentRect.Height})");
            Assert.True(target.ContentRect.Width >= 1100,
                $"calc(100vw + 50%) should be ~1200px (got {target.ContentRect.Width})");
        }

        // [CSS-SIZING §4.1] border-box height includes padding and border
        [Fact]
        public void AbsPos_BorderBoxHeightIncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;box-sizing:border-box;width:100px;height:180px;padding:20px;border:10px solid'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var expectedContent = 180 - 2 * 20 - 2 * 10;
            _output.WriteLine($"border-box content height={target.ContentRect.Height}, expected={expectedContent}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedContent) < 2,
                $"border-box: 180 - 40 - 20 = {expectedContent}px content (got {target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.BorderRect.Height - 180) < 2,
                $"border-box: border rect height = 180px (got {target.BorderRect.Height})");
        }

        // [CSS2 §10.4] min-width larger than max-width: min-width wins
        [Fact]
        public void AbsPos_MinWidthLargerThanMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;width:100px;min-width:200px;max-width:150px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"min > max width={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"min-width:200px wins over max-width:150px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.7] min-height larger than max-height: min-height wins
        [Fact]
        public void AbsPos_MinHeightLargerThanMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:400px'>
                    <div id='t' style='position:absolute;width:50px;height:80px;min-height:250px;max-height:180px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"min > max height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 250) < 2,
                $"min-height:250px wins over max-height:180px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Width from insets with border-box
        [Fact]
        public void AbsPos_WidthFromInsetsWithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;box-sizing:border-box;left:20px;right:30px;padding:10px;border:5px solid;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var borderWidth = 400 - 20 - 30;
            var contentWidth = borderWidth - 2 * 10 - 2 * 5;
            _output.WriteLine($"insets + border-box: content={target.ContentRect.Width}, border={target.BorderRect.Width}");
            Assert.True(System.Math.Abs(target.BorderRect.Width - borderWidth) < 2,
                $"Border rect = 400 - 20 - 30 = {borderWidth}px (got {target.BorderRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - contentWidth) < 2,
                $"Content = {borderWidth} - 20 - 10 = {contentWidth}px (got {target.ContentRect.Width})");
        }

        // [CSS-VALUES §5.1.2] vh unit height on abspos
        [Fact]
        public void AbsPos_VhHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;width:50px;height:20vh'></div>
                </div></body>", 400, 500);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"20vh height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"20vh with 500px viewport = 100px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] Auto width with multiple child blocks uses widest
        [Fact]
        public void AbsPos_AutoWidthFromMultipleChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:300px'>
                    <div id='t' style='position:absolute'>
                        <div style='width:90px;height:20px'></div>
                        <div style='width:145px;height:20px'></div>
                        <div style='width:110px;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"auto width from children={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 145) < 2,
                $"Shrink-to-fit uses widest child 145px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] Auto height from multiple child blocks sums heights
        [Fact]
        public void AbsPos_AutoHeightFromMultipleChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div id='t' style='position:absolute;width:100px'>
                        <div style='height:30px'></div>
                        <div style='height:45px'></div>
                        <div style='height:25px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"auto height from children={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Auto height = 30 + 45 + 25 = 100px (got {target.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §4.1] Abspos height from top+bottom in flex CB
        [Fact]
        public void AbsPos_HeightFromInsetsInFlexCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:300px;height:250px'>
                    <div id='t' style='position:absolute;top:20px;bottom:30px;width:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"flex CB height from insets={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2,
                $"250 - 20 - 30 = 200px (got {target.ContentRect.Height})");
        }

        // [CSS-GRID §6.1] Abspos height from top+bottom in grid CB
        [Fact]
        public void AbsPos_HeightFromInsetsInGridCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;position:relative;width:200px;height:300px;grid-template-columns:1fr'>
                    <div id='t' style='position:absolute;top:15px;bottom:25px;width:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"grid CB height from insets={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 260) < 2,
                $"300 - 15 - 25 = 260px (got {target.ContentRect.Height})");
        }
    }
}

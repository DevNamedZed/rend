using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS absolute positioning with percentage values.
    /// Covers inset percentages, percentage sizing, containing block
    /// interactions with padding/border, fixed position viewport
    /// percentages, calc with percent, min/max percent, nesting,
    /// and margin interactions.
    /// </summary>
    public class WptAbsposPercentTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsposPercentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.6.2] top:25% resolves against CB height
        [Fact]
        public void AbsPos_Top25Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:25%;left:0;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top:25% => Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2,
                $"25% of 200px = 50px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] left:50% resolves against CB width
        [Fact]
        public void AbsPos_Left50Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:50%;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"left:50% => X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 150) < 2,
                $"50% of 300px = 150px (got {target.ContentRect.X})");
        }

        // [CSS2 §10.3.7] right:25% resolves against CB width
        [Fact]
        public void AbsPos_Right25Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;top:0;right:25%;width:60px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // right:25% = 100px from right edge. X = 400 - 100 - 60 = 240
            _output.WriteLine($"right:25% => X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 240) < 2,
                $"right:25% of 400px with 60px width => X=240 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] bottom:10% resolves against CB height
        [Fact]
        public void AbsPos_Bottom10Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;left:0;bottom:10%;width:50px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // bottom:10% = 20px from bottom. Y = 200 - 20 - 30 = 150
            _output.WriteLine($"bottom:10% => Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 150) < 2,
                $"bottom:10% of 200px with 30px height => Y=150 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3] width:50% resolves against CB width
        [Fact]
        public void AbsPos_Width50Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:240px;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"width:50% => W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2,
                $"50% of 240px = 120px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.5] height:25% resolves against CB height
        [Fact]
        public void AbsPos_Height25Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:100px;height:400px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height:25% => H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"25% of 400px = 100px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7, §10.6.4] top:50% left:50% center offset
        [Fact]
        public void AbsPos_Top50Left50_CenterOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:50%;left:50%;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top:50% left:50% => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2,
                $"left:50% of 200px = 100px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2,
                $"top:50% of 200px = 100px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] percent insets resolve against different CB sizes
        [Fact]
        public void AbsPos_PercentOfSmallCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:80px;height:60px'>
                    <div id='t' style='position:absolute;top:50%;left:25%;width:20px;height:10px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"small CB => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2,
                $"25% of 80px = 20px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"50% of 60px = 30px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] percent insets with larger CB
        [Fact]
        public void AbsPos_PercentOfLargeCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:500px;height:400px'>
                    <div id='t' style='position:absolute;top:10%;left:20%;width:30px;height:30px'></div>
                </div></body>", 600, 500);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"large CB => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2,
                $"20% of 500px = 100px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2,
                $"10% of 400px = 40px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos CB is padding box, percent resolves against padding box
        [Fact]
        public void AbsPos_PercentWithPaddingOnCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:20px'>
                    <div id='t' style='position:absolute;top:25%;left:25%;width:30px;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // CB padding box = 240x240. 25% of 240 = 60
            _output.WriteLine($"padded CB => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 60) < 2,
                $"25% of 240px padding-box = 60px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 60) < 2,
                $"25% of 240px padding-box = 60px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos CB is padding box; border does NOT increase CB
        [Fact]
        public void AbsPos_PercentWithBorderOnCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;border:10px solid black'>
                    <div id='t' style='position:absolute;top:50%;left:50%;width:20px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // CB padding box = 200x200 (border outside). 50% of 200 = 100
            // Position relative to padding edge, offset from border edge by border width
            _output.WriteLine($"bordered CB => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 110) < 2,
                $"50% of 200px padding-box + 10px border = 110px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 110) < 2,
                $"50% of 200px padding-box + 10px border = 110px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.1] abspos CB with both padding and border
        [Fact]
        public void AbsPos_PercentWithPaddingAndBorderOnCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:30px;border:10px solid black'>
                    <div id='t' style='position:absolute;top:0;left:50%;width:20px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // CB padding box = (200+60)x(200+60) = 260x260. 50% of 260 = 130
            // X = border-left(10) + 130 = 140
            _output.WriteLine($"padded+bordered CB => X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 140) < 2,
                $"50% of 260px padding-box + 10px border = 140px (got {target.ContentRect.X})");
        }

        // [CSS2 §9.6.1] fixed position: percent resolves against viewport
        [Fact]
        public void FixedPos_PercentResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:25%;left:50%;width:40px;height:40px'></div>
                </body>", 400, 200);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"fixed => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2,
                $"left:50% of 400px viewport = 200px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2,
                $"top:25% of 200px viewport = 50px (got {target.ContentRect.Y})");
        }

        // [CSS2 §9.6.1] fixed position: percent width/height against viewport
        [Fact]
        public void FixedPos_PercentSizeAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:50%;height:25%'></div>
                </body>", 400, 200);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"fixed size => ({target.ContentRect.Width}x{target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"50% of 400px viewport = 200px (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"25% of 200px viewport = 50px (got {target.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc() with percentage in abspos height
        [Fact]
        public void AbsPos_CalcWithPercent_Height()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:40px;height:calc(25% + 20px)'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 25% of 400 = 100. 100 + 20 = 120
            _output.WriteLine($"calc(25% + 20px) => H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"calc(25% + 20px) of 400px = 120px (got {target.ContentRect.Height})");
        }

        // [CSS-VALUES §8.1] calc() with percentage in abspos width
        [Fact]
        public void AbsPos_CalcWithPercent_Width()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:calc(50% + 30px);height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // 50% of 300 = 150. 150 + 30 = 180
            _output.WriteLine($"calc(50% + 30px) => W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2,
                $"calc(50% + 30px) of 300px = 180px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.4] min-width percent in abspos
        [Fact]
        public void AbsPos_MinWidthPercent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;min-width:25%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // min-width:25% of 400 = 100. width:50px < 100, so resolved = 100
            _output.WriteLine($"min-width:25% => W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"min-width:25% of 400px = 100px, overrides 50px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.4] max-width percent in abspos
        [Fact]
        public void AbsPos_MaxWidthPercent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:300px;max-width:25%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // max-width:25% of 400 = 100. width:300px > 100, so resolved = 100
            _output.WriteLine($"max-width:25% => W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"max-width:25% of 400px = 100px, clamps 300px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.5] percentage height requires explicit CB height
        [Fact]
        public void AbsPos_PercentHeight_ExplicitCBHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:50%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"height:50% of 300px => H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2,
                $"50% of 300px = 150px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3] percentage width always resolves (even without explicit CB width)
        [Fact]
        public void AbsPos_PercentWidth_AlwaysResolves()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:25%;height:40px'></div>
                </div></body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            // CB width is body width = viewport 400. 25% of 400 = 100
            _output.WriteLine($"width:25% auto CB => W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"25% of 400px viewport = 100px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.1] nested percentage in abspos contexts
        [Fact]
        public void AbsPos_NestedPercentage()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='position:absolute;top:0;left:0;width:50%;height:50%'>
                        <div style='position:relative;width:100%;height:100%'>
                            <div id='t' style='position:absolute;top:0;left:0;width:50%;height:50%'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // Outer abspos: 50% of 400 = 200x200
            // Inner relative: 100% of 200 = 200x200
            // Target abspos: 50% of 200 = 100x100
            _output.WriteLine($"nested => ({target.ContentRect.Width}x{target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"50% of 50% of 400px = 100px (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"50% of 50% of 400px = 100px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] percent insets with margin
        [Fact]
        public void AbsPos_PercentInsets_WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:10%;left:10%;margin:10px;width:40px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // top:10% = 20, left:10% = 20, margin:10px
            // X = 20 + 10 = 30, Y = 20 + 10 = 30
            _output.WriteLine($"percent insets + margin => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 30) < 2,
                $"left:10% + margin:10px = 30px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2,
                $"top:10% + margin:10px = 30px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7, §10.6.4] 100% width/height fills CB
        [Fact]
        public void AbsPos_100Percent_FillsCB()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:250px;height:180px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:100%;height:100%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"100% fill => ({target.ContentRect.Width}x{target.ContentRect.Height})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 2,
                $"100% of 250px = 250px (got {target.ContentRect.Width})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2,
                $"100% of 180px = 180px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.5] percent height with auto CB height resolves for abspos
        [Fact]
        public void AbsPos_PercentHeight_AutoCBHeight_ResolvesFromContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:50%'></div>
                    <div style='height:160px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // CB auto height determined by content (160px). 50% of 160 = 80
            _output.WriteLine($"percent height auto CB => H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"50% of auto-height 160px = 80px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] percentage width with left+right overconstrained
        [Fact]
        public void AbsPos_PercentWidth_WithLeftRight_Overconstrained()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;top:0;left:10%;right:10%;width:50%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // left:10% = 40, width:50% = 200. right is overconstrained (ignored in LTR).
            // X = 40
            _output.WriteLine($"overconstrained => X={target.ContentRect.X}, W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2,
                $"left:10% of 400px = 40px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"width:50% of 400px = 200px (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.6.4] percent top with percent height
        [Fact]
        public void AbsPos_PercentTop_PercentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:25%;left:0;width:50px;height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"top:25% height:25% => Y={target.ContentRect.Y}, H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2,
                $"top:25% of 200px = 50px (got {target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"height:25% of 200px = 50px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.4] min-height percent in abspos
        [Fact]
        public void AbsPos_MinHeightPercent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:20px;min-height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // min-height:25% of 400 = 100. height:20px < 100, so resolved = 100
            _output.WriteLine($"min-height:25% => H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"min-height:25% of 400px = 100px, overrides 20px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.4] max-height percent in abspos
        [Fact]
        public void AbsPos_MaxHeightPercent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:300px;max-height:25%'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // max-height:25% of 400 = 100. height:300px > 100, so resolved = 100
            _output.WriteLine($"max-height:25% => H={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"max-height:25% of 400px = 100px, clamps 300px (got {target.ContentRect.Height})");
        }

        // [CSS2 §10.1] abspos percent resolves against non-static ancestor
        [Fact]
        public void AbsPos_PercentSkipsStaticAncestors()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div style='width:100px;height:100px'>
                        <div id='t' style='position:absolute;top:10%;left:10%;width:40px;height:40px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // CB is the position:relative 400x400 ancestor, not the static 100x100 parent
            // 10% of 400 = 40
            _output.WriteLine($"skips static => ({target.ContentRect.X},{target.ContentRect.Y})");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2,
                $"10% of 400px CB = 40px (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2,
                $"10% of 400px CB = 40px (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] percent width with auto left and right
        [Fact]
        public void AbsPos_PercentWidth_AutoLeftRight_DefaultsToStaticPos()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;top:0;width:50%;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            // width:50% of 300 = 150. auto left defaults to static position (0).
            _output.WriteLine($"width:50% auto left/right => W={target.ContentRect.Width}, X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2,
                $"50% of 300px = 150px (got {target.ContentRect.Width})");
            Assert.True(target.ContentRect.X < 2,
                $"auto left defaults to static position near 0 (got {target.ContentRect.X})");
        }
    }
}

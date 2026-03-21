using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS absolute positioning interaction with margins:
    /// auto centering, offset addition, overconstrained resolution,
    /// negative margins, percentage margins, and non-collapsing behavior.
    /// </summary>
    public class WptAbsposMarginTests
    {
        private readonly ITestOutputHelper _output;

        public WptAbsposMarginTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS2 §10.3.7] margin on abspos adds to inset offset
        [Fact]
        public void MarginAddsToOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:10px;left:20px;margin:15px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 35) < 2, $"left:20+margin-left:15=35 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 25) < 2, $"top:10+margin-top:15=25 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] margin:auto horizontal centering with left:0 right:0
        [Fact]
        public void MarginAutoHorizontalCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin-left:auto;margin-right:auto;width:200px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2, $"centered at 100 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] margin:auto vertical centering with top:0 bottom:0
        [Fact]
        public void MarginAutoVerticalCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin-top:auto;margin-bottom:auto;width:50px;height:200px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2, $"centered at 100 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7+10.6.4] margin:auto both axes centers both
        [Fact]
        public void MarginAutoBothAxesCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div id='t' style='position:absolute;top:0;right:0;bottom:0;left:0;margin:auto;width:100px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2, $"centered X at 100 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2, $"centered Y at 100 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] margin-left:auto pushes element to the right
        [Fact]
        public void MarginLeftAutoPushesRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin-left:auto;margin-right:0;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2, $"pushed right to 200 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.3.7] margin-right:auto pushes element to the left
        [Fact]
        public void MarginRightAutoPushesLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin-left:0;margin-right:auto;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 0) < 2, $"pushed left to 0 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] margin-top:auto pushes element down
        [Fact]
        public void MarginTopAutoPushesDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin-top:auto;margin-bottom:0;width:50px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 200) < 2, $"pushed down to 200 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.6.4] margin-bottom:auto pushes element up
        [Fact]
        public void MarginBottomAutoPushesUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin-top:0;margin-bottom:auto;width:50px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 0) < 2, $"pushed up to 0 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] negative margin on abspos shifts position
        [Fact]
        public void NegativeMarginShiftsPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div id='t' style='position:absolute;top:50px;left:50px;margin-top:-20px;margin-left:-10px;width:60px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2, $"left:50-margin:10=40 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2, $"top:50-margin:20=30 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] negative margin can push abspos outside CB
        [Fact]
        public void NegativeMarginOutsideContainingBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;margin-left:-30px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            Assert.True(target.ContentRect.X < 0, $"should be negative X (got {target.ContentRect.X})");
        }

        // [CSS2 §10.3.7] percentage margin resolves against CB width
        [Fact]
        public void PercentageMarginResolvesAgainstCbWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;margin-left:10%;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2, $"10% of 400=40 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.3.7] percentage margin-top also resolves against CB width (per spec)
        [Fact]
        public void PercentageMarginTopResolvesAgainstCbWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;top:0;left:0;margin-top:10%;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 40) < 2, $"margin-top 10% of CB width 400=40 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] overconstrained left+right+width: margin-left:auto absorbs slack
        [Fact]
        public void OverconstrainedMarginLeftAutoAbsorbsSlack()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:10px;right:20px;width:100px;margin-left:auto;margin-right:0;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} W={target.ContentRect.Width}");
            // CB=300, left=10, right=20, width=100, margin-right=0 => margin-left=300-10-20-100-0=170
            Assert.True(System.Math.Abs(target.ContentRect.X - 180) < 2, $"X should be 10+170=180 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2, $"width stays 100 (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.3.7] overconstrained left+right+width+margins: right is ignored in LTR
        [Fact]
        public void OverconstrainedAllSetRightIgnoredLtr()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:10px;right:20px;width:100px;margin-left:5px;margin-right:5px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} W={target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 15) < 2, $"left:10+margin-left:5=15 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2, $"width stays 100 (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.3.7] margin:auto on fixed position centers in viewport
        [Fact]
        public void MarginAutoFixedPositionCenters()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;right:0;bottom:0;left:0;margin:auto;width:200px;height:100px'></div>
            </body>", 400, 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} Y={target.ContentRect.Y}");
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2, $"centered X in 400 viewport (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2, $"centered Y in 300 viewport (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] margin on abspos with padding on the element
        [Fact]
        public void MarginWithPaddingOnAbspos()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin:0 auto;padding:10px;width:100px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} W={target.ContentRect.Width} PL={target.PaddingLeft}");
            // total box width = 100 + 10 + 10 = 120. margin-left = (400-120)/2 = 140
            Assert.True(System.Math.Abs(target.ContentRect.X - 150) < 2, $"content X=140+10=150 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2, $"content width stays 100 (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.3.7] margin on abspos with border on the element
        [Fact]
        public void MarginWithBorderOnAbspos()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin:0 auto;border:5px solid black;width:100px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} W={target.ContentRect.Width} BL={target.BorderLeftWidth}");
            // total box width = 100 + 5 + 5 = 110. margin-left = (400-110)/2 = 145
            Assert.True(System.Math.Abs(target.ContentRect.X - 150) < 2, $"content X=145+5=150 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2, $"content width stays 100 (got {target.ContentRect.Width})");
        }

        // [CSS3 box-sizing] margin on abspos with border-box sizing
        [Fact]
        public void MarginWithBorderBoxSizing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin:0 auto;box-sizing:border-box;border:5px solid black;padding:10px;width:120px;height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} W={target.ContentRect.Width}");
            // border-box: total box = 120. margin-left = (400-120)/2 = 140. content X = 140+5+10=155
            Assert.True(System.Math.Abs(target.ContentRect.X - 155) < 2, $"content X=155 (got {target.ContentRect.X})");
            // content width = 120 - 5 - 5 - 10 - 10 = 90
            Assert.True(System.Math.Abs(target.ContentRect.Width - 90) < 2, $"content width=90 (got {target.ContentRect.Width})");
        }

        // [CSS2 §8.3.1] margins on abspos do NOT collapse
        [Fact]
        public void MarginDoesNotCollapseOnAbspos()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:300px'>
                    <div style='height:50px;margin-bottom:30px'></div>
                    <div id='t' style='position:absolute;top:0;margin-top:20px;width:50px;height:50px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // abspos margin-top:20 is from CB top, not collapsed with sibling
            Assert.True(System.Math.Abs(target.ContentRect.Y - 20) < 2, $"margin should not collapse (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] margin:auto with only left set (right is auto)
        [Fact]
        public void MarginAutoWithOnlyLeftSet()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:20px;margin-left:auto;width:80px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // When right is auto and margin-left is auto: margin-left resolves to 0 (CSS2 §10.3.7)
            // Element positioned at left:20
            Assert.True(System.Math.Abs(target.ContentRect.X - 20) < 2, $"margin-left auto→0 when right auto (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] margin:auto with only top set (bottom is auto)
        [Fact]
        public void MarginAutoWithOnlyTopSet()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:30px;margin-top:auto;width:50px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // When bottom is auto and margin-top is auto: margin-top resolves to 0 (CSS2 §10.6.4)
            Assert.True(System.Math.Abs(target.ContentRect.Y - 30) < 2, $"margin-top auto→0 when bottom auto (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] large margin-left reduces available content width with left+right
        [Fact]
        public void LargeMarginReducesAutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin-left:50px;margin-right:50px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} W={target.ContentRect.Width}");
            // width = 300 - 0 - 0 - 50 - 50 = 200
            Assert.True(System.Math.Abs(target.ContentRect.X - 50) < 2, $"X=margin-left=50 (got {target.ContentRect.X})");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2, $"width=200 (got {target.ContentRect.Width})");
        }

        // [CSS2 §10.3.7] margin:auto asymmetric — one side auto, one fixed
        [Fact]
        public void MarginAutoAsymmetric()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin-left:auto;margin-right:50px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // margin-left = 400 - 0 - 0 - 100 - 50 = 250
            Assert.True(System.Math.Abs(target.ContentRect.X - 250) < 2, $"margin-left auto absorbs 250 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] vertical margin:auto asymmetric
        [Fact]
        public void VerticalMarginAutoAsymmetric()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin-top:auto;margin-bottom:50px;width:50px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // margin-top = 400 - 0 - 0 - 100 - 50 = 250
            Assert.True(System.Math.Abs(target.ContentRect.Y - 250) < 2, $"margin-top auto absorbs 250 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] margin on abspos with right offset
        [Fact]
        public void MarginWithRightOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;right:20px;margin-right:10px;width:80px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // X = 300 - 20 - 10 - 80 = 190
            Assert.True(System.Math.Abs(target.ContentRect.X - 190) < 2, $"right:20+margin-right:10 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] margin on abspos with bottom offset
        [Fact]
        public void MarginWithBottomOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;bottom:20px;margin-bottom:10px;width:50px;height:60px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // Y = 300 - 20 - 10 - 60 = 210
            Assert.True(System.Math.Abs(target.ContentRect.Y - 210) < 2, $"bottom:20+margin-bottom:10 (got {target.ContentRect.Y})");
        }

        // [CSS2 §10.3.7] margin:auto on abspos inside padded CB
        [Fact]
        public void MarginAutoCenterInPaddedCb()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:200px;padding:20px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin:0 auto;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X}");
            // CB padding box = 340. margin-left = (340-100)/2 = 120
            Assert.True(System.Math.Abs(target.ContentRect.X - 120) < 2, $"centered in padded CB at 120 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.3.7] abspos child margins don't affect parent height
        [Fact]
        public void AbsposMarginDoesNotAffectParentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='position:relative;width:200px'>
                    <div style='height:50px'></div>
                    <div style='position:absolute;margin-top:100px;width:50px;height:200px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parentH={parent.ContentRect.Height}");
            // Parent auto height only considers in-flow children (50px), not abspos
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2, $"parent height=50 (got {parent.ContentRect.Height})");
        }

        // [CSS2 §10.3.7] margin with padding and border combined on centered abspos
        [Fact]
        public void MarginAutoCenterWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin:0 auto;border:5px solid black;padding:10px;width:100px;height:40px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"X={target.ContentRect.X} W={target.ContentRect.Width}");
            // total box = 100 + 10 + 10 + 5 + 5 = 130. margin-left = (400-130)/2 = 135. content X = 135+5+10 = 150
            Assert.True(System.Math.Abs(target.ContentRect.X - 150) < 2, $"content X=150 (got {target.ContentRect.X})");
        }

        // [CSS2 §10.6.4] vertical margin:auto with padding and border
        [Fact]
        public void VerticalMarginAutoCenterWithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:400px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin:auto 0;border:5px solid black;padding:10px;width:50px;height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={target.ContentRect.Y}");
            // total box height = 100 + 10 + 10 + 5 + 5 = 130. margin-top = (400-130)/2 = 135. content Y = 135+5+10 = 150
            Assert.True(System.Math.Abs(target.ContentRect.Y - 150) < 2, $"content Y=150 (got {target.ContentRect.Y})");
        }
    }
}

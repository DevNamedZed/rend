using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptAbsoluteEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;
        public WptAbsoluteEdgeCaseTests(ITestOutputHelper output) { _output = output; }

        // abspos: inset:0 + margin:auto = centered both axes
        [Fact]
        public void Inset0_MarginAuto_Centered()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div id='t' style='position:absolute;inset:0;margin:auto;width:200px;height:200px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 100) < 2);
        }

        // abspos: top:50% left:50% centers at midpoint (not visually centered)
        [Fact]
        public void Percent50_50_AtMidpoint()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:50%;left:50%;width:40px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 100) < 2);
        }

        // abspos: only top set, left defaults to static position
        [Fact]
        public void OnlyTop_LeftIsStatic()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='height:50px'></div>
                    <div id='t' style='position:absolute;top:10px;width:40px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 10) < 2);
        }

        // abspos: negative right overflows
        [Fact]
        public void NegativeRight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;right:-50px;top:0;width:40px;height:40px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X > 200);
        }

        // abspos: width from left+right with padding on CB
        [Fact]
        public void WidthFromInsets_PaddedCB()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px;padding:30px'>
                    <div id='t' style='position:absolute;left:10px;right:10px;height:40px'></div>
                </div></body>");
            // CB padding box = 260x160. width = 260-10-10 = 240.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 240) < 2);
        }

        // abspos: height from top+bottom
        [Fact]
        public void HeightFromInsets()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:20px;bottom:30px;width:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 250) < 2);
        }

        // abspos: percentage height resolves against CB
        [Fact]
        public void PercentHeight_AgainstCB()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px'>
                    <div id='abs' style='position:absolute;width:50px;height:25%'></div>
                    <div style='height:400px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "abs")!.ContentRect.Height - 100) < 2);
        }

        // abspos: auto margins distribute space horizontally
        [Fact]
        public void AutoMargins_HorizontalCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;margin:0 auto;width:100px;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 100) < 2);
        }

        // abspos: auto margins distribute space vertically
        [Fact]
        public void AutoMargins_VerticalCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:300px'>
                    <div id='t' style='position:absolute;top:0;bottom:0;margin:auto 0;width:50px;height:100px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 100) < 2);
        }

        // abspos: over-constrained horizontal (left+right+width), right ignored in LTR
        [Fact]
        public void OverConstrained_Horizontal()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:300px;height:100px'>
                    <div id='t' style='position:absolute;left:10px;right:20px;width:100px;height:40px'></div>
                </div></body>");
            // LTR: left wins, right ignored
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // abspos doesn't affect sibling positions
        [Fact]
        public void AbsPos_NoSiblingEffect()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px'>
                    <div style='height:30px'></div>
                    <div style='position:absolute;height:500px'></div>
                    <div id='sib' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "sib")!.ContentRect.Y - 30) < 2);
        }

        // abspos: display:table with percentage dimensions
        [Fact]
        public void AbsPos_Table_PercentDimensions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px'>
                    <div id='t' style='position:absolute;display:table;width:100%;height:100%'></div>
                    <div style='height:150px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 150) < 2);
        }

        // fixed position: percentage width against viewport
        [Fact]
        public void Fixed_PercentWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:50%;height:30px'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // fixed position: percentage height against viewport
        [Fact]
        public void Fixed_PercentHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;top:0;left:0;width:50px;height:50%'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 150) < 2);
        }
    }
}

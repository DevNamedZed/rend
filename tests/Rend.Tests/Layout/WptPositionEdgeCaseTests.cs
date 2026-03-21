using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Edge cases for CSS positioning that commonly fail in WPT.
    /// </summary>
    public class WptPositionEdgeCaseTests
    {
        private readonly ITestOutputHelper _output;
        public WptPositionEdgeCaseTests(ITestOutputHelper output) { _output = output; }

        // abspos with percentage insets in padded container
        [Fact]
        public void AbsPos_PercentInsets_PaddedCB()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px;padding:20px'>
                    <div id='t' style='position:absolute;top:10%;left:10%;width:50px;height:50px'></div>
                </div></body>");
            // CB padding box = 240x240. 10% = 24px.
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t: ({t.ContentRect.X},{t.ContentRect.Y})");
            Assert.True(t.ContentRect.X >= 23 && t.ContentRect.X <= 25);
            Assert.True(t.ContentRect.Y >= 23 && t.ContentRect.Y <= 25);
        }

        // relative positioning doesn't affect parent height
        [Fact]
        public void Relative_ParentHeightUnchanged()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='position:relative;top:100px;height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(r, "parent")!;
            // Parent auto height based on normal flow position, not visual
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 30) < 2,
                $"Parent height = 30 (not 130) (got {parent.ContentRect.Height})");
        }

        // abspos with negative top
        [Fact]
        public void AbsPos_NegativeTop()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;top:-20px;left:0;width:50px;height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y < 0);
        }

        // multiple abspos children in same container
        [Fact]
        public void AbsPos_Multiple_Independent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='a' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div>
                    <div id='b' style='position:absolute;top:100px;left:100px;width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 100) < 2);
        }

        // abspos doesn't affect parent auto height
        [Fact]
        public void AbsPos_NoEffectOnParentHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='position:relative;width:200px'>
                    <div style='height:50px'></div>
                    <div style='position:absolute;top:0;height:500px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(r, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2,
                $"Abspos doesn't affect parent height (got {parent.ContentRect.Height})");
        }

        // z-index on positioned elements
        [Fact]
        public void ZIndex_Values()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative'>
                    <div id='a' style='position:relative;z-index:-1;width:50px;height:50px'></div>
                    <div id='b' style='position:relative;z-index:0;width:50px;height:50px'></div>
                    <div id='c' style='position:relative;z-index:999;width:50px;height:50px'></div>
                </div></body>");
            var sa = (LayoutTestHelper.FindById(r, "a")!.StyledNode as Rend.Style.StyledElement)!;
            var sb = (LayoutTestHelper.FindById(r, "b")!.StyledNode as Rend.Style.StyledElement)!;
            var sc = (LayoutTestHelper.FindById(r, "c")!.StyledNode as Rend.Style.StyledElement)!;
            Assert.Equal(-1, sa.Style.ZIndex);
            Assert.Equal(0, sb.Style.ZIndex);
            Assert.Equal(999, sc.Style.ZIndex);
        }

        // float doesn't affect abspos
        [Fact]
        public void AbsPos_IgnoresFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div style='float:left;width:80px;height:80px'></div>
                    <div id='t' style='position:absolute;top:0;left:0;width:50px;height:50px'></div>
                </div></body>");
            // Abspos ignores floats — positioned at top:0 left:0
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X < 2);
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y < 2);
        }

        // abspos with only right and bottom (no left/top)
        [Fact]
        public void AbsPos_OnlyRightBottom()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:200px'>
                    <div id='t' style='position:absolute;right:10px;bottom:20px;width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 130) < 2);
        }

        // fixed position with percentage dimensions
        [Fact]
        public void Fixed_PercentDimensions()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='position:fixed;width:50%;height:50%;top:0;left:0'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 150) < 2);
        }

        // nested relative positioning
        [Fact]
        public void NestedRelative_Offsets()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;top:10px;left:20px;width:200px'>
                    <div id='t' style='position:relative;top:5px;left:10px;height:30px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            // Combined: top=15, left=30
            Assert.True(t.ContentRect.X >= 29);
            Assert.True(t.ContentRect.Y >= 14);
        }
    }
}

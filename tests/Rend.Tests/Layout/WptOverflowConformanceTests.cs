using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests mirroring WPT css-overflow patterns.
    /// </summary>
    public class WptOverflowConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptOverflowConformanceTests(ITestOutputHelper output) { _output = output; }

        // overflow:hidden clips at explicit height
        [Fact]
        public void Hidden_ClipsAtHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:100px;height:50px'>
                    <div style='height:300px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 50) < 2);
        }

        // overflow:hidden contains floats (establishes BFC)
        [Fact]
        public void Hidden_ContainsFloats()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px'>
                    <div style='float:left;width:80px;height:120px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 119);
        }

        // overflow:auto contains floats
        [Fact]
        public void Auto_ContainsFloats()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:200px'>
                    <div style='float:left;width:80px;height:90px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 89);
        }

        // overflow:scroll contains floats
        [Fact]
        public void Scroll_ContainsFloats()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:scroll;width:200px'>
                    <div style='float:left;width:80px;height:70px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 69);
        }

        // overflow:hidden avoids sibling floats (BFC)
        [Fact]
        public void Hidden_AvoidsSiblingFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:50px'></div>
                    <div id='t' style='overflow:hidden'>content</div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X >= 99);
        }

        // overflow:visible does not establish BFC
        [Fact]
        public void Visible_NoAvoidFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:50px'></div>
                    <div id='t' style='overflow:visible;height:30px'></div>
                </div></body>");
            // overflow:visible does NOT avoid floats — content overlaps
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X < 2);
        }

        // overflow:clip parsed correctly
        [Fact]
        public void Clip_Parsed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:clip;width:100px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Clip, s.Style.OverflowX);
            Assert.Equal(CssOverflow.Clip, s.Style.OverflowY);
        }

        // overflow-x and overflow-y set independently
        [Fact]
        public void XY_Independent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow-x:hidden;overflow-y:scroll;width:100px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssOverflow.Hidden, s.Style.OverflowX);
            Assert.Equal(CssOverflow.Scroll, s.Style.OverflowY);
        }

        // text-overflow:ellipsis parsed
        [Fact]
        public void TextOverflow_Ellipsis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='text-overflow:ellipsis;overflow:hidden;white-space:nowrap;width:50px'>long text</div></body>");
            var s = (LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssTextOverflow.Ellipsis, s.Style.TextOverflow);
        }

        // max-height clips with overflow:hidden
        [Fact]
        public void MaxHeight_WithHidden()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;max-height:60px;width:200px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 61);
        }

        // overflow:hidden with border-box
        [Fact]
        public void Hidden_BorderBox()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;box-sizing:border-box;width:100px;height:80px;padding:10px;border:5px solid'>
                    <div style='height:200px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float totalH = t.ContentRect.Height + t.PaddingTop + t.PaddingBottom + t.BorderTopWidth + t.BorderBottomWidth;
            Assert.True(System.Math.Abs(totalH - 80) < 2);
        }

        // overflow:hidden doesn't affect siblings
        [Fact]
        public void Hidden_NoSiblingEffect()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='overflow:hidden;height:50px'>
                        <div style='height:300px'></div>
                    </div>
                    <div id='sib' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "sib")!.ContentRect.Y - 50) < 2);
        }

        // overflow:hidden prevents margin collapse with parent
        [Fact]
        public void Hidden_PreventsMarginCollapse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='p' style='overflow:hidden;width:200px'>
                    <div id='c' style='margin-top:30px;height:20px'></div>
                </div></body>");
            var p = LayoutTestHelper.FindById(r, "p")!;
            var c = LayoutTestHelper.FindById(r, "c")!;
            Assert.True(c.ContentRect.Y - p.ContentRect.Y >= 29);
            Assert.True(p.ContentRect.Height >= 49);
        }

        // overflow:auto with explicit height clips content
        [Fact]
        public void Auto_ExplicitHeight_Clips()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:auto;width:100px;height:40px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 40) < 2);
        }

        // overflow:hidden with auto height contains content normally
        [Fact]
        public void Hidden_AutoHeight_WrapsContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='overflow:hidden;width:200px'>
                    <div style='height:80px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 80) < 2);
        }

        // nested overflow:hidden
        [Fact]
        public void Nested_Hidden()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='overflow:hidden;width:200px;height:100px'>
                    <div id='inner' style='overflow:hidden;width:150px;height:60px'>
                        <div style='height:300px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "inner")!.ContentRect.Height - 60) < 2);
        }
    }
}

using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptImagesConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptImagesConformanceTests(ITestOutputHelper output) { _output = output; }

        // img with width/height attributes
        [Fact]
        public void Img_Attributes()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' width='200' height='150'></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 150) < 2);
        }

        // CSS width overrides attribute
        [Fact]
        public void Img_CssOverridesAttr()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' width='200' height='150' style='width:100px;height:80px'></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 80) < 2);
        }

        // object-fit values parsed
        [Fact] public void ObjectFit_Cover() { AssertObjectFit("cover", CssObjectFit.Cover); }
        [Fact] public void ObjectFit_Contain() { AssertObjectFit("contain", CssObjectFit.Contain); }
        [Fact] public void ObjectFit_Fill() { AssertObjectFit("fill", CssObjectFit.Fill); }
        [Fact] public void ObjectFit_None() { AssertObjectFit("none", CssObjectFit.None); }
        [Fact] public void ObjectFit_ScaleDown() { AssertObjectFit("scale-down", CssObjectFit.ScaleDown); }

        // img with box-sizing: border-box
        [Fact]
        public void Img_BorderBox()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='box-sizing:border-box;width:100px;height:80px;padding:10px;border:5px solid'></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(totalW - 100) < 2);
        }

        // img with max-width
        [Fact]
        public void Img_MaxWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' width='500' height='300' style='max-width:200px'></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width <= 201);
        }

        // img in flex container
        [Fact]
        public void Img_InFlex()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><img id='t' width='100' height='80'><div style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // img in grid
        [Fact]
        public void Img_InGrid()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><img id='t' width='100' height='80'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width <= 201);
        }

        // image-rendering parsed
        [Fact]
        public void ImageRendering_Pixelated()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='image-rendering:pixelated;width:100px;height:100px'></body>");
            Assert.Equal(CssImageRendering.Pixelated, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.ImageRendering);
        }

        // hr full width
        [Fact]
        public void Hr_FullWidth()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><hr id='t'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width > 200);
        }

        // br produces line break (container height > single line)
        [Fact]
        public void Br_LineBreak()
        {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='c' style='width:200px'>text<br>more</div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "c")!.ContentRect.Height > 20);
        }

        private void AssertObjectFit(string value, CssObjectFit expected)
        {
            var r = LayoutTestHelper.Layout($"<body style='margin:0'><img id='t' style='object-fit:{value};width:100px;height:100px'></body>");
            Assert.Equal(expected, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.ObjectFit);
        }
    }
}

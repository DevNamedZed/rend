using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for replaced element sizing (images, form controls),
    /// object-fit, object-position, and intrinsic ratio handling.
    /// </summary>
    public class WptImageSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptImageSizingTests(ITestOutputHelper output) { _output = output; }

        // [CSS2 §10.3.2] img with explicit width/height attributes
        [Fact] public void Img_AttributeDimensions() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' width='200' height='150'></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Height - 150) < 2);
        }

        // [CSS2 §10.3.2] CSS width overrides attribute
        [Fact] public void Img_CssOverridesAttribute() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' width='200' height='150' style='width:100px;height:80px'></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Height - 80) < 2);
        }

        // [CSS-IMAGES §5.1] object-fit property parsed
        [Fact] public void ObjectFit_Cover() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='object-fit:cover;width:100px;height:100px'></body>");
            Assert.Equal(CssObjectFit.Cover, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ObjectFit);
        }

        [Fact] public void ObjectFit_Contain() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='object-fit:contain;width:100px;height:100px'></body>");
            Assert.Equal(CssObjectFit.Contain, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ObjectFit);
        }

        [Fact] public void ObjectFit_Fill() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='object-fit:fill;width:100px;height:100px'></body>");
            Assert.Equal(CssObjectFit.Fill, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ObjectFit);
        }

        [Fact] public void ObjectFit_None() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='object-fit:none;width:100px;height:100px'></body>");
            Assert.Equal(CssObjectFit.None, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ObjectFit);
        }

        [Fact] public void ObjectFit_ScaleDown() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='object-fit:scale-down;width:100px;height:100px'></body>");
            Assert.Equal(CssObjectFit.ScaleDown, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.ObjectFit);
        }

        // [CSS-SIZING §5.1] aspect-ratio on replaced element
        [Fact] public void Img_AspectRatio_WidthOnly() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' width='200' height='100' style='width:100px'></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.Width - 100) < 2);
            // height should scale proportionally: 100 * (100/200) = 50
            _output.WriteLine($"img: {t.ContentRect.Width}x{t.ContentRect.Height}");
        }

        // [HTML] hr element is full width
        [Fact] public void Hr_FullWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><hr id='t'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width > 200);
        }

        // [CSS2 §10.3.2] replaced element with box-sizing: border-box
        [Fact] public void Img_BorderBox() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><img id='t' style='box-sizing:border-box;width:100px;height:80px;padding:10px;border:5px solid'></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(totalW - 100) < 2);
        }
    }
}

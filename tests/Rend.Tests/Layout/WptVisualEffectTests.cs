using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS visual effects: opacity, visibility, box-shadow,
    /// text-shadow, filter, clip-path, mix-blend-mode, isolation.
    /// </summary>
    public class WptVisualEffectTests
    {
        private readonly ITestOutputHelper _output;
        public WptVisualEffectTests(ITestOutputHelper output) { _output = output; }

        // [CSS-COLOR §3.2] opacity
        [Fact] public void Opacity_0() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='opacity:0;width:50px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Opacity) < 0.01f);
        }

        [Fact] public void Opacity_1() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='opacity:1;width:50px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Opacity - 1) < 0.01f);
        }

        [Fact] public void Opacity_Half() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='opacity:0.5;width:50px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Opacity - 0.5f) < 0.01f);
        }

        // [CSS-COLOR §3.2] opacity doesn't affect layout
        [Fact] public void Opacity_NoLayoutEffect() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='opacity:0;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // [CSS2 §11.2] visibility
        [Fact] public void Visibility_Hidden_TakesSpace() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='visibility:hidden;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Visibility_Collapse_OnNonTable() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='visibility:collapse;width:50px;height:50px'></div></body>");
            Assert.Equal(CssVisibility.Collapse, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Visibility);
        }

        // [CSS-BACKGROUNDS §7] box-shadow parsed
        [Fact] public void BoxShadow_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='box-shadow:2px 2px 5px rgba(0,0,0,0.5);width:50px;height:50px'></div></body>");
            var ref1 = ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.GetRefValue(Rend.Css.Properties.Internal.PropertyId.BoxShadow);
            Assert.NotNull(ref1);
        }

        // [CSS-BACKGROUNDS §7] box-shadow doesn't affect layout
        [Fact] public void BoxShadow_NoLayoutEffect() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='box-shadow:10px 10px 20px black;height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        // [CSS-TEXT-DECOR §4] text-shadow parsed
        [Fact] public void TextShadow_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-shadow:1px 1px 2px red;width:100px'>x</div></body>");
            var ref1 = ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.GetRefValue(Rend.Css.Properties.Internal.PropertyId.TextShadow);
            Assert.NotNull(ref1);
        }

        // [CSS-COMPOSITING §2] isolation parsed
        [Fact] public void Isolation_Isolate() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='isolation:isolate;width:50px;height:50px'></div></body>");
            Assert.Equal(CssIsolation.Isolate, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Isolation);
        }

        // [CSS-FILTER §2] filter doesn't affect layout
        [Fact] public void Filter_NoLayoutEffect() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='filter:blur(5px);height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Contain_Size() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='contain:size;width:100px;height:50px'></div></body>");
            Assert.Equal(CssContain.Size, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Contain);
        }

        [Fact] public void Contain_Strict() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='contain:strict;width:100px;height:50px'></div></body>");
            Assert.Equal(CssContain.Strict, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Contain);
        }

        // [CSS-CONTAIN §3] contain: size makes auto height = 0
        [Fact] public void Contain_Size_AutoHeight_Zero() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='contain:size;width:100px'><div style='height:200px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 1);
        }

        // [CSS2 §11.1.2] resize property
        [Fact] public void Resize_Both() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='resize:both;overflow:auto;width:100px;height:100px'></div></body>");
            Assert.Equal(CssResize.Both, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.Resize);
        }
    }
}

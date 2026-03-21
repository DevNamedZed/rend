using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBackgroundTests
    {
        private readonly ITestOutputHelper _output;
        public WptBackgroundTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void BackgroundColor_Inherited_From_Body()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0; background-color: green;'>
                <div id='test' style='width: 100px; height: 100px;'></div></body>");
            var test = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(test);
            // background-color doesn't inherit — div should have transparent
            var styled = (test!.StyledNode as StyledElement)!;
            Assert.Equal(0, styled.Style.BackgroundColor.A); // transparent
        }

        [Fact]
        public void BackgroundClip_BorderBox_Default()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='background: red; width: 100px; height: 100px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssBackgroundClip.BorderBox, styled.Style.BackgroundClip);
        }

        [Fact]
        public void BackgroundClip_PaddingBox()
        {
            // background-clip AFTER background shorthand (shorthand resets, then clip overrides)
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='background: red; background-clip: padding-box; width: 100px; height: 100px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssBackgroundClip.PaddingBox, styled.Style.BackgroundClip);
        }

        [Fact]
        public void BackgroundClip_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='background: red; background-clip: content-box; width: 100px; height: 100px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssBackgroundClip.ContentBox, styled.Style.BackgroundClip);
        }

        [Fact]
        public void BackgroundOrigin_PaddingBox_Default()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='background: red; width: 100px; height: 100px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssBackgroundOrigin.PaddingBox, styled.Style.BackgroundOrigin);
        }
    }
}

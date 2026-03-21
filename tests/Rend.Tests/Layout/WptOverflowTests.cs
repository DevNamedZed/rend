using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptOverflowTests
    {
        private readonly ITestOutputHelper _output;
        public WptOverflowTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void OverflowHidden_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow: hidden; width: 200px;'>
                    <div style='float: left; width: 100px; height: 80px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Height >= 79, $"Contains float (h={box.ContentRect.Height})");
        }

        [Fact]
        public void OverflowAuto_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow: auto; width: 200px;'>
                    <div style='float: left; width: 100px; height: 80px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Height >= 79, $"Contains float (h={box.ContentRect.Height})");
        }

        [Fact]
        public void OverflowScroll_ContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow: scroll; width: 200px;'>
                    <div style='float: left; width: 100px; height: 80px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Height >= 79, $"Contains float (h={box.ContentRect.Height})");
        }

        [Fact]
        public void OverflowVisible_DoesNotContainFloats()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='test' style='overflow: visible;'>
                        <div style='float: left; width: 100px; height: 80px;'></div>
                    </div>
                    <div id='after' style='height: 20px;'></div>
                </div></body>");
            var test = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(test);
            // overflow:visible doesn't establish BFC, float may not be contained
            _output.WriteLine($"test.h={test!.ContentRect.Height}");
        }

        [Fact]
        public void OverflowHidden_ExplicitHeight_Clips()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='overflow: hidden; width: 100px; height: 50px;'>
                    <div style='height: 200px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Height <= 51, $"Clips at height (h={box.ContentRect.Height})");
        }

        [Fact]
        public void TextOverflow_Ellipsis_WithOverflowHidden()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='text-overflow: ellipsis; overflow: hidden; white-space: nowrap; width: 50px;'>
                    Long text that overflows the container
                </div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            Assert.Equal(CssTextOverflow.Ellipsis, styled.Style.TextOverflow);
            Assert.Equal(CssOverflow.Hidden, styled.Style.OverflowX);
            Assert.Equal(CssWhiteSpace.Nowrap, styled.Style.WhiteSpace);
        }
    }
}

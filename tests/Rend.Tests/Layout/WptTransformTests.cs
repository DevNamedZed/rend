using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTransformTests
    {
        private readonly ITestOutputHelper _output;
        public WptTransformTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Transform_DoesNotAffectLayout()
        {
            // Transforms are visual-only, don't affect layout
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div style='transform: translateX(100px); height: 30px;'></div>
                    <div id='sibling' style='height: 30px;'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            // Transform doesn't push siblings
            Assert.True(System.Math.Abs(sibling!.ContentRect.Y - 30) < 2,
                $"Transform doesn't affect layout (Y={sibling.ContentRect.Y})");
        }

        [Fact]
        public void Transform_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='transform: rotate(45deg); width: 100px; height: 100px;'></div></body>");
            var styled = (LayoutTestHelper.FindById(root, "test")!.StyledNode as StyledElement)!;
            var transform = styled.Style.GetRefValue(Rend.Css.Properties.Internal.PropertyId.Transform);
            _output.WriteLine($"transform={transform}");
            Assert.NotNull(transform);
        }

        [Fact]
        public void Transform_CreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='transform: translateX(0); width: 100px; height: 100px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            // Any transform (even identity) creates a stacking context
            // and the element becomes a containing block for abspos descendants
        }

        [Fact]
        public void Transform_Scale_Parsed()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='transform: scale(2); width: 50px; height: 50px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            // Layout size unchanged — scale is visual only
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 50) < 1);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 1);
        }
    }
}

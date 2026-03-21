using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTransformsConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptTransformsConformanceTests(ITestOutputHelper output) { _output = output; }

        // transform doesn't affect layout flow
        [Fact]
        public void NoLayoutEffect_Translate()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='transform:translateX(100px);height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        [Fact]
        public void NoLayoutEffect_Scale()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='transform:scale(2);height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        [Fact]
        public void NoLayoutEffect_Rotate()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div style='transform:rotate(45deg);height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 30) < 2);
        }

        // transform:none parsed
        [Fact]
        public void Transform_None()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='transform:none;width:50px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 50) < 2);
        }

        // transform parsed as ref value
        [Fact]
        public void Transform_Parsed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='transform:rotate(45deg);width:100px;height:100px'></div></body>");
            var refVal = ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.GetRefValue(PropertyId.Transform);
            Assert.NotNull(refVal);
        }

        // scale doesn't change layout size
        [Fact]
        public void Scale_KeepsLayoutSize()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='transform:scale(3);width:50px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 50) < 2);
        }

        // translate doesn't change layout position
        [Fact]
        public void Translate_KeepsLayoutPosition()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='transform:translateX(100px);width:50px;height:50px'></div></body>");
            // Layout position unchanged (transform is visual only)
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X < 10);
        }

        // multiple transforms parsed
        [Fact]
        public void MultipleTransforms()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='transform:rotate(45deg) scale(1.5) translateX(10px);width:50px;height:50px'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 50) < 2);
        }

        // transform-origin parsed
        [Fact]
        public void TransformOrigin_Parsed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='transform:rotate(10deg);transform-origin:top left;width:100px;height:100px'></div></body>");
            var refVal = ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.GetRefValue(PropertyId.TransformOrigin);
            Assert.NotNull(refVal);
        }

        // transform doesn't affect parent height
        [Fact]
        public void NoParentHeightEffect()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='width:200px;overflow:hidden'><div style='transform:scale(5);height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "p")!.ContentRect.Height - 20) < 2);
        }
    }
}

using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockAutoHeightTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockAutoHeightTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void AutoHeight_SingleChild() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void AutoHeight_TwoChildren() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:30px'></div><div style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 70) < 2);
        }

        [Fact] public void AutoHeight_ThreeChildren() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:20px'></div><div style='height:30px'></div><div style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 90) < 2);
        }

        [Fact] public void AutoHeight_Empty() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2);
        }

        [Fact] public void AutoHeight_WithPadding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;padding:20px'><div style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void AutoHeight_WithBorder() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;border:10px solid'><div style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void AutoHeight_Nested() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div><div style='height:80px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void AutoHeight_AbsposExcluded() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='position:relative;width:200px'><div style='height:50px'></div><div style='position:absolute;height:500px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2);
        }

        [Fact] public void AutoHeight_DisplayNone_Excluded() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:30px'></div><div style='display:none;height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2);
        }

        [Fact] public void AutoHeight_VisibilityHidden_Included() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:30px'></div><div style='visibility:hidden;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 70) < 2);
        }

        [Fact] public void AutoHeight_FlexContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px'><div style='width:50px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void AutoHeight_FlexColumn() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;flex-direction:column;width:200px'><div style='height:30px'></div><div style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 70) < 2);
        }

        [Fact] public void AutoHeight_GridContainer() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:200px;width:200px'><div style='height:50px'></div><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        [Fact] public void AutoHeight_OverflowHidden_ContainsFloats() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='overflow:hidden;width:200px'><div style='float:left;width:100px;height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 79);
        }

        [Fact] public void AutoHeight_ChildWithMargin() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;overflow:hidden'><div style='height:50px;margin-bottom:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 69);
        }

        [Fact] public void ExplicitHeight_OverridesAuto() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:100px'><div style='height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }
    }
}

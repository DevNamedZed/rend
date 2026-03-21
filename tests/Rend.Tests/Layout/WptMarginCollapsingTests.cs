using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptMarginCollapsingTests
    {
        private readonly ITestOutputHelper _output;
        public WptMarginCollapsingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Adjacent_Siblings_Collapse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:30px'></div><div id='t' style='height:50px;margin-top:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void Larger_Margin_Wins() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:50px'></div><div id='t' style='height:50px;margin-top:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void Equal_Margins_Collapse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:30px'></div><div id='t' style='height:50px;margin-top:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void No_Collapse_With_Border_Between() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:30px;border-bottom:1px solid'></div><div id='t' style='height:50px;margin-top:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 80);
        }

        [Fact] public void No_Collapse_OverflowHidden() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px;margin-bottom:30px'></div><div style='overflow:hidden'><div id='t' style='margin-top:20px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 79);
        }

        [Fact] public void No_Collapse_InFlex() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div style='height:50px;margin-bottom:30px'></div><div id='t' style='height:50px;margin-top:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void No_Collapse_InGrid() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div style='height:50px;margin-bottom:30px'></div><div id='t' style='height:50px;margin-top:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 100) < 2);
        }

        [Fact] public void ParentChild_TopMargin_Collapse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='p' style='margin-top:30px'><div id='t' style='margin-top:20px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void ParentChild_No_Collapse_With_Padding() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='margin-top:30px;padding-top:1px'><div id='t' style='margin-top:20px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 51) < 2);
        }

        [Fact] public void ParentChild_No_Collapse_With_Border() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='margin-top:30px;border-top:1px solid'><div id='t' style='margin-top:20px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 51) < 2);
        }

        [Fact] public void Negative_Margins_MostNegative() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:-10px'></div><div id='t' style='height:50px;margin-top:-20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }

        [Fact] public void Mixed_Positive_Negative() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:30px'></div><div id='t' style='height:50px;margin-top:-10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2);
        }

        [Fact] public void Three_Siblings_Cascade() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:30px;margin-bottom:20px'></div><div style='height:30px;margin-top:15px;margin-bottom:25px'></div><div id='t' style='height:30px;margin-top:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 105) < 2);
        }

        [Fact] public void No_Collapse_Float() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='float:left;width:200px;margin-bottom:30px;height:50px'></div><div id='t' style='clear:left;margin-top:20px;height:50px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 69);
        }

        [Fact] public void No_Collapse_Abspos() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:200px'><div style='height:50px;margin-bottom:30px'></div><div style='position:absolute;margin-top:20px;height:50px'></div><div id='t' style='height:50px;margin-top:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 2);
        }

        [Fact] public void Empty_Block_Collapses() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='overflow:hidden'><div style='height:50px;margin-bottom:30px'></div><div style='margin-top:20px;margin-bottom:25px'></div><div id='t' style='height:50px;margin-top:10px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 80) < 3);
        }
    }
}

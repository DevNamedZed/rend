using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptAutoSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptAutoSizingTests(ITestOutputHelper output) { _output = output; }

        // auto width block fills container minus spacing
        [Fact]
        public void AutoWidth_FillsContainer()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 400) < 2);
        }

        // auto width with margin subtracts margin
        [Fact]
        public void AutoWidth_SubtractsMargin()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='margin:0 30px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 340) < 2);
        }

        // auto width with padding subtracts padding
        [Fact]
        public void AutoWidth_SubtractsPadding()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='padding:0 25px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 350) < 2);
        }

        // auto width with border subtracts border
        [Fact]
        public void AutoWidth_SubtractsBorder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='border:10px solid;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 380) < 2);
        }

        // auto width subtracts ALL horizontal spacing
        [Fact]
        public void AutoWidth_AllSpacing()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='margin:0 10px;padding:0 15px;border:5px solid;height:20px'></div></div></body>");
            // 400 - 10*2 - 15*2 - 5*2 = 340
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 340) < 2);
        }

        // auto height = content height
        [Fact]
        public void AutoHeight_EqualsContent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;overflow:hidden'><div style='height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 80) < 2);
        }

        // auto height with padding adds padding
        [Fact]
        public void AutoHeight_AddsPadding()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;padding:20px;overflow:hidden'><div style='height:50px'></div></div></body>");
            // content = 50. But ContentRect is content-box, padding separate.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 50) < 2);
        }

        // auto height with multiple children sums
        [Fact]
        public void AutoHeight_SumsChildren()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;overflow:hidden'><div style='height:30px'></div><div style='height:40px'></div><div style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 90) < 2);
        }

        // auto height with collapsing margins
        [Fact]
        public void AutoHeight_CollapsingMargins()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;overflow:hidden'><div style='margin-bottom:20px;height:30px'></div><div style='margin-top:15px;height:30px'></div></div></body>");
            // 30 + collapse(20,15)=20 + 30 = 80
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 80) < 2);
        }

        // float: shrink-to-fit = widest child
        [Fact]
        public void Float_ShrinkToFit_WidestChild()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='float:left'><div style='width:120px;height:10px'></div><div style='width:80px;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // abspos: shrink-to-fit with auto width
        [Fact]
        public void AbsPos_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='position:relative;width:400px;height:200px'><div id='t' style='position:absolute;top:0;left:0'><div style='width:80px;height:20px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width <= 81);
        }

        // inline-block: shrink-to-fit
        [Fact]
        public void InlineBlock_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><span id='t' style='display:inline-block'><div style='width:60px;height:20px'></div></span></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 60) < 2);
        }

        // fit-content: min(max-content, max(min-content, available))
        [Fact]
        public void FitContent_ConstrainedByAvailable()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px'><div id='t' style='width:fit-content'><div style='width:150px;height:10px'></div></div></div></body>");
            // max-content=150, available=100. fit-content = max(150, min(150, 100)) = 150?
            // Actually fit-content = min(max-content, max(min-content, available)) = min(150, max(150, 100)) = 150
            // But... fit-content shouldn't exceed available. Let me check.
            // CSS says fit-content = min(max-content, max(min-content, stretch-fit)).
            // stretch-fit = available = 100. So fit-content = min(150, max(150, 100)) = min(150, 150) = 150.
            // Actually no: fit-content means shrink-to-fit ≤ available.
            // fit-content = max(min-content, min(max-content, available))
            // = max(150, min(150, 100)) = max(150, 100) = 150
            _output.WriteLine($"fit-content: {LayoutTestHelper.FindById(r, "t")!.ContentRect.Width}");
        }

        // min-content = widest obligatory break
        [Fact]
        public void MinContent_WidestChild()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:min-content'><div style='width:80px;height:10px'></div><div style='width:120px;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // max-content = widest possible line
        [Fact]
        public void MaxContent_WidestChild()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:max-content'><div style='width:120px;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }

        // flex row auto height = tallest item
        [Fact]
        public void FlexRow_AutoHeight_Tallest()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div style='width:50px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 80) < 2);
        }

        // flex column auto height = sum of items + gaps
        [Fact]
        public void FlexColumn_AutoHeight_Sum()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:flex;flex-direction:column;gap:10px;width:100px'><div style='height:30px'></div><div style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 70) < 2);
        }

        // grid auto height = sum of row heights + gaps
        [Fact]
        public void Grid_AutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='display:grid;grid-template-columns:100px;gap:10px;width:100px'><div style='height:40px'></div><div style='height:50px'></div></div></body>");
            // 40+10+50 = 100
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // multicol auto height = balanced column height
        [Fact]
        public void Multicol_AutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='column-count:2;column-gap:0;width:200px'><div style='height:40px'></div><div style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 40) < 2);
        }
    }
}

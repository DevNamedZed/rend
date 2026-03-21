using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxRowCrossAxisTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxRowCrossAxisTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Stretch_100px() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2); }
        [Fact] public void Stretch_200px() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:50px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2); }
        [Fact] public void ExplicitHeight_NoStretch() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 30) < 2); }
        [Fact] public void FlexStart_Y0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 0) < 2); }
        [Fact] public void FlexEnd_Y70() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2); }
        [Fact] public void Center_Y35() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2); }
        [Fact] public void AlignSelf_Center() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='align-self:center;width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2); }
        [Fact] public void AlignSelf_FlexEnd() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='align-self:flex-end;width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2); }
        [Fact] public void AlignSelf_Stretch() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='align-self:stretch;width:50px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2); }
        [Fact] public void MarginTopAuto() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;height:30px;margin-top:auto'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 70) < 2); }
        [Fact] public void MarginAutoY_Centers() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;height:30px;margin-top:auto;margin-bottom:auto'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 35) < 2); }
        [Fact] public void Stretch_WithPadding() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;padding:10px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 20 - 100) < 2); }
        [Fact] public void Stretch_WithBorder() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;border:5px solid'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height + 10 - 100) < 2); }
        [Fact] public void Stretch_MaxHeight_Clamps() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:50px;max-height:80px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 81); }
        [Fact] public void AutoHeight_TallestItem() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='width:50px'></div><div style='width:50px;height:80px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Height - 80) < 2); }
    }
}

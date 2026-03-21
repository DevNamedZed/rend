using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockBorderEffectTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockBorderEffectTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Border_AllSides_5px() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:5px solid;width:100px;height:50px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 5) < 1); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderTopWidth - 5) < 1); }
        [Fact] public void Border_OffsetsChild() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border:10px solid;width:200px'><div id='t' style='height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 10) < 2); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 10) < 2); }
        [Fact] public void Border_ReducesAutoWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='border:10px solid;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 280) < 2); }
        [Fact] public void Border_StyleNone_ZeroWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:5px none;width:100px;height:30px'></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth < 1); }
        [Fact] public void Border_StyleHidden_ZeroWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border:5px hidden;width:100px;height:30px'></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth < 1); }
        [Fact] public void Border_Individual_Left3_Right7() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='border-left:3px solid;border-right:7px solid;width:100px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 3) < 1); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderRightWidth - 7) < 1); }
        [Fact] public void Border_PreventsMarginCollapse() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border-top:1px solid'><div id='t' style='margin-top:20px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 21) < 2); }
        [Fact] public void Border_AddsSiblingOffset() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:40px;border-bottom:10px solid'></div><div id='t' style='height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2); }
        [Fact] public void Border_WithPadding() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border:5px solid;padding:10px;width:200px'><div id='t' style='height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 15) < 2); }
        [Fact] public void Border_ContentBox_Width() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;border:10px solid;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void Border_BorderBox_Width() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;border:10px solid;height:40px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2); }
        [Fact] public void Outline_NoLayout() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='outline:10px solid;width:100px;height:50px'></div><div id='t' style='height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 50) < 2); }
        [Fact] public void Border_Transparent_TakesSpace() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='t' style='border:10px solid transparent;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 280) < 2); }
        [Fact] public void Border_InFlex() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 100px;border:5px solid;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 5) < 1); }
        [Fact] public void Border_InGrid() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'><div id='t' style='border:8px solid;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.BorderLeftWidth - 8) < 1); }
    }
}

using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxAutoWidthTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAutoWidthTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Block_FillsViewport() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 400) < 2); }
        [Fact] public void Block_FillsParent300() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:300px'><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Block_FillsParent200() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2); }
        [Fact] public void InlineFlex_Shrink1Item() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:inline-flex'><div style='width:120px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 120) < 2); }
        [Fact] public void InlineFlex_Shrink2Items() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:inline-flex'><div style='width:80px;height:30px'></div><div style='width:60px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 140) < 2); }
        [Fact] public void InlineFlex_Shrink3Items() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:inline-flex'><div style='width:50px;height:30px'></div><div style='width:60px;height:30px'></div><div style='width:70px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 180) < 2); }
        [Fact] public void InlineFlex_WithGap() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:inline-flex;gap:10px'><div style='width:50px;height:30px'></div><div style='width:60px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 120) < 2); }
        [Fact] public void Explicit_Width300() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:300px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Percent_Width50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:flex;width:50%'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MinWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:100px;min-width:250px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 250) < 2); }
        [Fact] public void MaxWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:400px;max-width:200px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2); }
        [Fact] public void BorderBox() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;box-sizing:border-box;width:300px;padding:20px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 260) < 2); }
        [Fact] public void MarginAutoCenter() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;margin:0 auto'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.X - 100) < 2); }
        [Fact] public void Block_MinusMargin() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:flex;margin:0 50px'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Calc_Width() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:calc(200px + 100px)'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2); }
    }
}

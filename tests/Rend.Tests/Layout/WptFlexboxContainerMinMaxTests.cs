using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxContainerMinMaxTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxContainerMinMaxTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MinWidth_200_Width100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:100px;min-width:200px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MaxWidth_200_Width300() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:300px;max-width:200px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MinHeight_100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;min-height:100px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height >= 99); }
        [Fact] public void MaxHeight_80() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;height:200px;max-height:80px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height <= 81); }
        [Fact] public void MinWidth_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:300px;min-width:100px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2); }
        [Fact] public void MaxWidth_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:100px;max-width:300px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 100) < 2); }
        [Fact] public void MinWidth_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:flex;width:50px;min-width:50%'><div style='width:30px;height:30px'></div></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width >= 199); }
        [Fact] public void MaxWidth_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:flex;width:300px;max-width:50%'><div style='width:30px;height:30px'></div></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width <= 201); }
        [Fact] public void MinHeight_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;height:200px;min-height:50px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 200) < 2); }
        [Fact] public void MaxHeight_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;height:50px;max-height:200px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 50) < 2); }
        [Fact] public void MinWidth_250_AutoWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='f' style='display:flex;min-width:250px'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width >= 249); }
        [Fact] public void MaxWidth_150_AutoWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;max-width:150px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width <= 151); }
        [Fact] public void Column_MinHeight_200() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-direction:column;width:200px;min-height:200px'><div style='height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height >= 199); }
        [Fact] public void Column_MaxHeight_100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;flex-direction:column;width:200px;height:200px;max-height:100px'><div style='height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height <= 101); }
        [Fact] public void MinWidth_WithBorderBox() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;box-sizing:border-box;width:100px;min-width:200px;padding:20px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width + 40 >= 199); }
    }
}

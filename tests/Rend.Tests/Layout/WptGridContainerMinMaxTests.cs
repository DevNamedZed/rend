using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridContainerMinMaxTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridContainerMinMaxTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MinWidth_200_Width100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:100px;min-width:200px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MaxWidth_200_Width300() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:300px;max-width:200px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MinHeight_100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;min-height:100px'><div style='height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height >= 99); }
        [Fact] public void MaxHeight_80() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:200px;max-height:80px'><div></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height <= 81); }
        [Fact] public void MinWidth_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:300px;min-width:100px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 300) < 2); }
        [Fact] public void MaxWidth_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:100px;max-width:300px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 100) < 2); }
        [Fact] public void MinWidth_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;width:50px;min-width:50%'><div style='height:20px'></div></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width >= 199); }
        [Fact] public void MaxWidth_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;width:300px;max-width:50%'><div style='height:20px'></div></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width <= 201); }
        [Fact] public void MinHeight_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:200px;min-height:50px'><div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 200) < 2); }
        [Fact] public void MaxHeight_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:50px;max-height:200px'><div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 50) < 2); }
        [Fact] public void MinWidth_250_Auto() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:200px'><div id='g' style='display:grid;grid-template-columns:1fr;min-width:250px'><div style='height:20px'></div></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width >= 249); }
        [Fact] public void MaxWidth_150_Auto() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;max-width:150px'><div style='height:20px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width <= 151); }
        [Fact] public void MinHeight_100_Auto() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;min-height:100px'><div style='height:20px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height >= 99); }
        [Fact] public void MinWidth_BorderBox() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:100px;min-width:200px;padding:20px'><div style='height:20px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width + 40 >= 199); }
        [Fact] public void MaxWidth_BorderBox() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;box-sizing:border-box;width:300px;max-width:200px;padding:20px'><div style='height:20px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width + 40 <= 201); }
    }
}

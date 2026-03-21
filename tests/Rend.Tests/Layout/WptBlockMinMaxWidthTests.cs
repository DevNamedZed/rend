using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockMinMaxWidthTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockMinMaxWidthTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void MinWidth_50_Width30() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:30px;min-width:50px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2); }
        [Fact] public void MinWidth_100_Width50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:100px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void MinWidth_200_Width300() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;min-width:200px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2); }
        [Fact] public void MaxWidth_100_Width200() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;max-width:100px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void MaxWidth_200_Width100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;max-width:200px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void MinWidth_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:50px;min-width:50%;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MaxWidth_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='t' style='width:300px;max-width:50%;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MinHeight_50_Height30() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:30px;min-height:50px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2); }
        [Fact] public void MaxHeight_50_Height100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:100px;max-height:50px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2); }
        [Fact] public void MinHeight_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:30px;min-height:50%'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2); }
        [Fact] public void MaxHeight_Percent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:300px;max-height:50%'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2); }
        [Fact] public void MinMax_Both_Middle() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:150px;min-width:100px;max-width:200px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2); }
        [Fact] public void MinMax_Both_Below() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:100px;max-width:200px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void MinMax_Both_Above() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;min-width:100px;max-width:200px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void MinWidth_Auto() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:100px'><div id='t' style='min-width:200px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 199); }
        [Fact] public void MaxWidth_Auto() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='max-width:200px;height:30px'></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 201); }
    }
}

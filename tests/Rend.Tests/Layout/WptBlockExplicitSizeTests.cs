using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockExplicitSizeTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockExplicitSizeTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void W50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2); }
        [Fact] public void W100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void W200() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void W300() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2); }
        [Fact] public void W400() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:400px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2); }
        [Fact] public void H50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:50px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2); }
        [Fact] public void H100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:100px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2); }
        [Fact] public void H200() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:200px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 200) < 2); }
        [Fact] public void W50_H50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;height:50px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 50) < 2); }
        [Fact] public void W200_H100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px;height:100px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2); }
        [Fact] public void Pct50_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50%;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void Pct25_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:25%;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void Pct75_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:75%;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Pct100_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100%;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2); }
        [Fact] public void Auto_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 400) < 2); }
        [Fact] public void Auto_H_1Child() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'><div style='height:60px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 60) < 2); }
        [Fact] public void Auto_H_Empty() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:200px'></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2); }
        [Fact] public void W0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:0;height:30px'></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 2); }
        [Fact] public void H0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:0'></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 2); }
        [Fact] public void Vw50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50vw;height:30px'></div></body>", 400, 300); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void Vh50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:100px;height:50vh'></div></body>", 400, 300); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 150) < 2); }
        [Fact] public void Calc_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:calc(200px + 50px);height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2); }
        [Fact] public void Em_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'><div id='t' style='width:10em;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2); }
        [Fact] public void MinW() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:50px;min-width:150px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2); }
        [Fact] public void MaxW() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='width:300px;max-width:150px;height:30px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2); }
        [Fact] public void BorderBox_W() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:200px;padding:20px;height:80px'></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 160) < 2); }
    }
}

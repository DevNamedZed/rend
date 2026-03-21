using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxBasisWidthTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxBasisWidthTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Basis50() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2); }
        [Fact] public void Basis100() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 100px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void Basis200() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 200px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void Basis0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 0px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 2); }
        [Fact] public void BasisAuto_Width120() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 auto;width:120px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2); }
        [Fact] public void Basis_Overrides_Width() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 150px;width:100px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2); }
        [Fact] public void Basis50Pct() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:0 0 50%;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2); }
        [Fact] public void Basis25Pct() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:0 0 25%;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2); }
        [Fact] public void BasisCalc() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2); }
        [Fact] public void Basis_Grow1_Fills() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:1 0 0px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Two_Basis_100_200() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:0 0 100px;height:30px'></div><div id='b' style='flex:0 0 200px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2); }
        [Fact] public void Basis_WithGrow_TwoItems() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 50px;height:30px'></div><div id='b' style='flex:1 0 100px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 125) < 2); }
        [Fact] public void Basis_Column_Height() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:200px'><div id='t' style='flex:0 0 80px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2); }
        [Fact] public void Basis_BorderBox() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='box-sizing:border-box;flex:0 0 150px;padding:20px;border:5px solid;height:60px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width + 40 + 10 - 150) < 2); }
        [Fact] public void Basis_MaxWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 200px;max-width:100px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 101); }
        [Fact] public void Basis_MinWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 1 50px;min-width:100px;height:30px'></div></div></body>"); Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 99); }
    }
}

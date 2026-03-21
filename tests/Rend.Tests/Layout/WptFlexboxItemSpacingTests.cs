using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxItemSpacingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxItemSpacingTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Gap10_Two() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 60) < 2); }

        [Fact] public void Gap20_Three() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 140) < 2); }

        [Fact] public void Margin10_Two() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;margin-right:10px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 60) < 2); }

        [Fact] public void Margin20_Three() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;margin-right:20px;height:30px'></div><div id='b' style='width:50px;margin-right:20px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 140) < 2); }

        [Fact] public void GapAndMargin() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:10px;width:300px'><div id='a' style='width:50px;margin-right:5px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 65) < 2); }

        [Fact] public void ColumnGap15_Two() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:15px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 45) < 2); }

        [Fact] public void ColumnGap20_Three() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:20px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 100) < 2); }

        [Fact] public void Gap0_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:0;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2); }

        [Fact] public void GapSingle_NoEffect() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='t' style='width:100px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 0) < 2); }

        [Fact] public void Gap_FiveItems() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:5px;width:400px'><div id='a' style='width:30px;height:20px'></div><div style='width:30px;height:20px'></div><div style='width:30px;height:20px'></div><div style='width:30px;height:20px'></div><div id='e' style='width:30px;height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"e")!.ContentRect.X - 140) < 2); }

        [Fact] public void MarginAutoSplit() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:50px;margin-right:auto;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 250) < 2); }

        [Fact] public void NegativeMargin() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='width:100px;height:30px'></div><div id='b' style='width:100px;margin-left:-20px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 80) < 2); }

        [Fact] public void Gap_WithGrow() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:220px'><div id='a' style='flex:1;height:30px'></div><div id='b' style='flex:1;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2); }

        [Fact] public void ColumnMargin_Two() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:30px;margin-bottom:15px'></div><div id='b' style='height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 45) < 2); }

        [Fact] public void Gap_Center() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;justify-content:center;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>"); float total = 50 + 20 + 50; float offset = (300 - total) / 2; Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - offset) < 2); }
    }
}

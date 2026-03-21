using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxContainerPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxContainerPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Container_X0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.X - 0) < 2); }
        [Fact] public void Container_Y0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Y - 0) < 2); }
        [Fact] public void Container_MarginLeft() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;margin-left:40px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.X - 40) < 2); }
        [Fact] public void Container_MarginTop() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;margin-top:25px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Y - 25) < 2); }
        [Fact] public void Container_MarginAutoCenter() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;margin:0 auto'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.X - 100) < 2); }
        [Fact] public void Container_AfterSibling() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:60px'></div><div id='f' style='display:flex;width:200px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Y - 60) < 2); }
        [Fact] public void Container_InPaddedParent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding:20px;width:300px'><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.X - 20) < 2); }
        [Fact] public void Container_InBorderedParent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border:12px solid;width:300px'><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.X - 12) < 2); }
        [Fact] public void Container_InFlexItem() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='flex:1'><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 400) < 2); }
        [Fact] public void Container_InGridItem() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'><div><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Container_PercentWidth() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='f' style='display:flex;width:75%'><div style='width:50px;height:30px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Container_ExplicitHeight() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px;height:150px'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 150) < 2); }
        [Fact] public void Container_AutoHeight() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex;width:200px'><div style='width:50px;height:80px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Height - 80) < 2); }
        [Fact] public void Container_FillsViewport() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='f' style='display:flex'><div style='width:50px;height:30px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"f")!.ContentRect.Width - 400) < 2); }
    }
}

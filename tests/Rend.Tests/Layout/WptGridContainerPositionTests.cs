using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridContainerPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridContainerPositionTests(ITestOutputHelper output) { _output = output; }

        [Fact] public void Container_X0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:200px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.X - 0) < 2); }
        [Fact] public void Container_Y0() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:200px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Y - 0) < 2); }
        [Fact] public void Container_MarginLeft() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:200px;margin-left:30px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.X - 30) < 2); }
        [Fact] public void Container_MarginTop() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:200px;margin-top:20px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Y - 20) < 2); }
        [Fact] public void Container_MarginAutoCenter() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr;width:200px;margin:0 auto'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.X - 100) < 2); }
        [Fact] public void Container_AfterSibling() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='height:50px'></div><div id='g' style='display:grid;grid-template-columns:1fr;width:200px'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Y - 50) < 2); }
        [Fact] public void Container_InPaddedParent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='padding:25px;width:300px'><div id='g' style='display:grid;grid-template-columns:1fr'><div style='height:20px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.X - 25) < 2); }
        [Fact] public void Container_InBorderedParent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='border:15px solid;width:300px'><div id='g' style='display:grid;grid-template-columns:1fr'><div style='height:20px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.X - 15) < 2); }
        [Fact] public void Container_InFlexItem() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'><div style='flex:1'><div id='g' style='display:grid;grid-template-columns:1fr'><div style='height:20px'></div></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 400) < 2); }
        [Fact] public void Container_InGridItem() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'><div><div id='g' style='display:grid;grid-template-columns:1fr'><div style='height:20px'></div></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Container_PercentWidth_InParent() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='width:400px'><div id='g' style='display:grid;grid-template-columns:1fr;width:75%'><div style='height:20px'></div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 300) < 2); }
        [Fact] public void Container_Height_Explicit() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px;height:150px'><div></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 150) < 2); }
        [Fact] public void Container_Height_Auto() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:200px;width:200px'><div style='height:80px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Height - 80) < 2); }
        [Fact] public void Container_Width_FillsViewport() { var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='g' style='display:grid;grid-template-columns:1fr'><div style='height:20px'></div></div></body>"); Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"g")!.ContentRect.Width - 400) < 2); }
    }
}

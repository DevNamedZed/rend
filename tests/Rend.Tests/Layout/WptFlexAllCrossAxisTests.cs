using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexAllCrossAxisTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAllCrossAxisTests(ITestOutputHelper output) { _output = output; }

        // --- Row stretch at various container heights ---

        [Fact]
        public void RowStretch_ContainerHeight50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:50px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void RowStretch_ContainerHeight80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:80px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void RowStretch_ContainerHeight100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void RowStretch_ContainerHeight120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:120px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 120) < 2);
        }

        [Fact]
        public void RowStretch_ContainerHeight150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:150px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 150) < 2);
        }

        [Fact]
        public void RowStretch_ContainerHeight200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void RowStretch_ContainerHeight250()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:250px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 250) < 2);
        }

        [Fact]
        public void RowStretch_ContainerHeight300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:300px;width:200px'><div id='t' style='width:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 300) < 2);
        }

        // --- Row flex-start at various container heights with item height 30 ---

        [Fact]
        public void RowFlexStart_ContainerHeight100_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 30) < 2);
        }

        [Fact]
        public void RowFlexStart_ContainerHeight150_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:150px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 30) < 2);
        }

        [Fact]
        public void RowFlexStart_ContainerHeight200_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:200px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 30) < 2);
        }

        // --- Row flex-end at various container heights ---

        [Fact]
        public void RowFlexEnd_ContainerHeight100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void RowFlexEnd_ContainerHeight150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:150px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 120) < 2);
        }

        [Fact]
        public void RowFlexEnd_ContainerHeight200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:200px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 170) < 2);
        }

        // --- Row center at various container heights with various item heights ---

        [Fact]
        public void RowCenter_ContainerHeight100_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight150_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:150px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 60) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight200_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:200px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 85) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight250_ItemHeight40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:250px;width:200px'><div id='t' style='width:60px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 105) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight300_ItemHeight50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:300px;width:200px'><div id='t' style='width:60px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 125) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight200_ItemHeight40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:200px;width:200px'><div id='t' style='width:60px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight150_ItemHeight50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:150px;width:200px'><div id='t' style='width:60px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 50) < 2);
        }

        // --- align-self overrides ---

        [Fact]
        public void AlignSelfCenter_OverridesFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:center;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void AlignSelfEnd_OverridesFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:120px;width:200px'><div id='a' style='width:50px;height:40px'></div><div id='b' style='align-self:flex-end;width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void AlignSelfStart_OverridesCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:flex-start;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void AlignSelfStretch_OverridesFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:stretch;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AlignSelfCenter_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:stretch;height:150px;width:200px'><div id='a' style='width:50px'></div><div id='b' style='align-self:center;width:50px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 55) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void AlignSelfEnd_OverridesCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:200px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:flex-end;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 85) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 170) < 2);
        }

        // --- margin-top:auto / margin-bottom:auto / both ---

        [Fact]
        public void MarginTopAuto_PushesItemToBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;height:30px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void MarginBottomAuto_KeepsItemAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;height:30px;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void MarginTopAndBottomAuto_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px;height:30px;margin-top:auto;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void MarginTopAuto_LargerContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:50px;height:40px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 160) < 2);
        }

        [Fact]
        public void MarginBottomAuto_LargerContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:50px;height:40px;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void MarginTopAndBottomAuto_LargerContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:50px;height:40px;margin-top:auto;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void MarginTopAuto_OverridesAlignItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:150px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 120) < 2);
        }

        [Fact]
        public void MarginBothAuto_OverridesAlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:120px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px;margin-top:auto;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 45) < 2);
        }

        // --- Column stretch at various container widths ---

        [Fact]
        public void ColumnStretch_ContainerWidth100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:100px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void ColumnStretch_ContainerWidth200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void ColumnStretch_ContainerWidth300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:300px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 300) < 2);
        }

        // --- Column center ---

        [Fact]
        public void ColumnCenter_ContainerWidth200_ItemWidth80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:center;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        // --- Column flex-end ---

        [Fact]
        public void ColumnEnd_ContainerWidth200_ItemWidth80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-end;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        // --- Explicit height overrides stretch ---

        [Fact]
        public void ExplicitHeight_OverridesStretch_Height40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:60px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void ExplicitHeight_OverridesStretch_Height80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:60px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void ExplicitHeight_OverridesStretch_Height150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:60px;height:150px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 150) < 2);
        }

        // --- max-height clamps stretch ---

        [Fact]
        public void MaxHeight_ClampsStretch_MaxHeight80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:200px;width:200px'><div id='t' style='width:60px;max-height:80px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height <= 81);
        }

        [Fact]
        public void MaxHeight_ClampsStretch_MaxHeight50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:300px;width:200px'><div id='t' style='width:60px;max-height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height <= 51);
        }

        [Fact]
        public void MaxHeight_ClampsStretch_MaxHeight120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:250px;width:200px'><div id='t' style='width:60px;max-height:120px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height <= 121);
        }

        [Fact]
        public void MaxHeight_DoesNotClampWhenSmaller()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:60px;max-height:200px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        // --- Additional cross-axis combinations ---

        [Fact]
        public void RowCenter_ContainerHeight100_ItemHeight50()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='width:60px;height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 25) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight250_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:250px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 110) < 2);
        }

        [Fact]
        public void RowCenter_ContainerHeight300_ItemHeight30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:300px;width:200px'><div id='t' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 135) < 2);
        }

        [Fact]
        public void AlignSelfStretch_OverridesCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:center;height:120px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:stretch;width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 45) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 120) < 2);
        }

        [Fact]
        public void AlignSelfStart_OverridesFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='align-self:flex-start;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void ColumnStretch_MultipleItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void ColumnExplicitWidth_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void RowStretch_TwoItems_ContainerHeight150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:150px;width:300px'><div id='a' style='width:80px'></div><div id='b' style='width:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 150) < 2);
        }

        [Fact]
        public void MarginTopAuto_WithTwoItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:120px;width:200px'><div id='a' style='width:50px;height:30px;margin-top:auto'></div><div id='b' style='width:50px;height:40px;margin-top:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 80) < 2);
        }

        [Fact]
        public void MarginBottomAuto_WithTwoItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;height:120px;width:200px'><div id='a' style='width:50px;height:30px;margin-bottom:auto'></div><div id='b' style='width:50px;height:40px;margin-bottom:auto'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
        }
    }
}

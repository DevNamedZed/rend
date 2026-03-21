using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAlignCrossPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAlignCrossPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AlignItems_Start_Height30_InRow100_Y0()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2);
        }

        [Fact]
        public void AlignItems_End_Height30_InRow100_Y70()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;width:200px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void AlignItems_Center_Height30_InRow100_Y35()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void AlignItems_Stretch_InRow100_Height100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:stretch;width:200px'>
                    <div id='t'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AlignItems_Start_Height20_InRow150_Y0()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:start;width:200px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void AlignItems_End_Height20_InRow150_Y130()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:end;width:200px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 130) < 2);
        }

        [Fact]
        public void AlignItems_Center_Height20_InRow150_Y65()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:center;width:200px'>
                    <div id='t' style='height:20px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 65) < 2);
        }

        [Fact]
        public void AlignItems_End_Height40_InRow200_Y160()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:end;width:200px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 160) < 2);
        }

        [Fact]
        public void AlignItems_Center_Height50_InRow200_Y75()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:center;width:200px'>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 75) < 2);
        }

        [Fact]
        public void AlignItems_Stretch_InRow150_Height150()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:stretch;width:200px'>
                    <div id='t'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2);
        }

        [Fact]
        public void JustifyItems_Start_Width80_InCol200_X0()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void JustifyItems_End_Width80_InCol200_X120()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void JustifyItems_Center_Width80_InCol200_X60()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2);
        }

        [Fact]
        public void JustifyItems_Stretch_InCol200_Width200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:stretch;width:200px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void JustifyItems_End_Width60_InCol300_X240()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:end;width:300px'>
                    <div id='t' style='width:60px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 240) < 2);
        }

        [Fact]
        public void JustifyItems_Center_Width100_InCol300_X100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:center;width:300px'>
                    <div id='t' style='width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void JustifyItems_Start_Width100_InCol300_X0()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:start;width:300px'>
                    <div id='t' style='width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void AlignSelf_End_Overrides_AlignItems_Start()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;width:200px'>
                    <div id='t' style='align-self:end;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void AlignSelf_Center_Overrides_AlignItems_End()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;align-items:end;width:200px'>
                    <div id='t' style='align-self:center;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 50) < 2);
        }

        [Fact]
        public void JustifySelf_End_Overrides_JustifyItems_Start()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>
                    <div id='t' style='justify-self:end;width:80px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void JustifySelf_Center_Overrides_JustifyItems_End()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:end;width:300px'>
                    <div id='t' style='justify-self:center;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void PlaceItems_Center_Height40_Width80_InRow100_Col200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:center;width:200px'>
                    <div id='t' style='width:80px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2);
        }

        [Fact]
        public void PlaceItems_Center_Height50_Width60_InRow200_Col300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:200px;place-items:center;width:300px'>
                    <div id='t' style='width:60px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 75) < 2);
        }

        [Fact]
        public void MarginAuto_Centers_Height40_Width80_InRow100_Col200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='width:80px;height:40px;margin:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2);
        }

        [Fact]
        public void MarginAuto_Centers_Height20_Width60_InRow150_Col300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:150px;width:300px'>
                    <div id='t' style='width:60px;height:20px;margin:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 65) < 2);
        }

        [Fact]
        public void MarginLeft_Auto_Width80_InCol200_X120()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='width:80px;height:30px;margin-left:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void MarginLeft_Auto_Width60_InCol300_X240()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='width:60px;height:30px;margin-left:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 240) < 2);
        }

        [Fact]
        public void PlaceSelf_Center_Height40_Width80_InRow150_Col200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:150px;width:200px'>
                    <div id='t' style='place-self:center;width:80px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 55) < 2);
        }

        [Fact]
        public void PlaceSelf_Center_Height30_Width100_InRow200_Col300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:200px;width:300px'>
                    <div id='t' style='place-self:center;width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(item.ContentRect.Y - 85) < 2);
        }

        [Fact]
        public void TwoItems_AlignStart_And_AlignEnd_InSeparateRows()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px 150px;width:200px'>
                    <div id='a' style='align-self:start;height:30px'></div>
                    <div id='b' style='align-self:end;height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 210) < 2);
        }

        [Fact]
        public void TwoItems_JustifyStart_And_JustifyEnd_InSeparateColumns()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 300px;width:500px'>
                    <div id='a' style='justify-self:start;width:60px;height:30px'></div>
                    <div id='b' style='justify-self:end;width:80px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 420) < 2);
        }

        [Fact]
        public void AlignItems_End_Height50_InRow200_Y150()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:end;width:200px'>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 150) < 2);
        }

        [Fact]
        public void AlignItems_Center_Height40_InRow100_Y30()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:center;width:200px'>
                    <div id='t' style='height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2);
        }

        [Fact]
        public void AlignItems_Stretch_InRow200_Height200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:stretch;width:200px'>
                    <div id='t'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void JustifyItems_End_Width100_InCol200_X100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>
                    <div id='t' style='width:100px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void JustifyItems_Center_Width60_InCol200_X70()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>
                    <div id='t' style='width:60px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 70) < 2);
        }

        [Fact]
        public void JustifyItems_Stretch_InCol300_Width300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:stretch;width:300px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2);
        }

        [Fact]
        public void MarginTop_Auto_Height30_InRow100_PushesToBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='height:30px;margin-top:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void MarginRight_Auto_Width80_InCol200_X0()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='width:80px;height:30px;margin-right:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 0) < 2);
        }
    }
}

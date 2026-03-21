using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexAllJustifyPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexAllJustifyPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private string BuildRowHtml(string justifyContent, int itemCount)
        {
            var items = "";
            for (int index = 0; index < itemCount; index++)
            {
                items += $"<div id='item{index}' style='width:50px;height:30px'></div>";
            }
            var justifyPart = string.IsNullOrEmpty(justifyContent) ? "" : $"justify-content:{justifyContent};";
            return $"<body style='margin:0'><div style='display:flex;{justifyPart}width:400px'>{items}</div></body>";
        }

        private string BuildColumnHtml(string justifyContent, int itemCount)
        {
            var items = "";
            for (int index = 0; index < itemCount; index++)
            {
                items += $"<div id='item{index}' style='height:30px'></div>";
            }
            return $"<body style='margin:0'><div style='display:flex;flex-direction:column;justify-content:{justifyContent};width:200px;height:200px'>{items}</div></body>";
        }

        private void AssertX(Rend.Layout.LayoutBox root, string itemId, float expectedX)
        {
            var box = LayoutTestHelper.FindById(root, itemId);
            Assert.NotNull(box);
            _output.WriteLine($"{itemId}: expected X={expectedX}, actual X={box!.ContentRect.X}");
            Assert.True(System.Math.Abs(box.ContentRect.X - expectedX) < 2,
                $"{itemId} X: expected {expectedX}, got {box.ContentRect.X}");
        }

        private void AssertY(Rend.Layout.LayoutBox root, string itemId, float expectedY)
        {
            var box = LayoutTestHelper.FindById(root, itemId);
            Assert.NotNull(box);
            _output.WriteLine($"{itemId}: expected Y={expectedY}, actual Y={box!.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - expectedY) < 2,
                $"{itemId} Y: expected {expectedY}, got {box.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // flex-start: 1–6 items in 400px container
        // ──────────────────────────────────────────────

        [Fact]
        public void FlexStart_OneItem_FirstAt0()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 1));
            AssertX(root,"item0", 0);
        }

        [Fact]
        public void FlexStart_TwoItems_FirstAt0_LastAt50()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 2));
            AssertX(root,"item0", 0);
            AssertX(root,"item1", 50);
        }

        [Fact]
        public void FlexStart_ThreeItems_FirstAt0_LastAt100()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 3));
            AssertX(root,"item0", 0);
            AssertX(root,"item2", 100);
        }

        [Fact]
        public void FlexStart_FourItems_FirstAt0_LastAt150()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 4));
            AssertX(root,"item0", 0);
            AssertX(root,"item3", 150);
        }

        [Fact]
        public void FlexStart_FiveItems_FirstAt0_LastAt200()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 5));
            AssertX(root,"item0", 0);
            AssertX(root,"item4", 200);
        }

        [Fact]
        public void FlexStart_SixItems_FirstAt0_LastAt250()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-start", 6));
            AssertX(root,"item0", 0);
            AssertX(root,"item5", 250);
        }

        // ──────────────────────────────────────────────
        // flex-end: 1–6 items in 400px container
        // ──────────────────────────────────────────────

        [Fact]
        public void FlexEnd_OneItem_FirstAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 1));
            AssertX(root,"item0", 350);
        }

        [Fact]
        public void FlexEnd_TwoItems_FirstAt300_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 2));
            AssertX(root,"item0", 300);
            AssertX(root,"item1", 350);
        }

        [Fact]
        public void FlexEnd_ThreeItems_FirstAt250_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 3));
            AssertX(root,"item0", 250);
            AssertX(root,"item2", 350);
        }

        [Fact]
        public void FlexEnd_FourItems_FirstAt200_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 4));
            AssertX(root,"item0", 200);
            AssertX(root,"item3", 350);
        }

        [Fact]
        public void FlexEnd_FiveItems_FirstAt150_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 5));
            AssertX(root,"item0", 150);
            AssertX(root,"item4", 350);
        }

        [Fact]
        public void FlexEnd_SixItems_FirstAt100_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("flex-end", 6));
            AssertX(root,"item0", 100);
            AssertX(root,"item5", 350);
        }

        // ──────────────────────────────────────────────
        // center: 1–6 items in 400px container
        // ──────────────────────────────────────────────

        [Fact]
        public void Center_OneItem_FirstAt175()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 1));
            AssertX(root,"item0", 175);
        }

        [Fact]
        public void Center_TwoItems_FirstAt150_LastAt200()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 2));
            AssertX(root,"item0", 150);
            AssertX(root,"item1", 200);
        }

        [Fact]
        public void Center_ThreeItems_FirstAt125_LastAt225()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 3));
            AssertX(root,"item0", 125);
            AssertX(root,"item2", 225);
        }

        [Fact]
        public void Center_FourItems_FirstAt100_LastAt250()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 4));
            AssertX(root,"item0", 100);
            AssertX(root,"item3", 250);
        }

        [Fact]
        public void Center_FiveItems_FirstAt75_LastAt275()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 5));
            AssertX(root,"item0", 75);
            AssertX(root,"item4", 275);
        }

        [Fact]
        public void Center_SixItems_FirstAt50_LastAt300()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 6));
            AssertX(root,"item0", 50);
            AssertX(root,"item5", 300);
        }

        // ──────────────────────────────────────────────
        // space-between: 2–5 items in 400px container
        // ──────────────────────────────────────────────

        [Fact]
        public void SpaceBetween_TwoItems_FirstAt0_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 2));
            AssertX(root,"item0", 0);
            AssertX(root,"item1", 350);
        }

        [Fact]
        public void SpaceBetween_ThreeItems_FirstAt0_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 3));
            AssertX(root,"item0", 0);
            AssertX(root,"item2", 350);
        }

        [Fact]
        public void SpaceBetween_FourItems_FirstAt0_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 4));
            // free = 400 - 4*50 = 200, gaps = 3, gap = 200/3 = 66.667
            AssertX(root,"item0", 0);
            AssertX(root,"item3", 350);
        }

        [Fact]
        public void SpaceBetween_FiveItems_FirstAt0_LastAt350()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 5));
            // free = 400 - 5*50 = 150, gaps = 4, gap = 37.5
            AssertX(root,"item0", 0);
            AssertX(root,"item4", 350);
        }

        // ──────────────────────────────────────────────
        // space-around: 2–4 items in 400px container
        // ──────────────────────────────────────────────

        [Fact]
        public void SpaceAround_TwoItems_FirstAt75_LastAt275()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 2));
            // free = 300, perItem = 150, half = 75
            AssertX(root,"item0", 75);
            AssertX(root,"item1", 275);
        }

        [Fact]
        public void SpaceAround_ThreeItems_FirstAt41p67_LastAt308p33()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 3));
            // free = 250, perItem = 250/3 = 83.333, half = 41.667
            float perItem = 250f / 3;
            float halfSpace = perItem / 2;
            AssertX(root,"item0", halfSpace);
            AssertX(root,"item2", halfSpace + 2 * (50 + perItem));
        }

        [Fact]
        public void SpaceAround_FourItems_FirstAt25_LastAt325()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 4));
            // free = 200, perItem = 50, half = 25
            AssertX(root,"item0", 25);
            AssertX(root,"item3", 325);
        }

        // ──────────────────────────────────────────────
        // space-evenly: 2–4 items in 400px container
        // ──────────────────────────────────────────────

        [Fact]
        public void SpaceEvenly_TwoItems_FirstAt100_LastAt250()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 2));
            // free = 300, gaps = 3, gap = 100
            AssertX(root,"item0", 100);
            AssertX(root,"item1", 250);
        }

        [Fact]
        public void SpaceEvenly_ThreeItems_FirstAt62p5_LastAt287p5()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 3));
            // free = 250, gaps = 4, gap = 62.5
            AssertX(root,"item0", 62.5f);
            AssertX(root,"item2", 287.5f);
        }

        [Fact]
        public void SpaceEvenly_FourItems_FirstAt40_LastAt310()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 4));
            // free = 200, gaps = 5, gap = 40
            AssertX(root,"item0", 40);
            AssertX(root,"item3", 310);
        }

        // ──────────────────────────────────────────────
        // space-between: verify middle item positions
        // ──────────────────────────────────────────────

        [Fact]
        public void SpaceBetween_ThreeItems_MiddleAt175()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 3));
            // free = 250, gaps = 2, gap = 125; item1 at 0 + 50 + 125 = 175
            AssertX(root,"item1", 175);
        }

        [Fact]
        public void SpaceBetween_FourItems_SecondAt116p67()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 4));
            // free = 200, gaps = 3, gap = 66.667; item1 at 0 + 50 + 66.667 = 116.667
            float gap = 200f / 3;
            AssertX(root,"item1", 50 + gap);
        }

        [Fact]
        public void SpaceBetween_FourItems_ThirdAt233p33()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 4));
            float gap = 200f / 3;
            AssertX(root,"item2", 50 + gap + 50 + gap);
        }

        [Fact]
        public void SpaceBetween_FiveItems_SecondAt87p5()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 5));
            // gap = 150/4 = 37.5; item1 at 50 + 37.5 = 87.5
            AssertX(root,"item1", 87.5f);
        }

        [Fact]
        public void SpaceBetween_FiveItems_ThirdAt175()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 5));
            // item2 at 87.5 + 50 + 37.5 = 175
            AssertX(root,"item2", 175);
        }

        [Fact]
        public void SpaceBetween_FiveItems_FourthAt262p5()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-between", 5));
            AssertX(root,"item3", 262.5f);
        }

        // ──────────────────────────────────────────────
        // space-around: verify middle item positions
        // ──────────────────────────────────────────────

        [Fact]
        public void SpaceAround_ThreeItems_MiddleAt175()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 3));
            // free = 250, perItem = 250/3 = 83.333
            // item1 at half + 50 + perItem = 41.667 + 50 + 83.333 = 175
            AssertX(root,"item1", 175);
        }

        [Fact]
        public void SpaceAround_FourItems_SecondAt125()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 4));
            // perItem = 50, half = 25; item1 at 25 + 50 + 50 = 125
            AssertX(root,"item1", 125);
        }

        [Fact]
        public void SpaceAround_FourItems_ThirdAt225()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-around", 4));
            AssertX(root,"item2", 225);
        }

        // ──────────────────────────────────────────────
        // space-evenly: verify middle item positions
        // ──────────────────────────────────────────────

        [Fact]
        public void SpaceEvenly_ThreeItems_MiddleAt175()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 3));
            // gap = 62.5; item1 at 62.5 + 50 + 62.5 = 175
            AssertX(root,"item1", 175);
        }

        [Fact]
        public void SpaceEvenly_FourItems_SecondAt130()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 4));
            // gap = 40; item1 at 40 + 50 + 40 = 130
            AssertX(root,"item1", 130);
        }

        [Fact]
        public void SpaceEvenly_FourItems_ThirdAt220()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("space-evenly", 4));
            // item2 at 130 + 50 + 40 = 220
            AssertX(root,"item2", 220);
        }

        // ──────────────────────────────────────────────
        // Column direction: center, flex-end, space-between
        // with 2 items (height:30) in height:200 container
        // ──────────────────────────────────────────────

        [Fact]
        public void Column_Center_TwoItems_FirstAt70()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("center", 2));
            // free = 200 - 60 = 140, offset = 70
            AssertY(root, "item0", 70);
        }

        [Fact]
        public void Column_Center_TwoItems_LastAt100()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("center", 2));
            AssertY(root, "item1", 100);
        }

        [Fact]
        public void Column_FlexEnd_TwoItems_FirstAt140()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("flex-end", 2));
            // free = 140
            AssertY(root, "item0", 140);
        }

        [Fact]
        public void Column_FlexEnd_TwoItems_LastAt170()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("flex-end", 2));
            AssertY(root, "item1", 170);
        }

        [Fact]
        public void Column_SpaceBetween_TwoItems_FirstAt0()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("space-between", 2));
            AssertY(root, "item0", 0);
        }

        [Fact]
        public void Column_SpaceBetween_TwoItems_LastAt170()
        {
            var root = LayoutTestHelper.Layout(BuildColumnHtml("space-between", 2));
            AssertY(root, "item1", 170);
        }

        // ──────────────────────────────────────────────
        // Additional center middle item verification
        // ──────────────────────────────────────────────

        [Fact]
        public void Center_ThreeItems_MiddleAt175()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 3));
            AssertX(root,"item1", 175);
        }

        [Fact]
        public void Center_FiveItems_MiddleAt175()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 5));
            // offset = 75; item2 at 75 + 100 = 175
            AssertX(root,"item2", 175);
        }

        [Fact]
        public void Center_FourItems_SecondAt150()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 4));
            AssertX(root,"item1", 150);
        }

        [Fact]
        public void Center_FourItems_ThirdAt200()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 4));
            AssertX(root,"item2", 200);
        }

        [Fact]
        public void Center_SixItems_SecondAt100()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 6));
            AssertX(root,"item1", 100);
        }

        [Fact]
        public void Center_SixItems_FifthAt250()
        {
            var root = LayoutTestHelper.Layout(BuildRowHtml("center", 6));
            AssertX(root,"item4", 250);
        }
    }
}

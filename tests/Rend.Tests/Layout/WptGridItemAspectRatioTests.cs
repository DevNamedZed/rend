using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridItemAspectRatioTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridItemAspectRatioTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AspectRatio_2To1_WidthDeterminesHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AspectRatio_1To1_Square()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:150px;width:150px'>
                    <div id='t' style='aspect-ratio:1/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2);
        }

        [Fact]
        public void AspectRatio_1To2_TallItem()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;width:100px'>
                    <div id='t' style='aspect-ratio:1/2'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void AspectRatio_16To9_Widescreen()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:320px;width:320px'>
                    <div id='t' style='aspect-ratio:16/9'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 320) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        [Fact]
        public void AspectRatio_4To3_ClassicRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:240px;width:240px'>
                    <div id='t' style='aspect-ratio:4/3'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 240) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2);
        }

        [Fact]
        public void AspectRatio_Stretch_WidthFromTrack()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:180px;width:180px'>
                    <div id='t' style='aspect-ratio:3/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void AspectRatio_ExplicitWidth_OverridesTrack()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='width:100px;aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void AspectRatio_ExplicitHeight_WithJustifyStart_ComputesWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:start;width:300px'>
                    <div id='t' style='height:80px;aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void AspectRatio_AlignItemsStart_NoStretchHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:start;width:200px'>
                    <div id='t' style='aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
            Assert.True(target.ContentRect.Y < 2);
        }

        [Fact]
        public void AspectRatio_JustifyItemsCenter_ItemCentered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:center;width:300px'>
                    <div id='t' style='width:100px;aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void AspectRatio_SpanningItem_WidthIncludesGap()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;gap:20px;width:220px'>
                    <div id='t' style='grid-column:span 2;aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 220) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 110) < 2);
        }

        [Fact]
        public void AspectRatio_MinHeight_ClampsHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:4/1;min-height:80px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(target.ContentRect.Height >= 78);
        }

        [Fact]
        public void AspectRatio_MaxHeight_ClampsHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:1/1;max-height:80px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(target.ContentRect.Height <= 82);
        }

        [Fact]
        public void AspectRatio_SingleNumber_TreatedAsRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AspectRatio_WithPadding_ContentBoxRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1;padding:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float totalWidth = target.ContentRect.Width + target.PaddingLeft + target.PaddingRight;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2);
            float totalHeight = target.ContentRect.Height + target.PaddingTop + target.PaddingBottom;
            float expectedHeight = target.ContentRect.Width / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedHeight) < 2);
        }

        [Fact]
        public void AspectRatio_WithBorder_ContentBoxRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1;border:5px solid black'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float totalWidth = target.ContentRect.Width + target.BorderLeftWidth + target.BorderRightWidth;
            Assert.True(System.Math.Abs(totalWidth - 200) < 2);
            float expectedContentHeight = target.ContentRect.Width / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedContentHeight) < 2);
        }

        [Fact]
        public void AspectRatio_BorderBox_RatioAppliesToBorderBox()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1;box-sizing:border-box;width:200px;height:100px;padding:10px;border:5px solid black'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxWidth = target.ContentRect.Width + target.PaddingLeft + target.PaddingRight
                                 + target.BorderLeftWidth + target.BorderRightWidth;
            float borderBoxHeight = target.ContentRect.Height + target.PaddingTop + target.PaddingBottom
                                  + target.BorderTopWidth + target.BorderBottomWidth;
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2);
            Assert.True(System.Math.Abs(borderBoxHeight - 100) < 2);
        }

        [Fact]
        public void AspectRatio_TwoColumnGrid_EachItemRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:150px 150px;align-items:start;width:300px'>
                    <div id='a' style='aspect-ratio:1/1'></div>
                    <div id='b' style='aspect-ratio:3/1'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 150) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void AspectRatio_NamedArea_WidthFromArea()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-areas:""main"";width:200px'>
                    <div id='t' style='grid-area:main;aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AspectRatio_WithMargin_MarginDoesNotAffectRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1;margin:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 90) < 2);
        }

        [Fact]
        public void AspectRatio_ExplicitWidthAndHeight_IgnoresRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='width:120px;height:80px;aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void AspectRatio_AlignItemsEnd_PositionedAtBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:end;width:200px'>
                    <div id='t' style='aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 100) < 2);
        }

        [Fact]
        public void AspectRatio_AlignItemsCenter_VerticallyCentered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;align-items:center;width:200px'>
                    <div id='t' style='aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Y - 50) < 2);
        }

        [Fact]
        public void AspectRatio_3To2_PortraitOrientation()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:120px;width:120px'>
                    <div id='t' style='aspect-ratio:3/2'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void AspectRatio_AutoRowSizing_RowHeightFromRatio()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px;width:400px'>
                    <div id='a' style='aspect-ratio:2/1'></div>
                    <div id='b' style='height:50px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void AspectRatio_WithPaddingAndBorder_ContentBoxDefault()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1;padding:10px;border:5px solid black'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float horizontalExtra = target.PaddingLeft + target.PaddingRight
                                  + target.BorderLeftWidth + target.BorderRightWidth;
            float expectedContentWidth = 200 - horizontalExtra;
            Assert.True(System.Math.Abs(target.ContentRect.Width - expectedContentWidth) < 2);
            float expectedContentHeight = target.ContentRect.Width / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedContentHeight) < 2);
        }

        [Fact]
        public void AspectRatio_FixedRowTrack_StretchOverridesRatio()
        {
            // [CSS-SIZING-4 §5.1] Stretch in block axis determines height (200px),
            // aspect-ratio 2/1 derives width = 200*2 = 400px (overflows column track).
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void AspectRatio_JustifySelfEnd_PositionedAtRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div id='t' style='width:100px;aspect-ratio:2/1;justify-self:end'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2);
        }
    }
}

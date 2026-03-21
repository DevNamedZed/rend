using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for exact cross-axis positions of flex items under various
    /// align-items / align-self / auto-margin combinations in row and column directions.
    /// Covers CSS Flexbox Level 1 sections 8.3 (align-items), 8.4 (align-self),
    /// and 8.1 (auto margins on the cross axis).
    /// </summary>
    public class WptFlexAlignCrossPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexAlignCrossPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ── Row direction: stretch ──────────────────────────────────────

        // [CSS-FLEXBOX §8.3] stretch: item fills container cross size, Y=0
        [Fact]
        public void Row_Stretch_Container100_ItemAutoHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2, $"Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2, $"H={item.ContentRect.Height}");
        }

        // ── Row direction: flex-start ───────────────────────────────────

        // [CSS-FLEXBOX §8.3] flex-start in 100px container, item 30px
        [Fact]
        public void Row_FlexStart_Container100_Height30()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2, $"Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2, $"H={item.ContentRect.Height}");
        }

        // ── Row direction: flex-end ─────────────────────────────────────

        // [CSS-FLEXBOX §8.3] flex-end in 100px container, item 30px → Y=70
        [Fact]
        public void Row_FlexEnd_Container100_Height30()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2, $"Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 30) < 2, $"H={item.ContentRect.Height}");
        }

        // ── Row direction: center ───────────────────────────────────────

        // [CSS-FLEXBOX §8.3] center in 100px container, item 30px → Y=35
        [Fact]
        public void Row_Center_Container100_Height30()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 35) < 2, $"Y={item.ContentRect.Y}");
        }

        // ── Varying container heights ───────────────────────────────────

        // [CSS-FLEXBOX §8.3] flex-end in 150px container, item 30px → Y=120
        [Fact]
        public void Row_FlexEnd_Container150_Height30()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:150px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 120) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] center in 150px container, item 30px → Y=60
        [Fact]
        public void Row_Center_Container150_Height30()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:150px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 60) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] flex-end in 200px container, item 40px → Y=160
        [Fact]
        public void Row_FlexEnd_Container200_Height40()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:40px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 160) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] center in 200px container, item 40px → Y=80
        [Fact]
        public void Row_Center_Container200_Height40()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:40px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 80) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] flex-end in 250px container, item 50px → Y=200
        [Fact]
        public void Row_FlexEnd_Container250_Height50()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:250px;width:200px'>
                    <div id='t' style='width:50px;height:50px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 200) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] center in 250px container, item 50px → Y=100
        [Fact]
        public void Row_Center_Container250_Height50()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:250px;width:200px'>
                    <div id='t' style='width:50px;height:50px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 100) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] flex-end in 300px container, item 60px → Y=240
        [Fact]
        public void Row_FlexEnd_Container300_Height60()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:300px;width:200px'>
                    <div id='t' style='width:50px;height:60px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 240) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] center in 300px container, item 20px → Y=140
        [Fact]
        public void Row_Center_Container300_Height20()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:300px;width:200px'>
                    <div id='t' style='width:50px;height:20px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 140) < 2, $"Y={item.ContentRect.Y}");
        }

        // ── align-self overrides ────────────────────────────────────────

        // [CSS-FLEXBOX §8.4] align-self:center overrides container flex-start
        [Fact]
        public void Row_AlignSelfCenter_OverridesFlexStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='align-self:center;width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 35) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.4] align-self:flex-end overrides container flex-start
        [Fact]
        public void Row_AlignSelfEnd_OverridesFlexStart()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='align-self:flex-end;width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.4] align-self:flex-start overrides container flex-end
        [Fact]
        public void Row_AlignSelfStart_OverridesFlexEnd()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='t' style='align-self:flex-start;width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.4] align-self:stretch overrides container center
        [Fact]
        public void Row_AlignSelfStretch_OverridesCenter()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='align-self:stretch;width:50px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2, $"Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2, $"H={item.ContentRect.Height}");
        }

        // ── Cross-axis auto margins ─────────────────────────────────────

        // [CSS-FLEXBOX §8.1] margin-top:auto pushes item to bottom → Y=70
        [Fact]
        public void Row_MarginTopAuto_PushesToBottom()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='margin-top:auto;width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"margin-top:auto Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.1] margin-bottom:auto pushes item to top → Y=0
        [Fact]
        public void Row_MarginBottomAuto_PushesToTop()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='margin-bottom:auto;width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.1] both margin-top:auto + margin-bottom:auto → centered Y=35
        [Fact]
        public void Row_BothMarginsAuto_Centered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='margin-top:auto;margin-bottom:auto;width:50px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"both auto margins Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 35) < 2, $"Y={item.ContentRect.Y}");
        }

        // ── With padding ────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.3] flex-end with padding: Y offset accounts for padding
        [Fact]
        public void Row_FlexEnd_WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px;padding:10px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            // Total cross size = 30 + 10 + 10 = 50. Offset = 100 - 50 = 50.
            float borderBoxTop = item.ContentRect.Y - item.PaddingTop;
            _output.WriteLine($"flex-end+padding: contentY={item.ContentRect.Y} paddingTop={item.PaddingTop} borderBoxTop={borderBoxTop}");
            Assert.True(System.Math.Abs(borderBoxTop - 50) < 2, $"borderBoxTop={borderBoxTop}");
        }

        // [CSS-FLEXBOX §8.3] center with padding: centered on border box
        [Fact]
        public void Row_Center_WithPadding()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:20px;padding:10px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            // Total cross size = 20 + 10 + 10 = 40. Offset = (100-40)/2 = 30.
            float borderBoxTop = item.ContentRect.Y - item.PaddingTop;
            _output.WriteLine($"center+padding: borderBoxTop={borderBoxTop}");
            Assert.True(System.Math.Abs(borderBoxTop - 30) < 2, $"borderBoxTop={borderBoxTop}");
        }

        // ── With border ─────────────────────────────────────────────────

        // [CSS-FLEXBOX §8.3] flex-end with border: Y offset accounts for border
        [Fact]
        public void Row_FlexEnd_WithBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px;border:5px solid black'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            // Total cross size = 30 + 5 + 5 = 40. Offset = 100 - 40 = 60.
            float borderBoxTop = item.ContentRect.Y - item.PaddingTop - item.BorderTopWidth;
            _output.WriteLine($"flex-end+border: borderBoxTop={borderBoxTop}");
            Assert.True(System.Math.Abs(borderBoxTop - 60) < 2, $"borderBoxTop={borderBoxTop}");
        }

        // [CSS-FLEXBOX §8.3] center with border: centered on border box
        [Fact]
        public void Row_Center_WithBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:20px;border:5px solid black'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            // Total cross size = 20 + 5 + 5 = 30. Offset = (100-30)/2 = 35.
            float borderBoxTop = item.ContentRect.Y - item.PaddingTop - item.BorderTopWidth;
            _output.WriteLine($"center+border: borderBoxTop={borderBoxTop}");
            Assert.True(System.Math.Abs(borderBoxTop - 35) < 2, $"borderBoxTop={borderBoxTop}");
        }

        // ── Mixed items in one container ────────────────────────────────

        // [CSS-FLEXBOX §8.4] three items with different align-self values
        [Fact]
        public void Row_MixedAlignSelf_StartCenterEnd()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:100px;width:300px'>
                    <div id='a' style='align-self:flex-start;width:50px;height:30px'></div>
                    <div id='b' style='align-self:center;width:50px;height:30px'></div>
                    <div id='c' style='align-self:flex-end;width:50px;height:30px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y={itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 35) < 2, $"b Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 70) < 2, $"c Y={itemC.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.4] mixed items with different heights
        [Fact]
        public void Row_MixedAlignSelf_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:200px;width:300px'>
                    <div id='a' style='align-self:flex-start;width:50px;height:40px'></div>
                    <div id='b' style='align-self:center;width:50px;height:60px'></div>
                    <div id='c' style='align-self:flex-end;width:50px;height:20px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y) < 2, $"a Y={itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 70) < 2, $"b Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 180) < 2, $"c Y={itemC.ContentRect.Y}");
        }

        // ── Column direction: cross-axis X positions ────────────────────

        // [CSS-FLEXBOX §8.3] column center: item at X = (200 - 80) / 2 = 60
        [Fact]
        public void Column_Center_Container200_Width80()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:center;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2, $"X={item.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2, $"W={item.ContentRect.Width}");
        }

        // [CSS-FLEXBOX §8.3] column flex-end: item at X = 200 - 80 = 120
        [Fact]
        public void Column_FlexEnd_Container200_Width80()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-end;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2, $"X={item.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.3] column flex-start: item at X = 0
        [Fact]
        public void Column_FlexStart_Container200_Width80()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;align-items:flex-start;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X={item.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.3] column stretch: item fills container width
        [Fact]
        public void Column_Stretch_Container200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X={item.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2, $"W={item.ContentRect.Width}");
        }

        // ── Varying item heights with different alignments ──────────────

        // [CSS-FLEXBOX §8.3] center in 100px container, item 20px → Y=40
        [Fact]
        public void Row_Center_Container100_Height20()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:20px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 40) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] flex-end in 100px container, item 50px → Y=50
        [Fact]
        public void Row_FlexEnd_Container100_Height50()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:50px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 50) < 2, $"Y={item.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] center in 100px container, item 60px → Y=20
        [Fact]
        public void Row_Center_Container100_Height60()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:60px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Y - 20) < 2, $"Y={item.ContentRect.Y}");
        }

        // ── Auto margins in column direction ────────────────────────────

        // [CSS-FLEXBOX §8.1] column margin-left:auto pushes item right
        [Fact]
        public void Column_MarginLeftAuto_PushesRight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='margin-left:auto;width:80px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 120) < 2, $"X={item.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.1] column margin-right:auto pushes item left
        [Fact]
        public void Column_MarginRightAuto_PushesLeft()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='margin-right:auto;width:80px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X) < 2, $"X={item.ContentRect.X}");
        }

        // [CSS-FLEXBOX §8.1] column both auto margins → centered at X=60
        [Fact]
        public void Column_BothHorizontalMarginsAuto_Centered()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='margin-left:auto;margin-right:auto;width:80px;height:30px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.X - 60) < 2, $"X={item.ContentRect.X}");
        }

        // ── With padding and border combined ────────────────────────────

        // [CSS-FLEXBOX §8.3] center with padding+border
        [Fact]
        public void Row_Center_WithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:20px;padding:5px;border:5px solid black'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            // Total cross = 20 + 5*2 + 5*2 = 40. Offset = (100-40)/2 = 30.
            float borderBoxTop = item.ContentRect.Y - item.PaddingTop - item.BorderTopWidth;
            _output.WriteLine($"center+padding+border: borderBoxTop={borderBoxTop}");
            Assert.True(System.Math.Abs(borderBoxTop - 30) < 2, $"borderBoxTop={borderBoxTop}");
        }

        // ── Stretch does not override explicit height ────────────────────

        // [CSS-FLEXBOX §8.3] stretch with explicit height keeps the explicit height
        [Fact]
        public void Row_Stretch_ExplicitHeight_NotOverridden()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:40px'></div></div></body>");
            var item = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2, $"H={item.ContentRect.Height}");
        }

        // ── Multiple items with varying sizes ───────────────────────────

        // [CSS-FLEXBOX §8.3] flex-end with multiple items of different heights
        [Fact]
        public void Row_FlexEnd_MultipleItems_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:flex-end;height:150px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                    <div id='c' style='width:50px;height:20px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            var itemC = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 120) < 2, $"a Y={itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 90) < 2, $"b Y={itemB.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Y - 130) < 2, $"c Y={itemC.ContentRect.Y}");
        }

        // [CSS-FLEXBOX §8.3] center with multiple items of different heights
        [Fact]
        public void Row_Center_MultipleItems_DifferentHeights()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:60px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 35) < 2, $"a Y={itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 20) < 2, $"b Y={itemB.ContentRect.Y}");
        }
    }
}

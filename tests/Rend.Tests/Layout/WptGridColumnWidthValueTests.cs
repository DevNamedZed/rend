using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridColumnWidthValueTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridColumnWidthValueTests(ITestOutputHelper output) { _output = output; }

        private const float Tolerance = 2f;

        private static void AssertWidth(Rend.Layout.LayoutBox root, string id, float expected)
        {
            var box = LayoutTestHelper.FindById(root, id)!;
            Assert.True(System.Math.Abs(box.ContentRect.Width - expected) < Tolerance,
                $"'{id}' width: expected {expected}, got {box.ContentRect.Width}");
        }

        private static void AssertX(Rend.Layout.LayoutBox root, string id, float expected)
        {
            var box = LayoutTestHelper.FindById(root, id)!;
            Assert.True(System.Math.Abs(box.ContentRect.X - expected) < Tolerance,
                $"'{id}' X: expected {expected}, got {box.ContentRect.X}");
        }

        [Fact]
        public void SingleFixed_100px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px;width:400px'>
                    <div id='a' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
        }

        [Fact]
        public void SingleFixed_200px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;width:400px'>
                    <div id='a' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 200);
        }

        [Fact]
        public void TwoFixed_50px_150px()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 150px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 50);
            AssertWidth(root, "b", 150);
        }

        [Fact]
        public void ThreeFixed_100px_Each()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 100);
        }

        [Fact]
        public void TwoPercent_50_50_Of400()
        {
            // 50% + 50% of 400px = 200px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50% 50%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 200);
            AssertWidth(root, "b", 200);
        }

        [Fact]
        public void TwoPercent_25_75_Of400()
        {
            // 25% of 400 = 100px, 75% of 400 = 300px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:25% 75%;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 300);
        }

        [Fact]
        public void SingleFr_FillsContainer400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:400px'>
                    <div id='a' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 400);
        }

        [Fact]
        public void TwoFr_EqualSplit_In400()
        {
            // 1fr + 1fr in 400px = 200px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 200);
            AssertWidth(root, "b", 200);
        }

        [Fact]
        public void ThreeFr_EqualSplit_In300()
        {
            // 1fr + 1fr + 1fr in 300px = 100px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 100);
        }

        [Fact]
        public void Fr_1_2_Ratio_In300()
        {
            // 1fr + 2fr in 300px: 1fr=100px, 2fr=200px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr;width:300px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 200);
        }

        [Fact]
        public void FixedPlusFr_100px_1fr_In400()
        {
            // 100px fixed + 1fr in 400px: fr gets 300px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 300);
        }

        [Fact]
        public void FixedFrFixed_80_1fr_80_In400()
        {
            // 80px + 1fr + 80px in 400px: fr gets 400-80-80 = 240px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 80);
            AssertWidth(root, "b", 240);
            AssertWidth(root, "c", 80);
        }

        [Fact]
        public void Repeat3_100px()
        {
            // repeat(3, 100px) = 100px 100px 100px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,100px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 100);
        }

        [Fact]
        public void Repeat4_1fr_In400()
        {
            // repeat(4, 1fr) in 400px = 100px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 100);
            AssertWidth(root, "d", 100);
        }

        [Fact]
        public void Minmax_50px_1fr_In400()
        {
            // minmax(50px, 1fr) as single column in 400px: max wins, fills 400px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(50px,1fr);width:400px'>
                    <div id='a' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 400);
        }

        [Fact]
        public void Minmax_100px_200px_In400()
        {
            // minmax(100px, 200px) as single column in 400px: clamped to 200px max
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(100px,200px);width:400px'>
                    <div id='a' style='height:20px'></div></div></body>");
            float width = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(width >= 99 && width <= 201,
                $"'a' width: expected between 100 and 200, got {width}");
        }

        [Fact]
        public void Minmax_100px_200px_InNarrowContainer()
        {
            // minmax(100px, 200px) in 80px container: clamped to 100px minimum
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(100px,200px);width:80px'>
                    <div id='a' style='height:20px'></div></div></body>");
            float width = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(width >= 99,
                $"'a' width: expected >= 100 (min), got {width}");
        }

        [Fact]
        public void PercentPlusFr_30_1fr_In400()
        {
            // 30% of 400 = 120px, 1fr gets remainder = 280px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:30% 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 120);
            AssertWidth(root, "b", 280);
        }

        [Fact]
        public void Repeat2_50px_1fr_In400()
        {
            // repeat(2, 50px 1fr) => 50px 1fr 50px 1fr in 400px
            // 400 - 50 - 50 = 300px for 2 fr units = 150px each
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(2,50px 1fr);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 50);
            AssertWidth(root, "b", 150);
            AssertWidth(root, "c", 50);
            AssertWidth(root, "d", 150);
        }

        [Fact]
        public void AutoColumn_NoContent_FillsContainer()
        {
            // Single auto column with no content fills container
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:auto;width:400px'>
                    <div id='a' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 400);
        }

        [Fact]
        public void AutoPlusFixed_WithContent()
        {
            // auto + 100px in 400px: auto sizes to content (80px), fixed gets 100px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:auto 100px;width:400px'>
                    <div id='a' style='width:80px;height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "b", 100);
            float autoWidth = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(autoWidth >= 79,
                $"'a' auto width: expected >= 80, got {autoWidth}");
        }

        [Fact]
        public void MinContentColumn_WithFixedChild()
        {
            // min-content column sizes to the intrinsic minimum of its content
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:min-content 1fr;width:400px'>
                    <div id='a' style='width:60px;height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            float minContentWidth = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(minContentWidth <= 61,
                $"'a' min-content width: expected <= 61, got {minContentWidth}");
            Assert.True(minContentWidth >= 59,
                $"'a' min-content width: expected >= 59, got {minContentWidth}");
        }

        [Fact]
        public void MaxContentColumn_WithFixedChild()
        {
            // max-content column sizes to the intrinsic maximum of its content
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:max-content 1fr;width:400px'>
                    <div id='a' style='width:150px;height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 150);
        }

        [Fact]
        public void ThreeFixed_Positions()
        {
            // 100px 100px 100px: positions at 0, 100, 200
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 100);
            AssertX(root, "c", 200);
        }

        [Fact]
        public void TwoFr_Positions_In400()
        {
            // 1fr 1fr in 400px: positions at 0 and 200
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 200);
        }

        [Fact]
        public void FixedFrFixed_Positions_In400()
        {
            // 80px 1fr 80px in 400px: positions at 0, 80, 320
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:80px 1fr 80px;width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 80);
            AssertX(root, "c", 320);
        }

        [Fact]
        public void TwoMinmax_50_1fr_In400()
        {
            // minmax(50px, 1fr) minmax(50px, 1fr) in 400px: each gets 200px
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(50px,1fr) minmax(50px,1fr);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 200);
            AssertWidth(root, "b", 200);
        }

        [Fact]
        public void Repeat3_100px_Positions()
        {
            // repeat(3, 100px): positions at 0, 100, 200
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(3,100px);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 100);
            AssertX(root, "c", 200);
        }

        [Fact]
        public void Repeat4_1fr_Positions_In400()
        {
            // repeat(4, 1fr) in 400px: positions at 0, 100, 200, 300
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);width:400px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div>
                    <div id='d' style='height:20px'></div></div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 100);
            AssertX(root, "c", 200);
            AssertX(root, "d", 300);
        }
    }
}

using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridColumnWidthEdgeTests
    {
        private readonly ITestOutputHelper _output;
        public WptGridColumnWidthEdgeTests(ITestOutputHelper output) { _output = output; }

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
        public void FiveColumnsEqual_1fr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr 1fr;width:500px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div></div></body>",
                viewportWidth: 600);
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 100);
            AssertWidth(root, "d", 100);
            AssertWidth(root, "e", 100);
        }

        [Fact]
        public void SixColumnsEqual_1fr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr 1fr 1fr;width:600px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div><div id='f' style='height:20px'></div></div></body>",
                viewportWidth: 700);
            AssertWidth(root, "a", 100);
            AssertWidth(root, "f", 100);
        }

        [Fact]
        public void FourColumnsWithGap()
        {
            // 400px container, 4 columns, 3 gaps of 20px = 60px total gap. Remaining 340/4 = 85px each.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr 1fr;column-gap:20px;width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 85);
            AssertWidth(root, "d", 85);
        }

        [Fact]
        public void DifferentFixedWidths()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:50px 100px 150px 200px;width:500px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>",
                viewportWidth: 600);
            AssertWidth(root, "a", 50);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 150);
            AssertWidth(root, "d", 200);
        }

        [Fact]
        public void MixedPxAndPercent()
        {
            // 400px container: 100px fixed + 50% of 400 = 200px. Fr gets remainder: 400 - 100 - 200 = 100px.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 50% 1fr;width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 200);
            AssertWidth(root, "c", 100);
        }

        [Fact]
        public void Repeat5_1fr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);width:500px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div></div></body>",
                viewportWidth: 600);
            AssertWidth(root, "a", 100);
            AssertWidth(root, "c", 100);
            AssertWidth(root, "e", 100);
        }

        [Fact]
        public void Repeat6_1fr()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(6,1fr);width:600px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div><div id='f' style='height:20px'></div></div></body>",
                viewportWidth: 700);
            AssertWidth(root, "a", 100);
            AssertWidth(root, "d", 100);
            AssertWidth(root, "f", 100);
        }

        [Fact]
        public void Repeat2_100px_1fr()
        {
            // repeat(2, 100px 1fr) => 100px 1fr 100px 1fr. 400px - 200px fixed = 200px for 2fr => 100px each.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(2,100px 1fr);width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 100);
            AssertWidth(root, "d", 100);
        }

        [Fact]
        public void FrWithLargeGap()
        {
            // 400px container, 2 columns with 200px gap. Remaining 200px / 2 = 100px each.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;column-gap:200px;width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
        }

        [Fact]
        public void NarrowContainerWithFr()
        {
            // Very narrow: 60px with 3 columns of 1fr => 20px each.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:60px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 20);
            AssertWidth(root, "b", 20);
            AssertWidth(root, "c", 20);
        }

        [Fact]
        public void ZeroPxColumn()
        {
            // 0px column should have zero width; remaining 300px goes to 1fr.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:0px 1fr;width:300px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 0);
            AssertWidth(root, "b", 300);
        }

        [Fact]
        public void VeryWideColumn()
        {
            // One 500px column in a 600px container, the 1fr column gets 100px.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:500px 1fr;width:600px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>",
                viewportWidth: 700);
            AssertWidth(root, "a", 500);
            AssertWidth(root, "b", 100);
        }

        [Fact]
        public void PercentageExceeding100()
        {
            // 60% + 60% = 120%. Each column resolves to 60% of 400 = 240px.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:60% 60%;width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 240);
            AssertWidth(root, "b", 240);
        }

        [Fact]
        public void EqualFrWithPadding()
        {
            // Container 400px with 20px padding on each side => content width 360px. 2 x 1fr = 180px each.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px;padding:0 20px;box-sizing:border-box'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 180);
            AssertWidth(root, "b", 180);
        }

        [Fact]
        public void EqualFrWithBorder()
        {
            // Container 400px with 5px border on each side => content width 390px. 2 x 1fr = 195px each.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px;border:5px solid black;box-sizing:border-box'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 195);
            AssertWidth(root, "b", 195);
        }

        [Fact]
        public void FrFixedPercentMixed()
        {
            // 600px container: 100px fixed + 25% (150px) + 1fr (remaining 350px).
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 25% 1fr;width:600px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>",
                viewportWidth: 700);
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 150);
            AssertWidth(root, "c", 350);
        }

        [Fact]
        public void FourColumnPositions()
        {
            // 400px, 4 x 100px columns. X positions: 0, 100, 200, 300.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px 100px 100px;width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 100);
            AssertX(root, "c", 200);
            AssertX(root, "d", 300);
        }

        [Fact]
        public void FiveColumnPositionsWithGap()
        {
            // 500px, 5 x 1fr with 10px gap. 4 gaps = 40px. 460/5 = 92px each.
            // Positions: 0, 102, 204, 306, 408.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);column-gap:10px;width:500px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div></div></body>",
                viewportWidth: 600);
            AssertX(root, "a", 0);
            AssertX(root, "b", 102);
            AssertX(root, "c", 204);
            AssertX(root, "d", 306);
            AssertX(root, "e", 408);
        }

        [Fact]
        public void SixColumnPositionsFixed()
        {
            // 6 columns of 50px each in a 300px container.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(6,50px);width:300px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div><div id='f' style='height:20px'></div></div></body>");
            AssertX(root, "a", 0);
            AssertX(root, "b", 50);
            AssertX(root, "c", 100);
            AssertX(root, "d", 150);
            AssertX(root, "e", 200);
            AssertX(root, "f", 250);
        }

        [Fact]
        public void ThreeFrDifferentWeights()
        {
            // 600px, 1fr 2fr 3fr => total 6fr, each fr = 100px.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr 3fr;width:600px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>",
                viewportWidth: 700);
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 200);
            AssertWidth(root, "c", 300);
        }

        [Fact]
        public void FourFrWithGapPositions()
        {
            // 400px, 4 x 1fr with 8px gap. 3 gaps = 24px. 376/4 = 94px each.
            // Positions: 0, 102, 204, 306.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);column-gap:8px;width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 94);
            AssertWidth(root, "d", 94);
            AssertX(root, "a", 0);
            AssertX(root, "b", 102);
            AssertX(root, "c", 204);
            AssertX(root, "d", 306);
        }

        [Fact]
        public void FixedPlusMultipleFr()
        {
            // 500px: 200px + 1fr + 2fr. Remaining 300px, 1fr=100, 2fr=200.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px 1fr 2fr;width:500px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>",
                viewportWidth: 600);
            AssertWidth(root, "a", 200);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 200);
        }

        [Fact]
        public void FiveColumnsFixedPositionSequence()
        {
            // 5 different fixed widths: 30, 50, 70, 90, 110 = 350px total.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:30px 50px 70px 90px 110px;width:350px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div>
                    <div id='e' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 30);
            AssertWidth(root, "b", 50);
            AssertWidth(root, "c", 70);
            AssertWidth(root, "d", 90);
            AssertWidth(root, "e", 110);
            AssertX(root, "a", 0);
            AssertX(root, "b", 30);
            AssertX(root, "c", 80);
            AssertX(root, "d", 150);
            AssertX(root, "e", 240);
        }

        [Fact]
        public void PercentColumnsQuarters()
        {
            // 4 x 25% = 100%. 400px container => 100px each.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:25% 25% 25% 25%;width:400px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 100);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 100);
            AssertWidth(root, "d", 100);
        }

        [Fact]
        public void SingleVeryNarrowFr()
        {
            // 10px container with 1fr should still work.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:10px'>
                    <div id='a' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 10);
        }

        [Fact]
        public void MixedPercentAndFixedWithGap()
        {
            // 500px, gap:10px. 30% = 150px, 100px fixed, 1fr = 500-150-100-2*10 = 230px.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:30% 100px 1fr;column-gap:10px;width:500px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div>
                    <div id='c' style='height:20px'></div></div></body>",
                viewportWidth: 600);
            AssertWidth(root, "a", 150);
            AssertWidth(root, "b", 100);
            AssertWidth(root, "c", 230);
        }

        [Fact]
        public void FrColumnGapConsumesAllSpace()
        {
            // 200px container, 2 x 1fr, gap of 200px. Fr space = 200-200 = 0. Columns get 0px.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;column-gap:200px;width:200px'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            AssertWidth(root, "a", 0);
            AssertWidth(root, "b", 0);
        }

        [Fact]
        public void PaddingAndBorderCombined()
        {
            // Container 500px border-box, padding:10px, border:5px => content width = 500-20-10 = 470px. 2 x 1fr = 235px.
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:500px;padding:0 10px;border:5px solid black;box-sizing:border-box'>
                    <div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>",
                viewportWidth: 600);
            AssertWidth(root, "a", 235);
            AssertWidth(root, "b", 235);
        }
    }
}

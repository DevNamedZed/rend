using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests reproducing WPT css-flexbox/align-self-*.html tests.
    /// Each test verifies exact cross-axis positions of flex items.
    /// </summary>
    public class WptFlexboxAlignSelfTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxAlignSelfTests(ITestOutputHelper output) { _output = output; }

        // WPT align-self-001: flex-start → items at Y=0
        [Fact]
        public void align_self_001_flex_start()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;height:100px;width:100px}#test div{align-self:flex-start;height:50px;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            var items = new[] { "a", "b", "c", "d" };
            foreach (var id in items)
            {
                var box = LayoutTestHelper.FindById(r, id)!;
                Assert.True(box.ContentRect.Y < 2, $"{id} at top (Y={box.ContentRect.Y})");
                Assert.True(System.Math.Abs(box.ContentRect.Height - 50) < 2, $"{id} height=50");
                Assert.True(System.Math.Abs(box.ContentRect.Width - 25) < 2, $"{id} width=25");
            }
        }

        // WPT align-self-002: flex-end → items at Y=50
        [Fact]
        public void align_self_002_flex_end()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;height:100px;width:100px}#test div{align-self:flex-end;height:50px;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Y - 50) < 2, $"{id} at Y=50");
            }
        }

        // WPT align-self-003: center → items at Y=25
        [Fact]
        public void align_self_003_center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;height:100px;width:100px}#test div{align-self:center;height:50px;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Y - 25) < 2, $"{id} at Y=25");
            }
        }

        // WPT align-self-004: stretch → items fill 100px height
        [Fact]
        public void align_self_004_stretch()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;height:100px;width:100px}#test div{align-self:stretch;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Height - 100) < 2, $"{id} stretches to 100");
            }
        }

        // WPT align-self-005: stretch with explicit height → keeps height
        [Fact]
        public void align_self_005_stretch_explicit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;height:100px;width:100px}#test div{align-self:stretch;height:50px;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Height - 50) < 2, $"{id} keeps 50px");
            }
        }

        // WPT align-self-007: auto inherits flex-start from align-items
        [Fact]
        public void align_self_007_auto_flex_start()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;align-items:flex-start;height:100px;width:100px}#test div{align-self:auto;height:50px;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(LayoutTestHelper.FindById(r, id)!.ContentRect.Y < 2, $"{id} at top");
            }
        }

        // WPT align-self-008: auto inherits flex-end from align-items
        [Fact]
        public void align_self_008_auto_flex_end()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;align-items:flex-end;height:100px;width:100px}#test div{align-self:auto;height:50px;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Y - 50) < 2, $"{id} at Y=50");
            }
        }

        // WPT align-self-009: auto inherits center from align-items
        [Fact]
        public void align_self_009_auto_center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;align-items:center;height:100px;width:100px}#test div{align-self:auto;height:50px;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Y - 25) < 2, $"{id} at Y=25");
            }
        }

        // WPT align-self-011: auto inherits stretch from align-items
        [Fact]
        public void align_self_011_auto_stretch()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;align-items:stretch;height:100px;width:100px}#test div{align-self:auto;width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Height - 100) < 2, $"{id} stretches");
            }
        }

        // WPT align-self-012: initial value is auto (same as stretch default)
        [Fact]
        public void align_self_012_initial()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#test{display:flex;height:100px;width:100px}#test div{width:25px}</style>
                <div id='test'><div id='a'></div><div id='b'></div><div id='c'></div><div id='d'></div></div></body>");
            foreach (var id in new[] { "a", "b", "c", "d" })
            {
                Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, id)!.ContentRect.Height - 100) < 2, $"{id} default stretch");
            }
        }

        // Mixed: some items flex-start, some flex-end
        [Fact]
        public void mixed_start_end()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='a' style='align-self:flex-start;width:50px;height:30px'></div>
                    <div id='b' style='align-self:flex-end;width:50px;height:30px'></div>
                    <div id='c' style='align-self:center;width:50px;height:30px'></div>
                    <div id='d' style='width:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "d")!.ContentRect.Height - 100) < 2);
        }

        // Column: align-self affects X position
        [Fact]
        public void column_align_self_center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='align-self:center;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 60) < 2);
        }

        // Column: align-self flex-end
        [Fact]
        public void column_align_self_flex_end()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='align-self:flex-end;width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 120) < 2);
        }

        // Align-self with margin
        [Fact]
        public void align_self_with_margin()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='align-self:center;width:50px;height:30px;margin:5px'></div>
                </div></body>");
            // Center: (100-30-10)/2 = 30 + 5(margin) = 35
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t.Y={t.ContentRect.Y}");
            Assert.True(t.ContentRect.Y >= 29 && t.ContentRect.Y <= 40);
        }
    }
}

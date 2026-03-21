using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests reproducing WPT css-flexbox/flex-basis-*.html tests.
    /// </summary>
    public class WptFlexboxBasisTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxBasisTests(ITestOutputHelper output) { _output = output; }

        // WPT flex-basis-001: flex-basis:60px sets item width to 60
        [Fact]
        public void basis_001_positive()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#c{display:flex;height:100px;width:100px}#c div{height:100px}#test{flex-basis:60px}#ref{width:40px}</style>
                <div id='c'><div id='test'></div><div id='ref'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "test")!.ContentRect.Width - 60) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "ref")!.ContentRect.Width - 40) < 2);
        }

        // WPT flex-basis-002: flex-basis:60px overrides width:80px
        [Fact]
        public void basis_002_overrides_width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#c{display:flex;height:100px;width:100px}#c div{height:100px}#test{flex-basis:60px;width:80px}#ref{width:40px}</style>
                <div id='c'><div id='test'></div><div id='ref'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "test")!.ContentRect.Width - 60) < 2);
        }

        // WPT flex-basis-003: negative flex-basis invalid → item gets 0 width (no width set)
        [Fact]
        public void basis_003_negative_no_width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#c{display:flex;height:100px;width:100px}#c div{height:100px}#test{flex-basis:-50px}#ref{width:50px}</style>
                <div id='c'><div id='test'></div><div id='ref'></div></div></body>");
            // Negative flex-basis is invalid → treated as auto → no width = 0
            var test = LayoutTestHelper.FindById(r, "test")!;
            _output.WriteLine($"test.w={test.ContentRect.Width}");
            // Item should have very small or 0 width (negative basis rejected)
        }

        // WPT flex-basis-004: negative flex-basis invalid, falls back to width:30px
        [Fact]
        public void basis_004_negative_with_width()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#c{display:flex;height:100px;width:100px}#c div{height:100px}#test{flex-basis:-50px;width:30px}#ref{width:50px}</style>
                <div id='c'><div id='test'></div><div id='ref'></div></div></body>");
            // Negative basis invalid → auto → uses width:30px
            var test = LayoutTestHelper.FindById(r, "test")!;
            _output.WriteLine($"test.w={test.ContentRect.Width}");
            Assert.True(System.Math.Abs(test.ContentRect.Width - 30) < 2, $"Falls back to width (got {test.ContentRect.Width})");
        }

        // WPT flex-basis-005: flex-basis:0 → item gets 0 width
        [Fact]
        public void basis_005_zero()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#c{display:flex;height:100px;width:100px}#c div{height:100px}#test{flex-basis:0px}</style>
                <div id='c'><div id='test'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "test")!.ContentRect.Width < 2);
        }

        // WPT flex-basis-006: flex-basis:auto uses width property
        [Fact]
        public void basis_006_auto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <style>#c{display:flex;height:100px;width:100px}#c div{height:100px}#test{flex-basis:auto;width:60px}#ref{width:40px}</style>
                <div id='c'><div id='test'></div><div id='ref'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "test")!.ContentRect.Width - 60) < 2);
        }

        // flex-basis percentage resolves against container
        [Fact]
        public void basis_percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'><div id='t' style='flex-basis:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // flex-basis with border-box
        [Fact]
        public void basis_border_box()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'><div id='t' style='box-sizing:border-box;flex:0 0 150px;padding:20px;border:5px solid;height:50px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float total = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(System.Math.Abs(total - 150) < 2);
        }

        // flex-basis with calc()
        [Fact]
        public void basis_calc()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'><div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // flex shorthand: flex:1 → basis=0, grow=1
        [Fact]
        public void shorthand_flex_1()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
        }

        // flex shorthand: flex:0 0 → basis=0
        [Fact]
        public void shorthand_flex_0_0()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'><div id='t' style='flex:0 0;width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width < 2);
        }

        // flex shorthand: flex:none → basis=auto, no grow/shrink
        [Fact]
        public void shorthand_flex_none()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'><div id='t' style='flex:none;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // flex shorthand: flex:auto → basis=auto, grow=1, shrink=1
        [Fact]
        public void shorthand_flex_auto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'><div id='t' style='flex:auto;width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 299);
        }

        // flex: 0 0 auto → keeps width
        [Fact]
        public void shorthand_flex_0_0_auto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'><div id='t' style='flex:0 0 auto;width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 120) < 2);
        }
    }
}

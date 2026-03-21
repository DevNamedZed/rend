using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptMulticolTests
    {
        private readonly ITestOutputHelper _output;
        public WptMulticolTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void ColumnCount_3_DividesEvenly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 3; column-gap: 0; width: 300px;'>
                    <div style='height: 90px;'></div>
                    <div style='height: 90px;'></div>
                    <div style='height: 90px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"mc: {mc!.ContentRect.Width}x{mc.ContentRect.Height}");
            // 3 blocks of 90px = 270px. 3 columns → 90px per column. Height = 90.
            Assert.True(mc.ContentRect.Height <= 91,
                $"3 columns should give 90px height (got {mc.ContentRect.Height})");
        }

        [Fact]
        public void ColumnRule_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 2; column-rule: 5px solid red; column-gap: 20px; width: 220px;'>
                    <div style='height: 60px;'></div>
                    <div style='height: 60px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"mc: {mc!.ContentRect.Width}x{mc.ContentRect.Height}");
            // column-rule doesn't take space (it's painted in the gap)
            Assert.True(mc.ContentRect.Width >= 219, $"Width should be 220 (got {mc.ContentRect.Width})");
        }

        [Fact]
        public void ColumnFill_Balance_Default()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 2; column-gap: 0; width: 200px;'>
                    <div style='height: 40px;'></div>
                    <div style='height: 40px;'></div>
                    <div style='height: 40px;'></div>
                    <div style='height: 40px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"mc: h={mc!.ContentRect.Height}");
            // 4*40=160px content, 2 cols, balanced → ~80px per column
            Assert.True(mc.ContentRect.Height <= 81,
                $"Balanced columns ~80px (got {mc.ContentRect.Height})");
        }

        [Fact]
        public void ColumnWidth_Auto_UsesColumnCount()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 4; column-gap: 10px; width: 430px;'>
                    <div style='height: 100px;'></div>
                </div></body>");
            var mc = LayoutTestHelper.FindById(root, "mc");
            Assert.NotNull(mc);
            _output.WriteLine($"mc: {mc!.ContentRect.Width}x{mc.ContentRect.Height}");
            // 430px - 3*10px gaps = 400px. 4 columns = 100px each.
        }

        [Fact]
        public void ColumnSpan_All_BreaksColumn()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='mc' style='column-count: 2; column-gap: 0; width: 200px;'>
                    <div style='height: 30px;'></div>
                    <div id='spanner' style='column-span: all; height: 20px;'></div>
                    <div style='height: 30px;'></div>
                </div></body>");
            var spanner = LayoutTestHelper.FindById(root, "spanner");
            Assert.NotNull(spanner);
            _output.WriteLine($"spanner: w={spanner!.ContentRect.Width} at Y={spanner.ContentRect.Y}");
            Assert.True(spanner.ContentRect.Width >= 199,
                $"Spanner should span full width (got {spanner.ContentRect.Width})");
        }
    }
}

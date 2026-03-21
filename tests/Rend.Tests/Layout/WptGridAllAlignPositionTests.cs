using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridAllAlignPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridAllAlignPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ──────────────────────────────────────────────
        // align-items with varying row heights and item heights
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §5.1] align-items:start — item at top of 80px row
        [Fact]
        public void AlignItems_Start_Row80_Item20()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:80px;align-items:start;width:100px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 2, $"Expected Y near 0, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:start — item at top of 150px row
        [Fact]
        public void AlignItems_Start_Row150_Item40()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:150px;align-items:start;width:100px'>" +
                "<div id='t' style='height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 2, $"Expected Y near 0, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:end — item at bottom of 100px row
        [Fact]
        public void AlignItems_End_Row100_Item30()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:end;width:100px'>" +
                "<div id='t' style='height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = 100 - 30;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:end — item at bottom of 200px row
        [Fact]
        public void AlignItems_End_Row200_Item50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;align-items:end;width:100px'>" +
                "<div id='t' style='height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = 200 - 50;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:center — item centered in 80px row
        [Fact]
        public void AlignItems_Center_Row80_Item20()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:80px;align-items:center;width:100px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (80 - 20) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:center — item centered in 150px row
        [Fact]
        public void AlignItems_Center_Row150_Item50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:150px;align-items:center;width:100px'>" +
                "<div id='t' style='height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (150 - 50) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:center — item centered in 200px row
        [Fact]
        public void AlignItems_Center_Row200_Item40()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;align-items:center;width:100px'>" +
                "<div id='t' style='height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (200 - 40) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:stretch — item fills 100px row
        [Fact]
        public void AlignItems_Stretch_Row100()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:stretch;width:100px'>" +
                "<div id='t'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height near 100, got {target.ContentRect.Height}");
        }

        // [CSS-ALIGN §5.1] align-items:stretch — item fills 200px row
        [Fact]
        public void AlignItems_Stretch_Row200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;align-items:stretch;width:100px'>" +
                "<div id='t'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2,
                $"Expected height near 200, got {target.ContentRect.Height}");
        }

        // [CSS-ALIGN §5.1] align-items:end — item at bottom of 80px row with 40px item
        [Fact]
        public void AlignItems_End_Row80_Item40()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:80px;align-items:end;width:100px'>" +
                "<div id='t' style='height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = 80 - 40;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:start — item at top of 200px row with 50px item
        [Fact]
        public void AlignItems_Start_Row200_Item50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:200px;align-items:start;width:100px'>" +
                "<div id='t' style='height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 2, $"Expected Y near 0, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // justify-items with varying column widths and item widths
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §6.1] justify-items:start — item at left of 150px column
        [Fact]
        public void JustifyItems_Start_Col150_Item50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:150px;justify-items:start;width:150px'>" +
                "<div id='t' style='width:50px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 2,
                $"Expected X near 0, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.1] justify-items:start — item at left of 300px column
        [Fact]
        public void JustifyItems_Start_Col300_Item100()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:start;width:300px'>" +
                "<div id='t' style='width:100px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 2,
                $"Expected X near 0, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.1] justify-items:end — item at right of 200px column
        [Fact]
        public void JustifyItems_End_Col200_Item80()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>" +
                "<div id='t' style='width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 200 - 80;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.1] justify-items:end — item at right of 250px column
        [Fact]
        public void JustifyItems_End_Col250_Item50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:250px;justify-items:end;width:250px'>" +
                "<div id='t' style='width:50px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 250 - 50;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.1] justify-items:center — item centered in 150px column
        [Fact]
        public void JustifyItems_Center_Col150_Item50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:150px;justify-items:center;width:150px'>" +
                "<div id='t' style='width:50px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (150 - 50) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.1] justify-items:center — item centered in 300px column
        [Fact]
        public void JustifyItems_Center_Col300_Item100()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:center;width:300px'>" +
                "<div id='t' style='width:100px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (300 - 100) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.1] justify-items:stretch — item fills 200px column
        [Fact]
        public void JustifyItems_Stretch_Col200()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:stretch;width:200px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width near 200, got {target.ContentRect.Width}");
        }

        // [CSS-ALIGN §6.1] justify-items:stretch — item fills 250px column
        [Fact]
        public void JustifyItems_Stretch_Col250()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:250px;justify-items:stretch;width:250px'>" +
                "<div id='t' style='height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 250) < 2,
                $"Expected width near 250, got {target.ContentRect.Width}");
        }

        // [CSS-ALIGN §6.1] justify-items:end — item at right of 300px column with 80px item
        [Fact]
        public void JustifyItems_End_Col300_Item80()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:end;width:300px'>" +
                "<div id='t' style='width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 300 - 80;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.1] justify-items:center — item centered in 200px column with 80px item
        [Fact]
        public void JustifyItems_Center_Col200_Item80()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>" +
                "<div id='t' style='width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (200 - 80) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // ──────────────────────────────────────────────
        // align-self overrides (6 tests)
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §5.3] align-self:end overrides container align-items:start
        [Fact]
        public void AlignSelf_End_Overrides_Start()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:start;width:100px'>" +
                "<div id='t' style='align-self:end;height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = 100 - 30;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.3] align-self:center overrides container align-items:start
        [Fact]
        public void AlignSelf_Center_Overrides_Start()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:120px;align-items:start;width:100px'>" +
                "<div id='t' style='align-self:center;height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (120 - 40) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.3] align-self:start overrides container align-items:end
        [Fact]
        public void AlignSelf_Start_Overrides_End()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:end;width:100px'>" +
                "<div id='t' style='align-self:start;height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 2,
                $"Expected Y near 0, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.3] align-self:stretch overrides container align-items:center
        [Fact]
        public void AlignSelf_Stretch_Overrides_Center()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:center;width:100px'>" +
                "<div id='t' style='align-self:stretch'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height near 100, got {target.ContentRect.Height}");
        }

        // [CSS-ALIGN §5.3] align-self:end overrides container align-items:center
        [Fact]
        public void AlignSelf_End_Overrides_Center()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:150px;align-items:center;width:100px'>" +
                "<div id='t' style='align-self:end;height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = 150 - 50;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.3] align-self:center overrides container align-items:stretch
        [Fact]
        public void AlignSelf_Center_Overrides_Stretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px;align-items:stretch;width:100px'>" +
                "<div id='t' style='align-self:center;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = (100 - 20) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // justify-self overrides (6 tests)
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §6.3] justify-self:end overrides container justify-items:start
        [Fact]
        public void JustifySelf_End_Overrides_Start()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>" +
                "<div id='t' style='justify-self:end;width:60px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 200 - 60;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.3] justify-self:center overrides container justify-items:start
        [Fact]
        public void JustifySelf_Center_Overrides_Start()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:start;width:200px'>" +
                "<div id='t' style='justify-self:center;width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (200 - 80) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.3] justify-self:start overrides container justify-items:end
        [Fact]
        public void JustifySelf_Start_Overrides_End()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:end;width:200px'>" +
                "<div id='t' style='justify-self:start;width:60px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 2,
                $"Expected X near 0, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.3] justify-self:stretch overrides container justify-items:center
        [Fact]
        public void JustifySelf_Stretch_Overrides_Center()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:center;width:200px'>" +
                "<div id='t' style='justify-self:stretch;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width near 200, got {target.ContentRect.Width}");
        }

        // [CSS-ALIGN §6.3] justify-self:end overrides container justify-items:center
        [Fact]
        public void JustifySelf_End_Overrides_Center()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:250px;justify-items:center;width:250px'>" +
                "<div id='t' style='justify-self:end;width:100px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 250 - 100;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §6.3] justify-self:center overrides container justify-items:stretch
        [Fact]
        public void JustifySelf_Center_Overrides_Stretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:300px;justify-items:stretch;width:300px'>" +
                "<div id='t' style='justify-self:center;width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (300 - 80) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // ──────────────────────────────────────────────
        // place-items:center with various sizes
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §5.1/6.1] place-items:center — 200px col, 100px row, 50x30 item
        [Fact]
        public void PlaceItems_Center_Col200_Row100_Item50x30()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;place-items:center;width:200px'>" +
                "<div id='t' style='width:50px;height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (200 - 50) / 2f;
            float expectedY = (100 - 30) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1/6.1] place-items:center — 300px col, 200px row, 100x40 item
        [Fact]
        public void PlaceItems_Center_Col300_Row200_Item100x40()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:200px;place-items:center;width:300px'>" +
                "<div id='t' style='width:100px;height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (300 - 100) / 2f;
            float expectedY = (200 - 40) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1/6.1] place-items:center — 150px col, 150px row, 80x20 item
        [Fact]
        public void PlaceItems_Center_Col150_Row150_Item80x20()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:150px;grid-template-rows:150px;place-items:center;width:150px'>" +
                "<div id='t' style='width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (150 - 80) / 2f;
            float expectedY = (150 - 20) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1/6.1] place-items:center — 250px col, 80px row, 100x50 item
        [Fact]
        public void PlaceItems_Center_Col250_Row80_Item100x50()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:250px;grid-template-rows:80px;place-items:center;width:250px'>" +
                "<div id='t' style='width:100px;height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (250 - 100) / 2f;
            float expectedY = (80 - 50) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // margin:auto centering in grid cells
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §9.2] margin:auto centers both axes in grid cell
        [Fact]
        public void MarginAuto_CentersBothAxes()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>" +
                "<div id='t' style='width:60px;height:40px;margin:auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (200 - 60) / 2f;
            float expectedY = (100 - 40) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §9.2] margin-left:auto pushes item right
        [Fact]
        public void MarginLeftAuto_PushesRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:250px;width:250px'>" +
                "<div id='t' style='margin-left:auto;width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 250 - 80;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §9.2] margin-right:auto pushes item left
        [Fact]
        public void MarginRightAuto_PushesLeft()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:250px;width:250px'>" +
                "<div id='t' style='margin-right:auto;width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.X < 2,
                $"Expected X near 0, got {target.ContentRect.X}");
        }

        // [CSS-ALIGN §9.2] margin-top:auto pushes item down
        [Fact]
        public void MarginTopAuto_PushesDown()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:120px;width:100px'>" +
                "<div id='t' style='margin-top:auto;height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = 120 - 40;
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §9.2] margin-bottom:auto pushes item up
        [Fact]
        public void MarginBottomAuto_PushesUp()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:120px;width:100px'>" +
                "<div id='t' style='margin-bottom:auto;height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Y < 2,
                $"Expected Y near 0, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §9.2] margin:0 auto centers horizontally only
        [Fact]
        public void MarginHorizontalAuto_CentersX()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:80px;width:300px'>" +
                "<div id='t' style='width:100px;height:30px;margin:0 auto'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (300 - 100) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(target.ContentRect.Y < 2,
                $"Expected Y near 0, got {target.ContentRect.Y}");
        }

        // ──────────────────────────────────────────────
        // Two items with different alignment
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §5.3] Two items in separate rows with different align-self
        [Fact]
        public void TwoItems_DifferentAlignSelf_StartAndEnd()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:80px 80px;width:100px'>" +
                "<div id='a' style='align-self:start;height:20px'></div>" +
                "<div id='b' style='align-self:end;height:20px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.Y < 2,
                $"Item A expected Y near 0, got {itemA.ContentRect.Y}");
            float expectedBY = 80 + (80 - 20);
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - expectedBY) < 2,
                $"Item B expected Y near {expectedBY}, got {itemB.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.3] Two items: one centered, one stretched
        [Fact]
        public void TwoItems_CenterAndStretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:100px 100px;width:100px'>" +
                "<div id='a' style='align-self:center;height:30px'></div>" +
                "<div id='b' style='align-self:stretch'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedAY = (100 - 30) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - expectedAY) < 2,
                $"Item A expected Y near {expectedAY}, got {itemA.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 100) < 2,
                $"Item B expected height near 100, got {itemB.ContentRect.Height}");
        }

        // [CSS-ALIGN §6.3] Two items in separate columns with different justify-self
        [Fact]
        public void TwoItems_DifferentJustifySelf_StartAndEnd()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:150px 150px;width:300px'>" +
                "<div id='a' style='justify-self:start;width:40px;height:20px'></div>" +
                "<div id='b' style='justify-self:end;width:40px;height:20px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(itemA.ContentRect.X < 2,
                $"Item A expected X near 0, got {itemA.ContentRect.X}");
            float expectedBX = 150 + (150 - 40);
            Assert.True(System.Math.Abs(itemB.ContentRect.X - expectedBX) < 2,
                $"Item B expected X near {expectedBX}, got {itemB.ContentRect.X}");
        }

        // [CSS-ALIGN §6.3] Two items: one centered, one at start
        [Fact]
        public void TwoItems_CenterAndStart_Horizontal()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px 200px;width:400px'>" +
                "<div id='a' style='justify-self:center;width:60px;height:20px'></div>" +
                "<div id='b' style='justify-self:start;width:60px;height:20px'></div></div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float expectedAX = (200 - 60) / 2f;
            Assert.True(System.Math.Abs(itemA.ContentRect.X - expectedAX) < 2,
                $"Item A expected X near {expectedAX}, got {itemA.ContentRect.X}");
            float expectedBX = 200;
            Assert.True(System.Math.Abs(itemB.ContentRect.X - expectedBX) < 2,
                $"Item B expected X near {expectedBX}, got {itemB.ContentRect.X}");
        }

        // ──────────────────────────────────────────────
        // Additional combination tests
        // ──────────────────────────────────────────────

        // [CSS-ALIGN §5.1/6.1] align-items:end + justify-items:end — corner position
        [Fact]
        public void AlignEnd_JustifyEnd_CornerPosition()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;justify-items:end;width:200px'>" +
                "<div id='t' style='width:50px;height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 200 - 50;
            float expectedY = 100 - 30;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1/6.1] align-items:start + justify-items:end — top-right
        [Fact]
        public void AlignStart_JustifyEnd_TopRight()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:start;justify-items:end;width:200px'>" +
                "<div id='t' style='width:60px;height:25px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = 200 - 60;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(target.ContentRect.Y < 2,
                $"Expected Y near 0, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1/6.1] align-items:end + justify-items:start — bottom-left
        [Fact]
        public void AlignEnd_JustifyStart_BottomLeft()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;align-items:end;justify-items:start;width:200px'>" +
                "<div id='t' style='width:60px;height:25px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedY = 100 - 25;
            Assert.True(target.ContentRect.X < 2,
                $"Expected X near 0, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1/6.1] align-items:center + justify-items:center — dead center
        [Fact]
        public void AlignCenter_JustifyCenter_ExactCenter()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:300px;grid-template-rows:200px;align-items:center;justify-items:center;width:300px'>" +
                "<div id='t' style='width:80px;height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float expectedX = (300 - 80) / 2f;
            float expectedY = (200 - 40) / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.X - expectedX) < 2,
                $"Expected X near {expectedX}, got {target.ContentRect.X}");
            Assert.True(System.Math.Abs(target.ContentRect.Y - expectedY) < 2,
                $"Expected Y near {expectedY}, got {target.ContentRect.Y}");
        }

        // [CSS-ALIGN §5.1] align-items:stretch with explicit height does not stretch
        [Fact]
        public void AlignItems_Stretch_ExplicitHeight_NoStretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:100px;grid-template-rows:150px;align-items:stretch;width:100px'>" +
                "<div id='t' style='height:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"Expected height near 50 (explicit), got {target.ContentRect.Height}");
        }

        // [CSS-ALIGN §6.1] justify-items:stretch with explicit width does not stretch
        [Fact]
        public void JustifyItems_Stretch_ExplicitWidth_NoStretch()
        {
            var root = LayoutTestHelper.Layout(
                "<body style='margin:0'><div style='display:grid;grid-template-columns:200px;justify-items:stretch;width:200px'>" +
                "<div id='t' style='width:80px;height:20px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 80) < 2,
                $"Expected width near 80 (explicit), got {target.ContentRect.Width}");
        }
    }
}

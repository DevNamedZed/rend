using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-VARIABLES §3 https://drafts.csswg.org/css-variables/#using-variables</spec>
    public class WptCssVarFallbackTests
    {
        private readonly ITestOutputHelper _output;

        public WptCssVarFallbackTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void VarFallbackUsedWhenUndefined()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width: var(--missing, 80px); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2);
        }

        [Fact]
        public void VarDefinedValueUsedOverFallback()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--w: 120px;'><div id='t' style='width: var(--w, 50px); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2);
        }

        [Fact]
        public void VarWithoutFallbackWhenDefined()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--h: 60px;'><div id='t' style='width: 50px; height: var(--h);'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void NestedVarFallbackInnerUsed()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width: var(--a, var(--b, 90px)); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        [Fact]
        public void NestedVarMiddleDefined()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--b: 70px;'><div id='t' style='width: var(--a, var(--b, 50px)); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 70) < 2);
        }

        [Fact]
        public void NestedVarOuterDefined()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--a: 110px; --b: 70px;'><div id='t' style='width: var(--a, var(--b, 50px)); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 110) < 2);
        }

        [Fact]
        public void VarInMarginShorthand()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width: 400px;'><div style='--m: 20px;'><div id='t' style='margin: var(--m); width: 50px; height: 10px;'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"marginTop={box!.MarginTop} marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 20) < 2);
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 2);
        }

        [Fact]
        public void VarInPaddingShorthand()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--p: 15px;'><div id='t' style='padding: var(--p); width: 50px; height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"paddingTop={box!.PaddingTop} paddingLeft={box.PaddingLeft}");
            Assert.True(System.Math.Abs(box.PaddingTop - 15) < 2);
            Assert.True(System.Math.Abs(box.PaddingLeft - 15) < 2);
        }

        [Fact]
        public void VarInBorderShorthand()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--b: 2px solid red;'><div id='t' style='border: var(--b); width: 50px; height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"borderTop={box!.BorderTopWidth} borderLeft={box.BorderLeftWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 2) < 2);
            Assert.True(System.Math.Abs(box.BorderLeftWidth - 2) < 2);
        }

        [Fact]
        public void VarWithSpaceSeparatedFallback()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='margin: var(--undef, 10px 20px); width: 50px; height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"marginTop={box!.MarginTop} marginLeft={box.MarginLeft}");
            Assert.True(System.Math.Abs(box.MarginTop - 10) < 2);
            Assert.True(System.Math.Abs(box.MarginLeft - 20) < 2);
        }

        /// <spec>CSS-VARIABLES §3.1 https://drafts.csswg.org/css-variables/#cycles</spec>
        [Fact]
        public void VarCycleDetectionFallsBackToInitial()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <style>
                    #t { --a: var(--b); --b: var(--a); width: var(--a, 100px); height: 10px; }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        /// <spec>CSS-VARIABLES §3.1 https://drafts.csswg.org/css-variables/#cycles</spec>
        [Fact]
        public void VarSelfReferenceFallsBackToInitial()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <style>
                    #t { --x: var(--x); width: var(--x, 75px); height: 10px; }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 75) < 2);
        }

        /// <spec>CSS-VARIABLES §3.1 https://drafts.csswg.org/css-variables/#guaranteed-invalid</spec>
        [Fact]
        public void GuaranteedInvalidDirectFallback()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <style>
                    #t { width: var(--never-defined, 130px); height: 10px; }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2);
        }

        [Fact]
        public void VarInCalcResolvedBeforeCalc()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--w: calc(100px + 30px);'><div id='t' style='width: var(--w); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2);
        }

        [Fact]
        public void VarCalcValueAsFallback()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width: var(--missing, calc(50px + 40px)); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 90) < 2);
        }

        [Fact]
        public void VarOverriddenAtChildLevel()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--size: 200px;'><div style='--size: 60px;'><div id='t' style='width: var(--size); height: 10px;'></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 60) < 2);
        }

        [Fact]
        public void VarInheritedThreeLevels()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--val: 140px;'><div><div><div id='t' style='width: var(--val); height: 10px;'></div></div></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 140) < 2);
        }

        [Fact]
        public void VarOnRoot()
        {
            var root = LayoutTestHelper.Layout(
                @"<html><head><style>:root { --rw: 160px; }</style></head><body style='margin:0'><div id='t' style='width: var(--rw); height: 10px;'></div></body></html>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2);
        }

        [Fact]
        public void VarWithPercentageValue()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width: 200px; --pct: 50%;'><div id='t' style='width: var(--pct); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void VarWithEmValue()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='font-size: 20px; --size: 5em;'><div id='t' style='width: var(--size); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void VarForFlexBasis()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display: flex; width: 300px; --basis: 100px;'>
                <div id='t' style='flex: 0 0 var(--basis); height: 30px;'></div>
                <div style='flex: 1; height: 30px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void VarForGridTemplateColumns()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <style>
                    .grid { display: grid; grid-template-columns: var(--cols); width: 300px; }
                </style>
                <div class='grid' style='--cols: 100px 200px;'>
                    <div id='a' style='height: 20px;'></div>
                    <div id='b' style='height: 20px;'></div>
                </div></body>");
            var boxA = LayoutTestHelper.FindById(root, "a");
            var boxB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(boxA);
            Assert.NotNull(boxB);
            _output.WriteLine($"a.width={boxA!.ContentRect.Width} b.width={boxB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(boxA.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(boxB.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void VarForWidth()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--w: 180px;'><div id='t' style='width: var(--w); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2);
        }

        [Fact]
        public void VarForHeight()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--h: 45px;'><div id='t' style='width: 50px; height: var(--h);'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"height={box!.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 45) < 2);
        }

        [Fact]
        public void VarForMarginIndividualSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width: 400px; --ml: 25px; --mt: 15px;'>
                <div id='t' style='margin-left: var(--ml); margin-top: var(--mt); width: 50px; height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"marginLeft={box!.MarginLeft} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.MarginLeft - 25) < 2);
            Assert.True(System.Math.Abs(box.MarginTop - 15) < 2);
        }

        [Fact]
        public void VarForPaddingIndividualSides()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--pl: 12px; --pt: 8px;'>
                <div id='t' style='padding-left: var(--pl); padding-top: var(--pt); width: 50px; height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"paddingLeft={box!.PaddingLeft} paddingTop={box.PaddingTop}");
            Assert.True(System.Math.Abs(box.PaddingLeft - 12) < 2);
            Assert.True(System.Math.Abs(box.PaddingTop - 8) < 2);
        }

        [Fact]
        public void MultipleVarOnSameElement()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--w: 150px; --h: 40px; --m: 10px;'>
                <div id='t' style='width: var(--w); height: var(--h); margin: var(--m);'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width} height={box.ContentRect.Height} marginTop={box.MarginTop}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(box.MarginTop - 10) < 2);
        }

        [Fact]
        public void VarFallbackNotUsedWhenDefined()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--w: 200px;'>
                <div id='t' style='width: var(--w, 50px); height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void VarWithPercentageFallback()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='width: 300px;'>
                <div id='t' style='width: var(--undef, 50%); height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2);
        }

        [Fact]
        public void VarInheritedOverridesAtEachLevel()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <div style='--size: 300px;'>
                    <div style='--size: 200px;'>
                        <div style='--size: 100px;'>
                            <div id='t' style='width: var(--size); height: 10px;'></div>
                        </div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void VarWithCalcFallback()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width: var(--undef, calc(60px + 40px)); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2);
        }

        [Fact]
        public void VarCycleThroughThreeProperties()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <style>
                    #t { --a: var(--b); --b: var(--c); --c: var(--a); width: var(--a, 85px); height: 10px; }
                </style>
                <div id='t'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 85) < 2);
        }

        [Fact]
        public void VarDefinedOnSameElement()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='--w: 95px; width: var(--w); height: 10px;'></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 95) < 2);
        }

        [Fact]
        public void VarReferencingAnotherVar()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'>
                <style>
                    .parent { --base: 50px; --double: var(--base); }
                </style>
                <div class='parent'><div id='t' style='width: var(--double); height: 10px;'></div></div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"width={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 2);
        }

        [Fact]
        public void VarForBorderWidthLonghand()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='--bw: 5px;'>
                <div id='t' style='border-top-width: var(--bw); border-top-style: solid; border-top-color: black; width: 50px; height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(box);
            _output.WriteLine($"borderTop={box!.BorderTopWidth}");
            Assert.True(System.Math.Abs(box.BorderTopWidth - 5) < 2);
        }
    }
}

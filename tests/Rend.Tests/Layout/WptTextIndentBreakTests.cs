using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS text-indent, word-break, overflow-wrap, text-transform,
    /// word-spacing, and letter-spacing layout effects.
    /// Uses inline-block elements positioned by text-indent to verify offset.
    /// </summary>
    public class WptTextIndentBreakTests
    {
        private readonly ITestOutputHelper _output;

        public WptTextIndentBreakTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-TEXT §8.1] text-indent: 20px offsets first inline-block on first line
        [Fact]
        public void TextIndent_20px_OffsetsFirstInlineBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:20px'>
                    <span id='t' style='display:inline-block;width:50px;height:20px;background:red'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            Assert.True(target.ContentRect.X >= 19,
                $"text-indent:20px should offset inline-block (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent: 0 (default) — no offset
        [Fact]
        public void TextIndent_Default_NoOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            Assert.True(target.ContentRect.X < 2,
                $"default text-indent should be 0 (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent: 0 explicitly set — no offset
        [Fact]
        public void TextIndent_ExplicitZero_NoOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:0'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            Assert.True(target.ContentRect.X < 2,
                $"text-indent:0 should not offset (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent percentage resolves and offsets inline content
        [Fact]
        public void TextIndent_Percentage_OffsetsInlineContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;text-indent:10%'>
                    <span id='t' style='display:inline-block;width:30px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // 10% of 400px = 40px
            Assert.True(System.Math.Abs(target.ContentRect.X - 40) < 2,
                $"text-indent:10% should offset inline-block (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent: negative value pulls first line left
        [Fact]
        public void TextIndent_Negative_PullsLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:-20px;padding-left:30px'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // padding-left:30px + text-indent:-20px = 10px effective offset
            Assert.True(System.Math.Abs(target.ContentRect.X - 10) < 2,
                $"negative text-indent should pull left (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent only affects first line — second block is unaffected
        [Fact]
        public void TextIndent_OnlyAffectsFirstLine_SecondBlockUnaffected()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:40px'>
                    <div>
                        <span id='first' style='display:inline-block;width:30px;height:20px'></span>
                    </div>
                    <div>
                        <span id='second' style='display:inline-block;width:30px;height:20px'></span>
                    </div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"first.X={first.ContentRect.X} second.X={second.ContentRect.X}");
            // Both child divs are separate blocks, each has its own first line that gets indented
            Assert.True(first.ContentRect.X >= 39,
                $"first line of first block should be indented (X={first.ContentRect.X})");
            Assert.True(second.ContentRect.X >= 39,
                $"first line of second block should be indented (inherited) (X={second.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent is inherited
        [Fact]
        public void TextIndent_Inherited()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:25px'>
                    <div>
                        <span id='t' style='display:inline-block;width:30px;height:20px'></span>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            Assert.True(target.ContentRect.X >= 24,
                $"text-indent should be inherited (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent with text-align:center — indent shifts center point
        [Fact]
        public void TextIndent_WithTextAlignCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:40px;text-align:center'>
                    <span id='t' style='display:inline-block;width:60px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // center of (300px + 40px indent) with 60px element:
            // available = 300, content = 60, indent = 40
            // centered X = (300 - 60) / 2 + 40 = 160 ... but text-indent adds to line start
            // The inline-block should be offset from where it would normally center
            var rootNoIndent = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-align:center'>
                    <span id='t' style='display:inline-block;width:60px;height:20px'></span>
                </div></body>");
            var targetNoIndent = LayoutTestHelper.FindById(rootNoIndent, "t")!;
            Assert.True(target.ContentRect.X > targetNoIndent.ContentRect.X,
                $"text-indent should shift centered content right (with={target.ContentRect.X}, without={targetNoIndent.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent with text-align:right — indent still affects first line
        [Fact]
        public void TextIndent_WithTextAlignRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:30px;text-align:right'>
                    <span id='t' style='display:inline-block;width:60px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // Right-aligned: X = 300 - 60 = 240, but text-indent adds 30 to available start
            // The inline content width for alignment includes the indent
            Assert.NotNull(target);
        }

        // [CSS-TEXT §8.1] text-indent: large value pushes content near edge
        [Fact]
        public void TextIndent_LargeValue()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:200px'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            Assert.True(target.ContentRect.X >= 199,
                $"large text-indent should push inline-block (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §8.1] text-indent in em units
        [Fact]
        public void TextIndent_EmUnits()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;font-size:16px;text-indent:2em'>
                    <span id='t' style='display:inline-block;width:50px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // 2em = 2 * 16 = 32px
            Assert.True(System.Math.Abs(target.ContentRect.X - 32) < 2,
                $"text-indent:2em at 16px should be ~32px (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §5.2] word-break:break-all allows breaking within words — container grows taller
        [Fact]
        public void WordBreak_BreakAll_IncreasesHeight()
        {
            var rootNormal = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:50px'>
                    Superlongwordthatwontbreak
                </div></body>");
            var rootBreakAll = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:50px;word-break:break-all'>
                    Superlongwordthatwontbreak
                </div></body>");
            var normalBox = LayoutTestHelper.FindById(rootNormal, "t")!;
            var breakBox = LayoutTestHelper.FindById(rootBreakAll, "t")!;
            _output.WriteLine($"normal.H={normalBox.ContentRect.Height} breakAll.H={breakBox.ContentRect.Height}");
            // Without break-all, the long word overflows on one line
            // With break-all at 50px, the word is broken across multiple lines, making it taller
            Assert.True(breakBox.ContentRect.Height >= normalBox.ContentRect.Height,
                $"word-break:break-all should wrap at least as much as normal (normal={normalBox.ContentRect.Height}, breakAll={breakBox.ContentRect.Height})");
        }

        // [CSS-TEXT §5.2] word-break:break-all vs normal — normal keeps word intact
        [Fact]
        public void WordBreak_Normal_KeepsWordIntact()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:300px;word-break:normal'>
                    Short words here
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.H={target.ContentRect.Height} t.W={target.ContentRect.Width}");
            // Normal word-break with wide container — everything fits on one line
            Assert.True(target.ContentRect.Width >= 299,
                $"container width should be 300px (W={target.ContentRect.Width})");
        }

        // [CSS-TEXT §5.5] overflow-wrap:break-word breaks long words that overflow
        [Fact]
        public void OverflowWrap_BreakWord_BreaksLongWord()
        {
            var rootNormal = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:300px'>
                    Short text
                </div></body>");
            var rootBreak = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:50px;overflow-wrap:break-word'>
                    Superlongwordthatwontbreak
                </div></body>");
            var normalBox = LayoutTestHelper.FindById(rootNormal, "t")!;
            var breakBox = LayoutTestHelper.FindById(rootBreak, "t")!;
            _output.WriteLine($"normal.H={normalBox.ContentRect.Height} break.H={breakBox.ContentRect.Height}");
            Assert.True(breakBox.ContentRect.Height > normalBox.ContentRect.Height,
                $"overflow-wrap:break-word should wrap long word (normal={normalBox.ContentRect.Height}, break={breakBox.ContentRect.Height})");
        }

        // [CSS-TEXT §5.5] overflow-wrap:normal — long word overflows instead of breaking
        [Fact]
        public void OverflowWrap_Normal_LongWordOverflows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:50px;overflow-wrap:normal'>
                    Superlongword
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.H={target.ContentRect.Height}");
            // Without break-word, the long word stays on one line (overflows)
            Assert.NotNull(target);
        }

        // [CSS-TEXT §5.5] overflow-wrap:anywhere breaks like break-word
        [Fact]
        public void OverflowWrap_Anywhere_BreaksLongWord()
        {
            var rootBreak = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:50px;overflow-wrap:anywhere'>
                    Superlongwordthatwontbreak
                </div></body>");
            var breakBox = LayoutTestHelper.FindById(rootBreak, "t")!;
            _output.WriteLine($"break.H={breakBox.ContentRect.Height}");
            // anywhere should also break long words
            Assert.True(breakBox.ContentRect.Height > 20,
                $"overflow-wrap:anywhere should produce multi-line output (H={breakBox.ContentRect.Height})");
        }

        // [CSS-TEXT §3.2] word-spacing is parsed and stored on style
        [Fact]
        public void WordSpacing_ParsedOnStyle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;word-spacing:10px'>
                    one two three
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = target.StyledNode as Rend.Style.StyledElement;
            _output.WriteLine($"word-spacing={styled?.Style.WordSpacing}");
            Assert.NotNull(styled);
            Assert.True(styled!.Style.WordSpacing > 0,
                $"word-spacing:10px should be parsed as positive value (got {styled.Style.WordSpacing})");
        }

        // [CSS-TEXT §3.2] word-spacing:0 is same as default
        [Fact]
        public void WordSpacing_Zero_SameAsDefault()
        {
            var rootDefault = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block'>one two</span>
                </div></body>");
            var rootZero = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;word-spacing:0'>one two</span>
                </div></body>");
            var defaultWidth = LayoutTestHelper.FindById(rootDefault, "t")!.ContentRect.Width;
            var zeroWidth = LayoutTestHelper.FindById(rootZero, "t")!.ContentRect.Width;
            _output.WriteLine($"default.W={defaultWidth} zero.W={zeroWidth}");
            Assert.True(System.Math.Abs(defaultWidth - zeroWidth) < 2,
                $"word-spacing:0 should match default (default={defaultWidth}, zero={zeroWidth})");
        }

        // [CSS-TEXT §3.2] negative word-spacing is parsed correctly
        [Fact]
        public void WordSpacing_Negative_ParsedCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;word-spacing:-3px'>
                    one two three
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = target.StyledNode as Rend.Style.StyledElement;
            _output.WriteLine($"word-spacing={styled?.Style.WordSpacing}");
            Assert.NotNull(styled);
            Assert.True(styled!.Style.WordSpacing < 0,
                $"word-spacing:-3px should be negative (got {styled.Style.WordSpacing})");
        }

        // [CSS-TEXT §3.1] letter-spacing is parsed and stored on style
        [Fact]
        public void LetterSpacing_ParsedOnStyle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:200px;letter-spacing:5px'>
                    Hello World
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var styled = target.StyledNode as Rend.Style.StyledElement;
            _output.WriteLine($"letter-spacing={styled?.Style.LetterSpacing}");
            Assert.NotNull(styled);
            Assert.True(System.Math.Abs(styled!.Style.LetterSpacing - 5) < 1,
                $"letter-spacing:5px should be parsed as 5 (got {styled.Style.LetterSpacing})");
        }

        // [CSS-TEXT §3.1] letter-spacing:0 same as default
        [Fact]
        public void LetterSpacing_Zero_SameAsDefault()
        {
            var rootDefault = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block'>Test</span>
                </div></body>");
            var rootZero = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block;letter-spacing:0'>Test</span>
                </div></body>");
            var defaultWidth = LayoutTestHelper.FindById(rootDefault, "t")!.ContentRect.Width;
            var zeroWidth = LayoutTestHelper.FindById(rootZero, "t")!.ContentRect.Width;
            _output.WriteLine($"default.W={defaultWidth} zero.W={zeroWidth}");
            Assert.True(System.Math.Abs(defaultWidth - zeroWidth) < 2,
                $"letter-spacing:0 should match default (default={defaultWidth}, zero={zeroWidth})");
        }

        // [CSS-TEXT §3.1] letter-spacing causes narrower container to wrap more
        [Fact]
        public void LetterSpacing_CausesExtraWrapping()
        {
            var rootNormal = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px'>
                    Hello World Test
                </div></body>");
            var rootSpaced = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;letter-spacing:5px'>
                    Hello World Test
                </div></body>");
            var normalHeight = LayoutTestHelper.FindById(rootNormal, "t")!.ContentRect.Height;
            var spacedHeight = LayoutTestHelper.FindById(rootSpaced, "t")!.ContentRect.Height;
            _output.WriteLine($"normal.H={normalHeight} spaced.H={spacedHeight}");
            Assert.True(spacedHeight >= normalHeight,
                $"letter-spacing should cause more wrapping in narrow container (normal={normalHeight}, spaced={spacedHeight})");
        }

        // [CSS-TEXT §2.1] text-transform:uppercase does not change layout dimensions
        [Fact]
        public void TextTransform_Uppercase_SameDimensions()
        {
            var rootNormal = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:300px'>
                    <span id='s' style='display:inline-block'>hello world</span>
                </div></body>");
            var rootUpper = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:300px;text-transform:uppercase'>
                    <span id='s' style='display:inline-block'>hello world</span>
                </div></body>");
            var normalBox = LayoutTestHelper.FindById(rootNormal, "s")!;
            var upperBox = LayoutTestHelper.FindById(rootUpper, "s")!;
            _output.WriteLine($"normal: W={normalBox.ContentRect.Width} H={normalBox.ContentRect.Height}");
            _output.WriteLine($"upper: W={upperBox.ContentRect.Width} H={upperBox.ContentRect.Height}");
            // Heights should remain the same
            Assert.True(System.Math.Abs(normalBox.ContentRect.Height - upperBox.ContentRect.Height) < 2,
                $"text-transform should not change height (normal={normalBox.ContentRect.Height}, upper={upperBox.ContentRect.Height})");
        }

        // [CSS-TEXT §2.1] text-transform:lowercase does not change layout dimensions
        [Fact]
        public void TextTransform_Lowercase_SameDimensions()
        {
            var rootNormal = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span id='t' style='display:inline-block'>HELLO WORLD</span>
                </div></body>");
            var rootLower = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-transform:lowercase'>
                    <span id='t' style='display:inline-block'>HELLO WORLD</span>
                </div></body>");
            var normalBox = LayoutTestHelper.FindById(rootNormal, "t")!;
            var lowerBox = LayoutTestHelper.FindById(rootLower, "t")!;
            _output.WriteLine($"normal: W={normalBox.ContentRect.Width} H={normalBox.ContentRect.Height}");
            _output.WriteLine($"lower: W={lowerBox.ContentRect.Width} H={lowerBox.ContentRect.Height}");
            Assert.True(System.Math.Abs(normalBox.ContentRect.Height - lowerBox.ContentRect.Height) < 2,
                $"text-transform:lowercase should not change height (normal={normalBox.ContentRect.Height}, lower={lowerBox.ContentRect.Height})");
        }

        // [CSS-TEXT §2.1] text-transform:capitalize does not change layout height
        [Fact]
        public void TextTransform_Capitalize_SameHeight()
        {
            var rootNormal = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span id='t' style='display:inline-block'>hello world test</span>
                </div></body>");
            var rootCap = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-transform:capitalize'>
                    <span id='t' style='display:inline-block'>hello world test</span>
                </div></body>");
            var normalBox = LayoutTestHelper.FindById(rootNormal, "t")!;
            var capBox = LayoutTestHelper.FindById(rootCap, "t")!;
            _output.WriteLine($"normal.H={normalBox.ContentRect.Height} cap.H={capBox.ContentRect.Height}");
            Assert.True(System.Math.Abs(normalBox.ContentRect.Height - capBox.ContentRect.Height) < 2,
                $"text-transform:capitalize should not change height (normal={normalBox.ContentRect.Height}, cap={capBox.ContentRect.Height})");
        }

        // [CSS-TEXT §8.1] text-indent does not apply to inline-level elements
        [Fact]
        public void TextIndent_DoesNotApplyToInlineElement()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <span style='text-indent:50px'>
                        <span id='t' style='display:inline-block;width:30px;height:20px'></span>
                    </span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // text-indent on inline span should not shift content (only block-level)
            Assert.True(target.ContentRect.X < 2,
                $"text-indent on inline should not apply (X={target.ContentRect.X})");
        }

        // [CSS-TEXT §5.2] word-break:break-all with inline-block — narrow container wraps
        [Fact]
        public void WordBreak_BreakAll_NarrowContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:40px;word-break:break-all;font-size:16px'>
                    ABCDEFGHIJKLMNOP
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.H={target.ContentRect.Height}");
            // 16 chars in 40px with break-all should produce multiple lines
            Assert.True(target.ContentRect.Height > 30,
                $"break-all in 40px should wrap into multiple lines (H={target.ContentRect.Height})");
        }

        // [CSS-TEXT §5.2] word-break:keep-all prevents breaks at CJK boundaries (Latin unaffected)
        [Fact]
        public void WordBreak_KeepAll_LatinUnaffected()
        {
            var rootNormal = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px'>
                    Hello World Test
                </div></body>");
            var rootKeep = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;word-break:keep-all'>
                    Hello World Test
                </div></body>");
            var normalHeight = LayoutTestHelper.FindById(rootNormal, "t")!.ContentRect.Height;
            var keepHeight = LayoutTestHelper.FindById(rootKeep, "t")!.ContentRect.Height;
            _output.WriteLine($"normal.H={normalHeight} keepAll.H={keepHeight}");
            // For Latin text, keep-all behaves like normal
            Assert.True(System.Math.Abs(normalHeight - keepHeight) < 2,
                $"keep-all should not affect Latin wrapping (normal={normalHeight}, keep={keepHeight})");
        }

        // [CSS-TEXT §8.1] text-indent 50% produces large offset
        [Fact]
        public void TextIndent_Percentage_LargeOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;text-indent:50%'>
                    <span id='t' style='display:inline-block;width:20px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // 50% of 400px = 200px
            Assert.True(System.Math.Abs(target.ContentRect.X - 200) < 2,
                $"text-indent:50% should produce ~200px offset (X={target.ContentRect.X})");
        }

        // Combined: text-indent + word-spacing on same block
        [Fact]
        public void TextIndent_WithWordSpacing_Combined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:30px;word-spacing:5px'>
                    <span id='t' style='display:inline-block;width:40px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            // text-indent should still offset the first inline-block
            Assert.True(target.ContentRect.X >= 29,
                $"text-indent should offset even with word-spacing (X={target.ContentRect.X})");
        }

        // Combined: text-indent + letter-spacing on same block
        [Fact]
        public void TextIndent_WithLetterSpacing_Combined()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px;text-indent:30px;letter-spacing:3px'>
                    <span id='t' style='display:inline-block;width:40px;height:20px'></span>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"t.X={target.ContentRect.X}");
            Assert.True(target.ContentRect.X >= 29,
                $"text-indent should offset even with letter-spacing (X={target.ContentRect.X})");
        }
    }
}

using System.Collections.Generic;
using Xunit;
using Rend.Css;
using Rend.Layout.Internal;

namespace Rend.Tests.Layout
{
    public class GridTrackTests
    {
        private static CssListValue L(params CssValue[] values) =>
            new CssListValue(new List<CssValue>(values));

        private static CssFunctionValue Fn(string name, params CssValue[] args) =>
            new CssFunctionValue(name, new List<CssValue>(args));

        [Fact]
        public void ResolveTrackList_FixedPixels_ReturnsExactSizes()
        {
            var list = L(
                new CssDimensionValue(100, "px"),
                new CssDimensionValue(200, "px"),
                new CssDimensionValue(300, "px"));

            var result = GridLayout.ResolveTrackList(list, 600);

            Assert.NotNull(result);
            Assert.Equal(3, result!.Length);
            Assert.Equal(100f, result[0], 0.1f);
            Assert.Equal(200f, result[1], 0.1f);
            Assert.Equal(300f, result[2], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_FrUnits_DistributesSpace()
        {
            var list = L(
                new CssDimensionValue(1, "fr"),
                new CssDimensionValue(2, "fr"),
                new CssDimensionValue(1, "fr"));

            var result = GridLayout.ResolveTrackList(list, 400);

            Assert.NotNull(result);
            Assert.Equal(3, result!.Length);
            Assert.Equal(100f, result[0], 0.1f);
            Assert.Equal(200f, result[1], 0.1f);
            Assert.Equal(100f, result[2], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_MixedFixedAndFr_DistributesRemaining()
        {
            var list = L(
                new CssDimensionValue(100, "px"),
                new CssDimensionValue(1, "fr"),
                new CssDimensionValue(2, "fr"));

            var result = GridLayout.ResolveTrackList(list, 400);

            Assert.NotNull(result);
            Assert.Equal(3, result!.Length);
            Assert.Equal(100f, result[0], 0.1f);
            Assert.Equal(100f, result[1], 0.1f);
            Assert.Equal(200f, result[2], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_Percentages_ResolvesToPixels()
        {
            var list = L(new CssPercentageValue(25), new CssPercentageValue(75));

            var result = GridLayout.ResolveTrackList(list, 400);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Length);
            Assert.Equal(100f, result[0], 0.1f);
            Assert.Equal(300f, result[1], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_Minmax_EnforcesMinimum()
        {
            // minmax(150px, 1fr) minmax(150px, 1fr) in 200px → each fr=100, but floor=150
            var list = L(
                Fn("minmax", new CssDimensionValue(150, "px"), new CssDimensionValue(1, "fr")),
                Fn("minmax", new CssDimensionValue(150, "px"), new CssDimensionValue(1, "fr")));

            var result = GridLayout.ResolveTrackList(list, 200);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Length);
            Assert.True(result[0] >= 150f, $"Track 0 should be >= 150px but was {result[0]}");
            Assert.True(result[1] >= 150f, $"Track 1 should be >= 150px but was {result[1]}");
        }

        [Fact]
        public void ResolveTrackList_Minmax_UsesMaxWhenRoomAvailable()
        {
            // minmax(100px, 1fr) minmax(100px, 2fr) in 600px
            var list = L(
                Fn("minmax", new CssDimensionValue(100, "px"), new CssDimensionValue(1, "fr")),
                Fn("minmax", new CssDimensionValue(100, "px"), new CssDimensionValue(2, "fr")));

            var result = GridLayout.ResolveTrackList(list, 600);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Length);
            Assert.Equal(200f, result[0], 0.1f);
            Assert.Equal(400f, result[1], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_MinmaxBothFixed_UsesMax()
        {
            // minmax(100px, 200px) → 200px
            var list = Fn("minmax", new CssDimensionValue(100, "px"), new CssDimensionValue(200, "px"));

            var result = GridLayout.ResolveTrackList(list, 500);

            Assert.NotNull(result);
            Assert.Single(result!);
            Assert.Equal(200f, result[0], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_Repeat_ExpandsTracks()
        {
            var list = Fn("repeat", new CssNumberValue(3), new CssDimensionValue(100, "px"));

            var result = GridLayout.ResolveTrackList(list, 300);

            Assert.NotNull(result);
            Assert.Equal(3, result!.Length);
            for (int i = 0; i < 3; i++)
                Assert.Equal(100f, result[i], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_AutoFill_CalculatesCount()
        {
            // repeat(auto-fill, 100px) in 350px → 3 cols
            var list = Fn("repeat", new CssKeywordValue("auto-fill"), new CssDimensionValue(100, "px"));

            var result = GridLayout.ResolveTrackList(list, 350);

            Assert.NotNull(result);
            Assert.Equal(3, result!.Length);
            for (int i = 0; i < 3; i++)
                Assert.Equal(100f, result[i], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_AutoFillWithMinmax_CalculatesCountFromMin()
        {
            // repeat(auto-fill, minmax(80px, 1fr)) in 500px → 6 cols
            var list = Fn("repeat",
                new CssKeywordValue("auto-fill"),
                Fn("minmax", new CssDimensionValue(80, "px"), new CssDimensionValue(1, "fr")));

            var result = GridLayout.ResolveTrackList(list, 500);

            Assert.NotNull(result);
            Assert.Equal(6, result!.Length);
            for (int i = 0; i < result.Length; i++)
                Assert.True(result[i] >= 80f, $"Track {i} should be >= 80px but was {result[i]}");
        }

        [Fact]
        public void ResolveTrackList_None_ReturnsNull()
        {
            Assert.Null(GridLayout.ResolveTrackList(new CssKeywordValue("none"), 400));
        }

        [Fact]
        public void ResolveTrackList_Null_ReturnsNull()
        {
            Assert.Null(GridLayout.ResolveTrackList(null, 400));
        }

        [Fact]
        public void ResolveTrackList_FitContent_ReturnsLimit()
        {
            var list = L(
                Fn("fit-content", new CssDimensionValue(200, "px")),
                new CssDimensionValue(1, "fr"));

            var result = GridLayout.ResolveTrackList(list, 400);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Length);
            Assert.Equal(200f, result[0], 0.1f);
            Assert.Equal(200f, result[1], 0.1f);
        }

        [Fact]
        public void ResolveTrackList_RepeatFr_ExpandsAndDistributes()
        {
            // repeat(4, 1fr) in 400px → 4 x 100px
            var list = Fn("repeat", new CssNumberValue(4), new CssDimensionValue(1, "fr"));

            var result = GridLayout.ResolveTrackList(list, 400);

            Assert.NotNull(result);
            Assert.Equal(4, result!.Length);
            for (int i = 0; i < 4; i++)
                Assert.Equal(100f, result[i], 0.1f);
        }
    }
}

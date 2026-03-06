using System.Collections.Generic;

namespace Rend.VisualRegression.Infrastructure
{
    public static class VisualTestCatalog
    {
        private static readonly List<VisualTestCase> _cases = new();
        private static readonly object _lock = new();

        public static void Register(VisualTestCase testCase)
        {
            lock (_lock)
            {
                _cases.Add(testCase);
            }
        }

        public static IReadOnlyList<VisualTestCase> AllCases
        {
            get
            {
                EnsureInitialized();
                lock (_lock)
                {
                    return _cases.AsReadOnly();
                }
            }
        }

        private static void EnsureInitialized()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.BasicElementTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.BoxModelTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.TypographyTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.FlexboxTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.TableTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.ColorAndBackgroundTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.TransformAndEffectTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.PositioningTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.GridTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.OverflowTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.VisibilityDisplayTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.SizingTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.FloatTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.NestedLayoutTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.AdditionalLayoutTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.ExtendedTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.AdvancedTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.FormControlTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.SvgTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.InlineAndListTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.SemanticElementTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.RealWorldPatternTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.MultiColumnTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.ClipPathBorderImageTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.FilterTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.NewFeatureTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.TableAdvancedTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.WhitespaceTextTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.LayoutEdgeCaseTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.RealWorldPatternAdvancedTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.VisualEffectTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.BoxLayoutTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.FlexboxAdvancedTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.GridAdvancedTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.PositioningAdvancedTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.ColorPatternTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.UIComponentTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.OverflowClipTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.ShadowAndEffectTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.ResponsivePatternTests).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(TestCases.PixelPerfectTests).TypeHandle);
        }
    }
}

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
        }
    }
}

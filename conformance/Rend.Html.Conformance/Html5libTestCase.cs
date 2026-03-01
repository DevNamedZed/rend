using System.Collections.Generic;

namespace Rend.Html.Conformance
{
    public sealed class Html5libTestCase
    {
        public string SourceFile { get; set; } = "";
        public int Index { get; set; }
        public string Data { get; set; } = "";
        public List<string> Errors { get; set; } = new();
        public string ExpectedTree { get; set; } = "";
        public string? FragmentContext { get; set; }
        public string? FragmentNamespace { get; set; }
        public bool ScriptingEnabled { get; set; }

        public override string ToString()
        {
            var label = Data.Length > 60 ? Data.Substring(0, 60) + "..." : Data;
            label = label.Replace("\n", "\\n");
            return $"{SourceFile}[{Index}]: {label}";
        }
    }
}

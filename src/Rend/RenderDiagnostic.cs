namespace Rend
{
    /// <summary>Severity of a <see cref="RenderDiagnostic"/>.</summary>
    public enum RenderDiagnosticSeverity
    {
        /// <summary>Informational: rendering proceeded but a notable substitution or assumption was made.</summary>
        Info,

        /// <summary>Warning: output may differ from the author's intent (e.g. a font fell back).</summary>
        Warning,

        /// <summary>Error: a part of the document could not be rendered.</summary>
        Error
    }

    /// <summary>
    /// A diagnostic raised during rendering and delivered to <see cref="RenderOptions.OnDiagnostic"/>.
    /// Diagnostics surface conditions that would otherwise be silent — for example, a requested font
    /// that could not be resolved and was substituted.
    /// </summary>
    public sealed class RenderDiagnostic
    {
        /// <summary>Gets the severity of this diagnostic.</summary>
        public RenderDiagnosticSeverity Severity { get; }

        /// <summary>Gets the human-readable diagnostic message.</summary>
        public string Message { get; }

        /// <summary>Creates a new <see cref="RenderDiagnostic"/>.</summary>
        public RenderDiagnostic(RenderDiagnosticSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        /// <inheritdoc />
        public override string ToString() => $"[{Severity}] {Message}";
    }
}

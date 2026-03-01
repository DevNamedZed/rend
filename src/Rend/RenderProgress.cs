namespace Rend
{
    /// <summary>
    /// Reports progress during the HTML rendering pipeline.
    /// </summary>
    public sealed class RenderProgress
    {
        /// <summary>Overall progress percentage (0-100).</summary>
        public int Percentage { get; }

        /// <summary>Current pipeline stage.</summary>
        public RenderStage Stage { get; }

        /// <summary>Human-readable description of the current stage.</summary>
        public string Description { get; }

        public RenderProgress(int percentage, RenderStage stage, string description)
        {
            Percentage = percentage;
            Stage = stage;
            Description = description;
        }
    }

    /// <summary>
    /// The stages of the rendering pipeline.
    /// </summary>
    public enum RenderStage
    {
        Parsing,
        Styling,
        Layout,
        Rendering,
        Finishing
    }
}

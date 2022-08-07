namespace Cadmus.Export
{
    /// <summary>
    /// Renderer for any object represented by JSON (like a part or a fragment).
    /// This takes as input the JSON code, and renders it into some output format.
    /// </summary>
    public interface IJsonRenderer
    {
        /// <summary>
        /// Renders the specified JSON code.
        /// </summary>
        /// <param name="json">The input JSON.</param>
        /// <returns>Rendered output.</returns>
        string Render(string json);
    }
}

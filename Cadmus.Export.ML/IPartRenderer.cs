namespace Cadmus.Export.ML
{
    /// <summary>
    /// Cadmus part renderer. This takes as input the JSON representation of the
    /// part, and renders it into some output format.
    /// </summary>
    public interface IPartRenderer
    {
        /// <summary>
        /// Renders the specified JSON code.
        /// </summary>
        /// <param name="json">The input JSON.</param>
        /// <returns>Rendered output.</returns>
        string Render(string json);
    }
}

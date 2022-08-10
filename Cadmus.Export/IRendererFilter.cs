namespace Cadmus.Export
{
    /// <summary>
    /// Renderer filter. This represents any filter applied after the
    /// rendition of a Cadmus object.
    /// </summary>
    public interface IRendererFilter
    {
        /// <summary>
        /// Applies this filter to the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        string Apply(string text);
    }
}

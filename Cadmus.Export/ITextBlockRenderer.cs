using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Renderer of <see cref="TextBlockRow"/>'s.
    /// </summary>
    public interface ITextBlockRenderer
    {
        /// <summary>
        /// Renders the specified rows.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <returns>XML code.</returns>
        string Render(IEnumerable<TextBlockRow> rows);
    }
}

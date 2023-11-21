using System;
using System.Collections.Generic;
using Cadmus.Export.Filters;

namespace Cadmus.Export;

/// <summary>
/// Base class for <see cref="ITextBlockRenderer"/> implementations.
/// </summary>
public abstract class TextBlockRenderer
{
    /// <summary>
    /// Gets the optional filters to apply after the renderer completes.
    /// </summary>
    public IList<IRendererFilter> Filters { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRenderer"/> class.
    /// </summary>
    protected TextBlockRenderer()
    {
        Filters = new List<IRendererFilter>();
    }

    /// <summary>
    /// Renders the specified JSON code.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <param name="context">The optional renderer context.</param>
    /// <returns>Rendered output.</returns>
    protected abstract string DoRender(IEnumerable<TextBlockRow> rows,
        IRendererContext? context = null);

    /// <summary>
    /// Renders the specified JSON code.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <param name="context">The optional renderer context.</param>
    /// <returns>Rendered output.</returns>
    /// <exception cref="ArgumentNullException">rows</exception>
    public string Render(IEnumerable<TextBlockRow> rows,
        IRendererContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(rows);

        string result = DoRender(rows, context);

        if (Filters.Count > 0)
        {
            foreach (IRendererFilter filter in Filters)
                result = filter.Apply(result, context);
        }

        return result;
    }
}

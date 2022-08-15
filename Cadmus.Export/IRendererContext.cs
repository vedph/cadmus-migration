using Fusi.Tools;
using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Renderer context. This includes a generic metadata dictionary and
    /// a layer IDs map.
    /// </summary>
    /// <seealso cref="IHasDataDictionary" />
    public interface IRendererContext : IHasDataDictionary
    {
        /// <summary>
        /// Gets the target IDs dictionary, where keys are block layer IDs,
        /// and values are the corresponding block IDs.
        /// </summary>
        IDictionary<string, string> TargetIds { get; }
    }
}

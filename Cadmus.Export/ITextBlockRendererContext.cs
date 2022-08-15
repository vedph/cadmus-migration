using Fusi.Tools;
using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Context for <see cref="ITextBlockRenderer"/>'s.
    /// </summary>
    /// <seealso cref="IHasDataDictionary" />
    public interface ITextBlockRendererContext : IHasDataDictionary
    {
        /// <summary>
        /// Gets the target IDs dictionary, where keys are block layer IDs,
        /// and values are the corresponding block IDs.
        /// </summary>
        IDictionary<string, string> TargetIds { get; }
    }
}

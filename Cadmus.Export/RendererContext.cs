using Cadmus.Core.Storage;
using Fusi.Tools;
using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Default implementation of <see cref="IRendererContext"/>.
    /// </summary>
    public class RendererContext : DataDictionary, IRendererContext
    {
        /// <summary>
        /// Gets the target IDs dictionary, where keys are block layer IDs,
        /// and values are the corresponding block IDs.
        /// </summary>
        public IDictionary<string, string> TargetIds { get; }

        /// <summary>
        /// Gets or sets the optional Cadmus repository to be consumed by
        /// components using this context.
        /// </summary>
        public ICadmusRepository? Repository { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RendererContext"/>
        /// class.
        /// </summary>
        public RendererContext()
        {
            TargetIds = new Dictionary<string, string>();
        }
    }
}

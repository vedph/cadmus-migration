using Fusi.Tools;
using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Default implementation of <see cref="ITextBlockRendererContext"/>.
    /// </summary>
    public class TextBlockRendererContext : DataDictionary, ITextBlockRendererContext
    {
        public IDictionary<string, string> TargetIds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockRendererContext"/>
        /// class.
        /// </summary>
        public TextBlockRendererContext()
        {
            TargetIds = new Dictionary<string, string>();
        }
    }
}

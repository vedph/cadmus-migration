using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Base class for <see cref="IItemComposer"/> implementors.
    /// </summary>
    public abstract class ItemComposer
    {
        /// <summary>
        /// Gets or sets the optional text part flattener.
        /// </summary>
        public ITextPartFlattener? TextPartFlattener { get; set; }

        /// <summary>
        /// Gets or sets the optional text block renderer.
        /// </summary>
        public ITextBlockRenderer? TextBlockRenderer { get; set; }

        /// <summary>
        /// Gets the JSON renderers.
        /// </summary>
        public IDictionary<string, IJsonRenderer> JsonRenderers { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemComposer"/> class.
        /// </summary>
        protected ItemComposer()
        {
            JsonRenderers = new Dictionary<string, IJsonRenderer>();
        }

        /// <summary>
        /// Open the composer. The default implementation does nothing.
        /// </summary>
        public virtual void Open()
        {
        }

        /// <summary>
        /// Close the composer. The default implementation does nothing.
        /// </summary>
        public virtual void Close()
        {
        }
    }
}

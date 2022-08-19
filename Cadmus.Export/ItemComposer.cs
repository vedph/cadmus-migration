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
        /// Gets the output handled by this composer, or null if not opened.
        /// </summary>
        public ItemComposition? Output { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemComposer"/>
        /// class.
        /// </summary>
        protected ItemComposer()
        {
            JsonRenderers = new Dictionary<string, IJsonRenderer>();
        }

        /// <summary>
        /// Ensures the writer with the specified key exists in <see cref="Output"/>,
        /// creating it if required.
        /// </summary>
        /// <param name="key">The writer's key.</param>
        protected abstract void EnsureWriter(string key);

        /// <summary>
        /// Writes <paramref name="content"/> to the output writer with the
        /// specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The writer's key. If the writer does not exist,
        /// it will be created (via <see cref="EnsureWriter(string)"/>).</param>
        /// <param name="content">The content.</param>
        protected void WriteOutput(string key, string content)
        {
            EnsureWriter(key);
            Output?.Writers[key].Write(content);
        }

        /// <summary>
        /// Open the composer.
        /// </summary>
        /// <param name="output">The output object to use, or null to create
        /// a new one.</param>
        public virtual void Open(ItemComposition? output = null)
        {
            Output = output ?? new ItemComposition();
        }

        /// <summary>
        /// Close the composer. The default implementation does nothing.
        /// </summary>
        public virtual void Close()
        {
        }
    }
}

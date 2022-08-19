using Cadmus.Core;
using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Item composer interface. An item composer task is composing some or
    /// all the parts of an item together, building some specific output from it.
    /// For instance, in building a TEI document from text items with layers
    /// a composer can use an instance of <see cref="ITextBlockRenderer"/> to
    /// render the text part, and a number of <see cref="IJsonRenderer"/>'s
    /// to render the fragments from its layer parts. It then gets all their
    /// outputs and composes them into a set of files, by appending text output
    /// in a text file and each layer output in a separate file.
    /// </summary>
    public interface IItemComposer
    {
        /// <summary>
        /// Gets or sets the text part flattener used by this composer, if any.
        /// By definition this will match the item's part with role =
        /// <see cref="PartBase.BASE_TEXT_ROLE_ID"/> ("base-text").
        /// </summary>
        ITextPartFlattener? TextPartFlattener { get; set; }

        /// <summary>
        /// Gets or sets the text block renderer used by this composer,
        /// if any. By definition this will match the item's part with
        /// role = <see cref="PartBase.BASE_TEXT_ROLE_ID"/> ("base-text").
        /// </summary>
        ITextBlockRenderer? TextBlockRenderer { get; set; }

        /// <summary>
        /// Gets the JSON renderers used by this composer under their keys.
        /// Each key is built from the part type ID, eventually followed by
        /// <c>|</c> and its role ID when present.
        /// </summary>
        IDictionary<string, IJsonRenderer> JsonRenderers { get; }

        /// <summary>
        /// Gets the output handled by this composer, or null if not opened.
        /// </summary>
        public ItemComposition? Output { get; }

        /// <summary>
        /// Opens the composer output.
        /// </summary>
        /// <param name="output">The output object to use, or null to create
        /// a new one.</param>
        void Open(ItemComposition? output = null);

        /// <summary>
        /// Composes the output from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        void Compose(IItem item);

        /// <summary>
        /// Closes the composer output.
        /// </summary>
        void Close();
    }
}

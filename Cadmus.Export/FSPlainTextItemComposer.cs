using Cadmus.Core;
using Fusi.Tools.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cadmus.Export
{
    /// <summary>
    /// File-based plain text item composer. This is essentially used to export
    /// plain text documents from a text item into a single file, or one file
    /// per items group.
    /// </summary>
    [Tag("it.vedph.item-composer.txt")]
    public sealed class FSPlainTextItemComposer : ItemComposer, IItemComposer,
        IConfigurable<PlainTextItemComposerOptions>
    {
        private readonly TextBlockBuilder _blockBuilder;
        private PlainTextItemComposerOptions? _options;
        private string? _fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FSPlainTextItemComposer"/>
        /// class.
        /// </summary>
        public FSPlainTextItemComposer()
        {
            _blockBuilder = new();
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(PlainTextItemComposerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Ensures the writer with the specified key exists in
        /// <see cref="ItemComposer.Output" />, creating it if required.
        /// </summary>
        /// <param name="key">The writer's key.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        protected override void EnsureWriter(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (Output?.Writers.ContainsKey(key) != false) return;
            Output.Writers[key] = new StreamWriter(
                Path.Combine(_options!.OutputDirectory ?? "", key + ".txt"),
                false,
                Encoding.UTF8);

            WriteOutput(key, FillTemplate(_options.TextHead));
        }

        /// <summary>
        /// Invoked when the item's group changed since the last call to
        /// <see cref="ItemComposer.Compose" />. This can be used when processing
        /// grouped items in order.
        /// </summary>
        /// <param name="item">The new item.</param>
        /// <param name="prevGroupId">The previous group identifier.</param>
        protected override void OnGroupChanged(IItem item, string? prevGroupId)
        {
            // ignore if not grouping items
            if (_options?.ItemGrouping != true) return;

            // close previous writer if any and set new filename
            if (_fileName != null) Output?.FlushWriters(true);
            _fileName = item.GroupId;
        }

        /// <summary>
        /// Does the composition for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected override void DoCompose(IItem item)
        {
            if (Output == null) return;

            // first time we must build the filename
            if (_fileName == null)
            {
                _fileName =
                    _options!.ItemGrouping && !string.IsNullOrEmpty(item.GroupId)
                    ? item.GroupId
                    : item.Title;
            }

            // item head if any
            if (!string.IsNullOrEmpty(_options!.ItemHead))
                WriteOutput(_fileName, FillTemplate(_options.ItemHead));

            // text: there must be one
            IPart? textPart = item.Parts.Find(
                p => p.RoleId == PartBase.BASE_TEXT_ROLE_ID);
            if (textPart == null || TextPartFlattener == null ||
                TextBlockRenderer == null)
            {
                return;
            }

            // flatten and render into blocks
            var tr = TextPartFlattener.GetTextRanges(textPart,
                Array.Empty<IPart>(), null);

            List<TextBlockRow> rows =
                _blockBuilder.Build(tr.Item1, tr.Item2).ToList();

            // render blocks
            string result = TextBlockRenderer.Render(rows, Context);
            WriteOutput(_fileName, result);

            // item tail if any
            if (!string.IsNullOrEmpty(_options!.ItemTail))
                WriteOutput(_fileName, FillTemplate(_options.ItemTail));
        }

        /// <summary>
        /// Close the composer.
        /// </summary>
        public override void Close()
        {
            if (_fileName != null && !string.IsNullOrEmpty(_options!.TextTail))
            {
                WriteOutput(_fileName, FillTemplate(_options.TextTail));
            }
            base.Close();
        }
    }

    /// <summary>
    /// Options for <see cref="FSPlainTextItemComposer"/>.
    /// </summary>
    public class PlainTextItemComposerOptions
    {
        /// <summary>
        /// Gets or sets the optional text to write before each item. Its value
        /// can include placeholders in curly braces, corresponding to any of
        /// the metadata keys defined in the item composer's context.
        /// </summary>
        public string? ItemHead { get; set; }

        /// <summary>
        /// Gets or sets the optional text to write after each item. Its value
        /// can include placeholders in curly braces, corresponding to any of
        /// the metadata keys defined in the item composer's context.
        /// </summary>
        public string? ItemTail { get; set; }

        /// <summary>
        /// Gets or sets the optional text head. This is written at the start
        /// of the text flow. Its value can include placeholders in curly
        /// braces, corresponding to any of the metadata keys defined in
        /// the item composer's context.
        /// </summary>
        public string? TextHead { get; set; }

        /// <summary>
        /// Gets or sets the optional text tail. This is written at the end
        /// of the text flow. Its value can include placeholders in curly
        /// braces, corresponding to any of the metadata keys defined in
        /// the item composer's context.
        /// </summary>
        public string? TextTail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether item grouping is enabled.
        /// When enabled, whenever the group ID of the processed item changes
        /// in relation with the group ID of the latest processed item, a new
        /// file is created.
        /// </summary>
        public bool ItemGrouping { get; set; }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlainTextItemComposerOptions"/>
        /// class.
        /// </summary>
        public PlainTextItemComposerOptions()
        {
            OutputDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory);
        }
    }
}

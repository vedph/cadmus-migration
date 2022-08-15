using Cadmus.Core;
using Fusi.Tools.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cadmus.Export.ML
{
    /// <summary>
    /// File-based TEI standoff item composer. This just saves the text flows
    /// produced by a text item with its layers into a set of XML documents.
    /// <para>Tag: <c>it.vedph.item-composer.tei-standoff.fs</c>.</para>
    /// </summary>
    /// <seealso cref="ItemComposer" />
    /// <seealso cref="IItemComposer" />
    [Tag("it.vedph.item-composer.tei-standoff.fs")]
    public sealed class FSTeiStandoffItemComposer : TeiStandoffItemComposer,
        IItemComposer, IConfigurable<FSTeiStandoffItemComposerOptions>
    {
        private readonly Dictionary<string, TextWriter> _writers;
        private FSTeiStandoffItemComposerOptions? _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FSTeiStandoffItemComposer"/>
        /// class.
        /// </summary>
        public FSTeiStandoffItemComposer()
        {
            _writers = new Dictionary<string, TextWriter>();
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(FSTeiStandoffItemComposerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Open the composer.
        /// </summary>
        public override void Open()
        {
            Close();
        }

        /// <summary>
        /// Close the composer.
        /// </summary>
        public override void Close()
        {
            foreach (var p in _writers)
            {
                Metadata.Data[M_FLOW_KEY] = p.Key;

                string? tail = p.Key == PartBase.BASE_TEXT_ROLE_ID
                    ? FillTemplate(_options?.TextTail)
                    : FillTemplate(_options?.LayerTail);
                if (!string.IsNullOrEmpty(tail)) p.Value.WriteLine(tail);

                p.Value.Flush();
                p.Value.Close();
            }
            _writers.Clear();
        }

        /// <summary>
        /// Composes the output from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Composition result or null.</returns>
        protected override object? DoCompose(IItem item, object? context = null)
        {
            Dictionary<string, string> flows = RenderFlows(item);
            foreach (string key in flows.Keys)
            {
                if (!_writers.ContainsKey(key))
                {
                    Metadata.Data[M_FLOW_KEY] = key;

                    _writers[key] = new StreamWriter(
                        Path.Combine(_options!.OutputDirectory ?? "",
                                     key + ".xml"), false, Encoding.UTF8);

                    string? head = key == PartBase.BASE_TEXT_ROLE_ID
                        ? FillTemplate(_options.TextHead)
                        : FillTemplate(_options.LayerHead);
                    if (!string.IsNullOrEmpty(head))
                        _writers[key].WriteLine(head);
                }
                _writers[key].WriteLine(flows[key]);
            }
            return flows;
        }
    }

    /// <summary>
    /// Options for <see cref="FSTeiStandoffItemComposer"/>.
    /// </summary>
    public class FSTeiStandoffItemComposerOptions : TeiStandoffItemComposerOptions
    {
        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FSTeiStandoffItemComposerOptions"/>
        /// class.
        /// </summary>
        public FSTeiStandoffItemComposerOptions()
        {
            OutputDirectory = "";
        }
    }
}

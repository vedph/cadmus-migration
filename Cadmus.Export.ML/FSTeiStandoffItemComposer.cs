using Cadmus.Core;
using Fusi.Tools.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cadmus.Export.ML
{
    /// <summary>
    /// File-based TEI standoff item composer.
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
            foreach (TextWriter writer in _writers.Values)
            {
                writer.Flush();
                writer.Close();
            }
            _writers.Clear();
        }

        /// <summary>
        /// Composes the output from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="context">The context.</param>
        /// <returns>Composition result or null.</returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public object? Compose(IItem item, object? context = null)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            Dictionary<string, string> flows = RenderFlows(item);
            foreach (string key in flows.Keys)
            {
                if (!_writers.ContainsKey(key))
                {
                    _writers[key] = new StreamWriter(
                        Path.Combine(_options!.OutputDirectory ?? "",
                                     key + ".xml"), false, Encoding.UTF8);
                }
                _writers[key].WriteLine(flows[key]);
            }
            return flows;
        }
    }

    /// <summary>
    /// Options for <see cref="FSTeiStandoffItemComposer"/>.
    /// </summary>
    public class FSTeiStandoffItemComposerOptions
    {
        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        public string? OutputDirectory { get; set; }
    }
}

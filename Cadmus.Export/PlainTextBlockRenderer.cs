using Fusi.Tools.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cadmus.Export
{
    /// <summary>
    /// Plain text block renderer.
    /// <para>Tag: <c>it.vedph.text-block-renderer.txt</c>.</para>
    /// </summary>
    [Tag("it.vedph.text-block-renderer.txt")]
    public sealed class PlainTextBlockRenderer : ITextBlockRenderer,
        IConfigurable<PlainTextBlockRendererOptions>
    {
        private PlainTextBlockRendererOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlainTextBlockRenderer"/>
        /// class.
        /// </summary>
        public PlainTextBlockRenderer()
        {
            _options = new();
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(PlainTextBlockRendererOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Renders the specified rows.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <param name="context">The rendering context.</param>
        /// <returns>Rendition.</returns>
        /// <exception cref="ArgumentNullException">rows</exception>
        public string Render(IEnumerable<TextBlockRow> rows,
            IRendererContext context)
        {
            if (rows is null) throw new ArgumentNullException(nameof(rows));

            StringBuilder text = new();
            foreach (TextBlockRow row in rows)
            {
                text.AppendJoin(
                    _options.BlockSeparator,
                    row.Blocks.Select(b => b.Text))
                    .Append(_options.RowSeparator);
            }

            return text.ToString();
        }
    }

    /// <summary>
    /// Options for <see cref="PlainTextBlockRenderer"/>.
    /// </summary>
    public class PlainTextBlockRendererOptions
    {
        /// <summary>
        /// Gets or sets the text to be used as block separator.
        /// The default value is space.
        /// </summary>
        public string BlockSeparator { get; set; }

        /// <summary>
        /// Gets or sets the text to be used as block rows separator.
        /// The default value is a newline.
        /// </summary>
        public string RowSeparator { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PlainTextBlockRendererOptions"/> class.
        /// </summary>
        public PlainTextBlockRendererOptions()
        {
            BlockSeparator = " ";
            RowSeparator = Environment.NewLine;
        }
    }
}

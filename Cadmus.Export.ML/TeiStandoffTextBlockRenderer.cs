using Fusi.Tools;
using Fusi.Tools.Config;
using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cadmus.Export.ML
{
    /// <summary>
    /// Standoff TEI text blocks renderer.
    /// <para>Tag: <c>it.vedph.text-block-renderer.tei-standoff</c>.</para>
    /// </summary>
    [Tag("it.vedph.text-block-renderer.tei-standoff")]
    public sealed class TeiStandoffTextBlockRenderer : ITextBlockRenderer,
        IConfigurable<TeiStandoffTextBlockRendererOptions>
    {
        /// <summary>
        /// The name of the metadata placeholder for row's y number (1-N).
        /// </summary>
        public const string M_ROW_Y = "y";
        /// <summary>
        /// The name of the metadata placeholder for block's ID.
        /// </summary>
        public const string M_BLOCK_ID = "b";

        private readonly Dictionary<string, object> _nullCtxData;
        private TeiStandoffTextBlockRendererOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeiStandoffTextBlockRenderer"/>
        /// class.
        /// </summary>
        public TeiStandoffTextBlockRenderer()
        {
            _nullCtxData = new Dictionary<string, object>();
            _options = new TeiStandoffTextBlockRendererOptions();
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(TeiStandoffTextBlockRendererOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            _options = options;
        }

        private static string Xmlize(string text)
        {
            if (!text.Contains('<') &&
                !text.Contains('>') &&
                !text.Contains('&'))
            {
                return text;
            }

            StringBuilder sb = new(text);
            sb.Replace("&", "&amp;");
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            return sb.ToString();
        }

        private void RenderRowText(TextBlockRow row, StringBuilder text,
            IHasDataDictionary? context = null)
        {
            // open row
            if (!string.IsNullOrEmpty(_options.RowOpen))
            {
                text.Append(TextTemplate.FillTemplate(_options.RowOpen,
                    context?.Data ?? _nullCtxData));
            }

            // for each block in row
            foreach (TextBlock block in row.Blocks)
            {
                // open block
                if (!string.IsNullOrEmpty(_options.BlockOpen))
                {
                    if (context != null) context.Data[M_BLOCK_ID] = block.Id;
                    text.Append(TextTemplate.FillTemplate(_options.BlockOpen,
                        context?.Data ?? _nullCtxData));
                }

                text.Append(Xmlize(block.Text));

                // close block
                if (!string.IsNullOrEmpty(_options.BlockClose))
                {
                    text.Append(TextTemplate.FillTemplate(_options.BlockClose,
                        context?.Data ?? _nullCtxData));
                }
            }

            // close row
            if (!string.IsNullOrEmpty(_options.RowClose))
            {
                text.Append(TextTemplate.FillTemplate(_options.RowClose,
                    context?.Data ?? _nullCtxData));
            }
        }

        /// <summary>
        /// Renders the specified rows.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <param name="context">The optional context.</param>
        /// <returns>Rendition.</returns>
        /// <exception cref="ArgumentNullException">rows</exception>
        public string Render(IEnumerable<TextBlockRow> rows,
            IHasDataDictionary? context = null)
        {
            if (rows is null) throw new ArgumentNullException(nameof(rows));

            StringBuilder text = new();
            int y = 0;
            foreach (TextBlockRow row in rows)
            {
                if (context != null) context.Data[M_ROW_Y] = ++y;
                RenderRowText(row, text, context);
            }

            return text.ToString();
        }
    }

    /// <summary>
    /// Options for <see cref="TeiStandoffTextBlockRenderer"/>.
    /// </summary>
    public class TeiStandoffTextBlockRendererOptions
    {
        /// <summary>
        /// Gets or sets the code to insert at each row start.
        /// This can be a template, with placeholders delimited by curly braces.
        /// </summary>
        public string? RowOpen { get; set; }

        /// <summary>
        /// Gets or sets the code to insert at each row end.
        /// This can be a template, with placeholders delimited by curly braces.
        /// </summary>
        public string? RowClose { get; set; }

        /// <summary>
        /// Gets or sets the code to insert at each block start.
        /// This can be a template, with placeholders delimited by curly braces.
        /// </summary>
        public string? BlockOpen { get; set; }

        /// <summary>
        /// Gets or sets the code to insert at each block end.
        /// This can be a template, with placeholders delimited by curly braces.
        /// </summary>
        public string? BlockClose { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TeiStandoffTextBlockRendererOptions"/> class.
        /// </summary>
        public TeiStandoffTextBlockRendererOptions()
        {
            RowOpen = "<div>";
            RowClose = "</div>";
            BlockOpen = "<seg>";
            BlockClose = "</seg>";
        }
    }
}

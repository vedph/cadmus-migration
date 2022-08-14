using Fusi.Tools.Config;
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
        IConfigurable<SimpleTeiTextPartRendererOptions>
    {
        private SimpleTeiTextPartRendererOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeiStandoffTextBlockRenderer"/>
        /// class.
        /// </summary>
        public TeiStandoffTextBlockRenderer()
        {
            _options = new SimpleTeiTextPartRendererOptions
            {
                RowElement = "div",
                BlockElement = "seg"
            };
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(SimpleTeiTextPartRendererOptions options)
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

        private void RenderRowText(int y, TextBlockRow row, StringBuilder text)
        {
            // open row
            if (!string.IsNullOrEmpty(_options.RowElement))
            {
                text.Append('<').Append(_options.RowElement)
                   .Append(" xml:id=\"r").Append(y).Append("\">");
            }

            // for each block in row
            foreach (TextBlock block in row.Blocks)
            {
                // open block
                if (!string.IsNullOrEmpty(_options.BlockElement))
                {
                    text.Append('<').Append(_options.BlockElement)
                       .Append(" xml:id=\"").Append(block.Id).Append("\">");
                }

                text.Append(Xmlize(block.Text));

                // close block
                if (!string.IsNullOrEmpty(_options.BlockElement))
                {
                    text.Append("</").Append(_options.BlockElement).Append('>');
                }
            }

            // close row
            if (!string.IsNullOrEmpty(_options.RowElement))
            {
                text.Append("</").Append(_options.RowElement).AppendLine(">");
            }
        }

        /// <summary>
        /// Renders the specified rows.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <returns>XML code.</returns>
        /// <exception cref="ArgumentNullException">rows</exception>
        public string Render(IEnumerable<TextBlockRow> rows)
        {
            if (rows is null) throw new ArgumentNullException(nameof(rows));

            StringBuilder text = new();
            int y = 0;
            foreach (TextBlockRow row in rows)
            {
                y++;
                RenderRowText(y, row, text);
            }

            return text.ToString();
        }
    }

    /// <summary>
    /// Options for <see cref="TeiStandoffTextBlockRenderer"/>.
    /// </summary>
    public class SimpleTeiTextPartRendererOptions
    {
        /// <summary>
        /// Gets or sets the name of the XML element corresponding to a row
        /// of text blocks. If null, no element will be added.
        /// </summary>
        public string? RowElement { get; set; }

        /// <summary>
        /// Gets or sets the name of the XML element corresponding to a text
        /// block. If null, no element will be added.
        /// </summary>
        public string? BlockElement { get; set; }
    }
}

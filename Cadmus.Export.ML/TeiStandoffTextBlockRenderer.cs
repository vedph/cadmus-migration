using Fusi.Tools.Config;
using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Cadmus.Export.ML
{
    /// <summary>
    /// Standoff TEI text blocks renderer.
    /// <para>Tag: <c>it.vedph.text-block-renderer.tei-standoff</c>.</para>
    /// </summary>
    [Tag("it.vedph.text-block-renderer.tei-standoff")]
    public sealed class TeiStandoffTextBlockRenderer : TextBlockRenderer,
        ITextBlockRenderer,
        IConfigurable<TeiStandoffTextBlockRendererOptions>
    {
        /// <summary>
        /// The name of the metadata placeholder for the item's ordinal number
        /// (1-N). This is set externally when repeatedly using this renderer
        /// for multiple items.
        /// </summary>
        public const string M_ITEM_NR = "item-nr";
        /// <summary>
        /// The name of the metadata placeholder for each block's fragment ID.
        /// A fragment ID is built with item number + layer ID + fragment index,
        /// all separated by underscore and prefixed by an initial single <c>f</c>
        /// (e.g. <c>f1_2_3</c>).
        /// </summary>
        public const string M_FRAGMENT_ID = "target-id";
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

        private static string GetLayerIdPrefix(string id)
        {
            int i = id.Length;
            while (i > 0 && (id[i - 1] >= '0' && id[i - 1] <= '9')) i--;
            return id[0..i];
        }

        private void RenderRowText(TextBlockRow row, StringBuilder text,
            IRendererContext context)
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
                context!.Data[M_BLOCK_ID] = block.Id;

                if (block.LayerIds.Count > 0)
                {
                    foreach (string id in block.LayerIds)
                    {
                        string layerPrefix = GetLayerIdPrefix(id);

                        string frId = TeiStandoffItemComposer.BuildFragmentId(
                            (int)context.Data[M_ITEM_NR],
                            context.LayerIds[layerPrefix],
                            int.Parse(block.Id, CultureInfo.InvariantCulture));

                        context.FragmentIds[id] = frId;
                        context.Data[M_FRAGMENT_ID] = frId;
                    }

                    // open block
                    if (!string.IsNullOrEmpty(_options.BlockOpen))
                    {
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
                else
                {
                    context.Data.Remove(M_FRAGMENT_ID);
                    text.Append(Xmlize(block.Text));
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
        /// <param name="context">The rendering context.</param>
        /// <returns>Rendition.</returns>
        /// <exception cref="ArgumentNullException">rows</exception>
        protected override string DoRender(IEnumerable<TextBlockRow> rows,
            IRendererContext? context = null)
        {
            if (rows is null) throw new ArgumentNullException(nameof(rows));

            StringBuilder text = new();
            int y = 0;
            foreach (TextBlockRow row in rows)
            {
                if (context != null) context.Data[M_ROW_Y] = ++y;
                RenderRowText(row, text, context!);
            }

            return text.ToString();
        }
    }

    #region TeiStandoffTextBlockRendererOptions
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
            RowOpen = "<div xml:id=\"r{" +
                TeiStandoffTextBlockRenderer.M_ITEM_NR + "}_{" +
                TeiStandoffTextBlockRenderer.M_ROW_Y + "}\">";
            RowClose = "</div>";
            BlockOpen = "<seg xml:id=\"{" +
                TeiStandoffTextBlockRenderer.M_FRAGMENT_ID + "}\">";
            BlockClose = "</seg>";
        }
    }
    #endregion
}

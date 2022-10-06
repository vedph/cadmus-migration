using Cadmus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;

namespace Cadmus.Export.ML
{
    /// <summary>
    /// Base class for TEI standoff item composers. This deals with text items,
    /// using an <see cref="ITextPartFlattener"/> to flatten it with all its
    /// layers, and an <see cref="ITextBlockRenderer"/> to render the resulting
    /// text blocks into XML. It then uses a number of <see cref="IJsonRenderer"/>'s
    /// to render each layer's fragment in its own XML document. So, ultimately
    /// this produces several XML documents, one for the base text and as many
    /// documents as its layers.
    /// </summary>
    /// <seealso cref="ItemComposer" />
    public abstract class TeiStandoffItemComposer : ItemComposer
    {
        private readonly TextBlockBuilder _blockBuilder;
        private readonly JsonSerializerOptions _jsonOptions;
        private int _nextLayerId;

        /// <summary>
        /// The TEI namespace.
        /// </summary>
        public readonly XNamespace TEI_NS = "http://www.tei-c.org/ns/1.0";

        /// <summary>
        /// The text flow metadata key (<c>flow-key</c>).
        /// </summary>
        public const string M_FLOW_KEY = "flow-key";
        /// <summary>
        /// The layer identifier (<c>layer-id</c>). This is from the renderer
        /// context layer ID mappings.
        /// </summary>
        public const string M_LAYER_ID = "layer-id";

        /// <summary>
        /// Initializes a new instance of the <see cref="TeiStandoffItemComposer"/>
        /// class.
        /// </summary>
        protected TeiStandoffItemComposer()
        {
            _blockBuilder = new();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        private string BuildLayerId(IPart part)
        {
            string id = part.TypeId;
            if (!string.IsNullOrEmpty(part.RoleId)) id += "|" + part.RoleId;

            // save layer ID (1-N)
            if (!Context.LayerIds.ContainsKey(id))
                Context.LayerIds[id] = ++_nextLayerId;

            return id;
        }

        /// <summary>
        /// Renders the various flows of text.
        /// </summary>
        /// <param name="item">The source item.</param>
        /// <returns>Dictionary with key=flow ID and value=XML code.</returns>
        protected void RenderFlows(IItem item)
        {
            if (Output == null) return;

            // text: there must be one
            IPart? textPart = item.Parts.Find(
                p => p.RoleId == PartBase.BASE_TEXT_ROLE_ID);

            if (textPart == null || TextPartFlattener == null ||
                TextBlockRenderer == null)
            {
                return;
            }

            // layers
            IList<IPart> layerParts = item.Parts.Where(p =>
                    p.RoleId.StartsWith(PartBase.FR_PREFIX))
                // just to ensure mapping consistency between successive runs
                .OrderBy(p => p.RoleId)
                .ToList();

            // flatten and render into blocks
            var tr = TextPartFlattener.GetTextRanges(textPart,
                layerParts,
                (IList<string?>)layerParts.Select(p => BuildLayerId(p)).ToList());

            List<TextBlockRow> rows =
                _blockBuilder.Build(tr.Item1, tr.Item2).ToList();

            // render blocks
            string result = TextBlockRenderer.Render(rows, Context);
            WriteOutput(PartBase.BASE_TEXT_ROLE_ID, result);

            // render layers
            foreach (IPart layerPart in layerParts)
            {
                string id = BuildLayerId(layerPart);

                if (JsonRenderers.ContainsKey(id))
                {
                    Context.Data[M_LAYER_ID] = Context.LayerIds[id];
                    string json = JsonSerializer.Serialize<object>(layerPart,
                        _jsonOptions);
                    result = JsonRenderers[id].Render(json, Context);
                    WriteOutput(id, result);
                }
            }
        }

        /// <summary>
        /// Composes the output from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Composition result or null.</returns>
        /// <exception cref="ArgumentNullException">item</exception>
        protected override void DoCompose(IItem item)
        {
            if (Output != null) RenderFlows(item);
        }
     }

    /// <summary>
    /// Base options for TEI standoff item composers.
    /// </summary>
    public class TeiStandoffItemComposerOptions
    {
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
        /// Gets or sets the optional layer head. This is written at the start
        /// of each layer flow. Its value can include placeholders in curly
        /// braces, corresponding to any of the metadata keys defined in
        /// the item composer's context.
        /// </summary>
        public string? LayerHead { get; set; }

        /// <summary>
        /// Gets or sets the optional layer tail. This is written at the end
        /// of each layer flow. Its value can include placeholders in curly
        /// braces, corresponding to any of the metadata keys defined in
        /// the item composer's context.
        /// </summary>
        public string? LayerTail { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TeiStandoffItemComposerOptions"/> class.
        /// </summary>
        public TeiStandoffItemComposerOptions()
        {
            TextHead = "<body>";
            TextTail = "</body>";
            LayerHead = "<TEI xmlns=\"http://www.tei-c.org/ns/1.0\">" +
                Environment.NewLine +
                "<standOff type=\"{" +
                ItemComposer.M_ITEM_NR + "}\">";
            LayerTail = "</standOff>" + Environment.NewLine + "</TEI>";
        }
    }
}

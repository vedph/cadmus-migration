using Cadmus.Core;
using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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

        // metadata constants
        public const string M_ITEM_ID = "item-id";
        public const string M_ITEM_TITLE = "item-title";
        public const string M_ITEM_FACET = "item-facet";
        public const string M_ITEM_GROUP = "item-group";
        public const string M_ITEM_FLAGS = "item-flags";
        public const string M_ITEM_NR = "item-nr";
        public const string M_FLOW_KEY = "flow-key";

        /// <summary>
        /// Gets the ordinal item number. This is set to 0 when opening the
        /// composer, and increased whenever a new item is processed.
        /// </summary>
        protected int ItemNumber { get; private set; }

        /// <summary>
        /// Gets the context using during processing.
        /// </summary>
        protected TextBlockRendererContext Context { get; }

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
            Context = new TextBlockRendererContext();
        }

        /// <summary>
        /// Open the composer. This resets <see cref="ItemNumber"/> and the
        /// processing context.
        /// </summary>
        public override void Open()
        {
            ItemNumber = 0;
            Context.Data.Clear();
            Context.TargetIds.Clear();
        }

        private static string BuildLayerId(IPart part)
        {
            string id = part.TypeId;
            if (!string.IsNullOrEmpty(part.RoleId)) id += "|" + part.RoleId;
            return id;
        }

        /// <summary>
        /// Renders the various flows of text.
        /// </summary>
        /// <param name="item">The source item.</param>
        /// <returns>Dictionary with key=flow ID and value=XML code.</returns>
        protected Dictionary<string, string> RenderFlows(IItem item)
        {
            Dictionary<string, string> flows = new();

            Context.Data[M_ITEM_ID] = item.Id;
            Context.Data[M_ITEM_TITLE] = item.Title;
            Context.Data[M_ITEM_FACET] = item.FacetId;
            Context.Data[M_ITEM_GROUP] = item.GroupId;
            Context.Data[M_ITEM_FLAGS] = item.Flags;

            // text: there must be one
            IPart? textPart = item.Parts.Find(
                p => p.RoleId == PartBase.BASE_TEXT_ROLE_ID);

            if (textPart == null || TextPartFlattener == null ||
                TextBlockRenderer == null)
            {
                return flows;
            }

            // layers
            IList<IPart> layerParts = item.Parts.Where(p =>
                    p.RoleId.StartsWith(PartBase.FR_PREFIX)).ToList();

            // flatten and render into blocks
            var tr = TextPartFlattener.GetTextRanges(textPart,
                layerParts,
                (IList<string?>)layerParts.Select(p => BuildLayerId(p)).ToList());

            List<TextBlockRow> rows =
                _blockBuilder.Build(tr.Item1, tr.Item2).ToList();

            // render blocks
            flows[PartBase.BASE_TEXT_ROLE_ID] =
                TextBlockRenderer.Render(rows, Context);

            // render layers
            foreach (IPart layerPart in layerParts)
            {
                string id = BuildLayerId(layerPart);
                if (JsonRenderers.ContainsKey(id))
                {
                    string json = JsonSerializer.Serialize<object>(layerPart,
                        _jsonOptions);
                    flows[id] = JsonRenderers[id].Render(json, Context);
                }
            }

            return flows;
        }

        /// <summary>
        /// Fills the specified template using <see cref="Context"/>.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>The filled template.</returns>
        protected string FillTemplate(string? template)
            => template != null
                ? TextTemplate.FillTemplate(template, Context.Data)
                : "";

        /// <summary>
        /// Does the composition.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Optional output.</returns>
        protected abstract object? DoCompose(IItem item);

        /// <summary>
        /// Composes the output from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Composition result or null.</returns>
        /// <exception cref="ArgumentNullException">item</exception>
        public object? Compose(IItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            Context.Data[M_ITEM_NR] = ++ItemNumber;
            var result = DoCompose(item);
            Context.Data.Clear();
            return result;
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
        /// <see cref="TeiStandoffItemComposer"/>.
        /// </summary>
        public string? TextHead { get; set; }

        /// <summary>
        /// Gets or sets the optional text tail. This is written at the end
        /// of the text flow. Its value can include placeholders in curly
        /// braces, corresponding to any of the metadata keys defined in
        /// <see cref="TeiStandoffItemComposer"/>.
        /// </summary>
        public string? TextTail { get; set; }

        /// <summary>
        /// Gets or sets the optional layer head. This is written at the start
        /// of each layer flow. Its value can include placeholders in curly
        /// braces, corresponding to any of the metadata keys defined in
        /// <see cref="TeiStandoffItemComposer"/>.
        /// </summary>
        public string? LayerHead { get; set; }

        /// <summary>
        /// Gets or sets the optional layer tail. This is written at the end
        /// of each layer flow. Its value can include placeholders in curly
        /// braces, corresponding to any of the metadata keys defined in
        /// <see cref="TeiStandoffItemComposer"/>.
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
            LayerHead = "<standOff type=\"{" +
                TeiStandoffItemComposer.M_ITEM_NR + "}\">";
            LayerTail = "</standOff>";
        }
    }
}

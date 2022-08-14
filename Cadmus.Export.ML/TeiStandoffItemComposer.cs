using Cadmus.Core;
using Fusi.Tools.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Cadmus.Export.ML
{
    public abstract class TeiStandoffItemComposer : ItemComposer
    {
        private readonly TextBlockBuilder _blockBuilder;
        private readonly JsonSerializerOptions _jsonOptions;

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

        private static string BuildLayerId(IPart part)
        {
            string id = part.TypeId;
            if (!string.IsNullOrEmpty(part.RoleId)) id += "|" + part.RoleId;
            return id;
        }

        protected Dictionary<string, string> RenderFlows(IItem item)
        {
            Dictionary<string, string> flows = new();

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
            flows[PartBase.BASE_TEXT_ROLE_ID] = TextBlockRenderer.Render(rows);

            // render layers
            foreach (IPart layerPart in layerParts)
            {
                string id = BuildLayerId(layerPart);
                if (JsonRenderers.ContainsKey(id))
                {
                    string json = JsonSerializer.Serialize(layerPart, _jsonOptions);
                    flows[id] = JsonRenderers[id].Render(json);
                }
            }

            return flows;
        }
    }
}

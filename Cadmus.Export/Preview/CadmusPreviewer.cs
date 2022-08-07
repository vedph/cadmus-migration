using Cadmus.Core;
using Cadmus.Core.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Cadmus.Export.Preview
{
    /// <summary>
    /// Cadmus object previewer. This is a high level class using rendition
    /// components to render a preview for Cadmus parts or fragments.
    /// </summary>
    public class CadmusPreviewer
    {
        private sealed class MappingEntry
        {
            public string TypeId { get; set; }
            public string? RoleId { get; set; }
            public string TargetId { get; set; }

            public MappingEntry()
            {
                TypeId = "";
                TargetId = "";
            }

            public override string ToString()
            {
                return $"{TypeId} {RoleId}: {TargetId}";
            }
        }

        private readonly ICadmusRepository _repository;
        private readonly CadmusPreviewFactory _factory;
        private readonly Dictionary<string, string> _mappings;
        private TextBlockBuilder? _blockBuilder;
        // cache
        private readonly Dictionary<string, IJsonRenderer> _jsonRenderers;
        private readonly Dictionary<string, ITextPartFlattener> _flatteners;

        /// <summary>
        /// Initializes a new instance of the <see cref="CadmusPreviewer"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="factory">The factory.</param>
        /// <exception cref="ArgumentNullException">repository or factory</exception>
        public CadmusPreviewer(ICadmusRepository repository,
            CadmusPreviewFactory factory)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            _mappings = new();

            // cached components
            _jsonRenderers = new Dictionary<string, IJsonRenderer>();
            _flatteners = new Dictionary<string, ITextPartFlattener>();
        }

        public void SetMappings(IDictionary<string, string> mappings)
        {
            if (mappings is null)
                throw new ArgumentNullException(nameof(mappings));

            _mappings.Clear();
            foreach (string key in mappings.Keys) _mappings[key] = mappings[key];
        }

        public void LoadMappings(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            MappingEntry[]? entries = JsonSerializer.Deserialize<MappingEntry[]>(stream);
            _mappings.Clear();
            if (entries == null) return;

            foreach (var entry in entries)
            {
                string key = entry.RoleId != null
                    ? $"{entry.TypeId} {entry.RoleId}"
                    : entry.TypeId;
                _mappings[key] = entry.TargetId;
            }
        }

        private IJsonRenderer? GetRendererFromTarget(string key)
        {
            // get the renderer targeting the part type ID
            if (!_mappings.ContainsKey(key)) return null;

            string id = _mappings[key];
            IJsonRenderer? renderer;

            if (_jsonRenderers.ContainsKey(id))
                renderer = _jsonRenderers[id];
            else
            {
                renderer = _factory.GetJsonRenderer(id);
                if (renderer == null) return null;
                _jsonRenderers[id] = renderer;
            }
            return renderer;
        }

        /// <summary>
        /// Renders the part with the specified ID, using the renderer targeting
        /// its part type ID.
        /// </summary>
        /// <param name="id">The part's identifier.</param>
        /// <returns>Rendition.</returns>
        /// <exception cref="ArgumentNullException">id</exception>
        public string RenderPart(string id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            string? json = _repository.GetPartContent(id);
            if (json == null) return "";

            // get part type ID
            JsonDocument doc = JsonDocument.Parse(json);
            string? typeId = doc.RootElement.GetProperty("typeId").GetString();
            if (typeId == null) return "";

            // get the renderer targeting the part type ID
            IJsonRenderer? renderer = GetRendererFromTarget(typeId);

            // render
            return renderer != null? renderer.Render(json) : "";
        }

        /// <summary>
        /// Renders the fragment at index <paramref name="frIndex"/> in the part
        /// with ID <paramref name="id"/>, using the renderer targeting
        /// its part role ID.
        /// </summary>
        /// <param name="id">The part's identifier.</param>
        /// <returns>Rendition.</returns>
        /// <exception cref="ArgumentNullException">id</exception>
        /// <exception cref="ArgumentOutOfRangeException">frIndex less than 0
        /// </exception>
        public string RenderFragment(string id, int frIndex)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            if (frIndex < 0) throw new ArgumentOutOfRangeException(nameof(frIndex));

            string? json = _repository.GetPartContent(id);
            if (json == null) return "";

            // get the part type ID and role ID (=fragment type)
            JsonDocument doc = JsonDocument.Parse(json);
            string? typeId = doc.RootElement.GetProperty("typeId").GetString();
            if (typeId == null) return "";
            string? roleId = doc.RootElement.GetProperty("roleId").GetString();
            if (roleId == null) return "";

            // the target ID is the combination of these two IDs
            string targetId = $"{typeId} {roleId}";

            IJsonRenderer? renderer = GetRendererFromTarget(targetId);

            // extract the targeted fragment
            JsonElement frr = doc.RootElement
                .GetProperty("content")
                .GetProperty("fragments");
            if (frIndex >= frr.GetArrayLength()) return "";

            JsonElement? fr = null;
            int i = 0;
            foreach (JsonElement entry in frr.EnumerateArray())
            {
                if (i == frIndex)
                {
                    fr = entry;
                    break;
                }
                i++;
            }
            if (fr == null) return "";

            // render
            string frJson = fr.ToString()!;
            return renderer != null ? renderer.Render(frJson) : "";
        }

        /// <summary>
        /// Builds the text blocks from the specified text part.
        /// </summary>
        /// <param name="id">The part identifier.</param>
        /// <param name="layerIds">The IDs of the layers to include in the
        /// rendition.</param>
        /// <returns>Rendition.</returns>
        /// <exception cref="ArgumentNullException">id or layerIds</exception>
        public IList<TextBlockRow> BuildTextBlocks(string id,
            IList<string> layerIds)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            if (layerIds is null) throw new ArgumentNullException(nameof(layerIds));

            string? json = _repository.GetPartContent(id);
            if (json == null) return Array.Empty<TextBlockRow>();

            // get the part type ID (role ID is always base-text)
            JsonDocument doc = JsonDocument.Parse(json);
            string? typeId = doc.RootElement.GetProperty("typeId").GetString();
            if (typeId == null) return Array.Empty<TextBlockRow>();

            // get the flattener for that type ID
            if (!_mappings.ContainsKey(typeId)) return Array.Empty<TextBlockRow>();
            string flattenerId = _mappings[typeId];

            ITextPartFlattener? flattener;
            if (_flatteners.ContainsKey(flattenerId))
                flattener = _flatteners[flattenerId];
            else
            {
                flattener = _factory.GetTextPartFlattener(flattenerId);
                if (flattener == null) return Array.Empty<TextBlockRow>();
                _flatteners[flattenerId] = flattener;
            }

            // load part and layers
            IPart? part = _repository.GetPart<IPart>(id);
            if (part == null) return Array.Empty<TextBlockRow>();
            List<IPart> layerParts = layerIds
                .Select(lid => _repository.GetPart<IPart>(lid))
                .Where(p => p != null)
                .ToList();

            // flatten them
            var tr = flattener.GetTextRanges(part, layerParts);

            // build blocks rows
            if (_blockBuilder == null) _blockBuilder = new();
            return _blockBuilder.Build(tr.Item1, tr.Item2).ToList();
        }
    }
}

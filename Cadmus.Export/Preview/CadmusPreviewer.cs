using Cadmus.Core;
using Cadmus.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Cadmus.Export.Preview
{
    /// <summary>
    /// Cadmus object previewer. This is a high level class using rendition
    /// components to render a preview for Cadmus parts or fragments.
    /// </summary>
    public sealed class CadmusPreviewer
    {
        private readonly ICadmusRepository _repository;
        private readonly CadmusPreviewFactory _factory;
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
            _repository = repository ??
                throw new ArgumentNullException(nameof(repository));
            _factory = factory ??
                throw new ArgumentNullException(nameof(factory));

            // cached components
            _jsonRenderers = new Dictionary<string, IJsonRenderer>();
            _flatteners = new Dictionary<string, ITextPartFlattener>();
        }

        /// <summary>
        /// Gets all the keys registered for JSON renderers in the
        /// configuration of this factory. This is used by client code
        /// to determine for which Cadmus objects a preview is available.
        /// </summary>
        /// <returns>List of unique keys.</returns>
        public HashSet<string> GetJsonRendererKeys()
            => _factory.GetJsonRendererKeys();

        /// <summary>
        /// Gets all the keys registered for JSON text part flatteners
        /// in the configuration of this factory. This is used by client code
        /// to determine for which Cadmus objects a preview is available.
        /// </summary>
        /// <returns>List of unique keys.</returns>
        public HashSet<string> GetFlattenerKeys()
            => _factory.GetFlattenerKeys();

        /// <summary>
        /// Gets all the keys registered for item composers in the configuration
        /// of this factory.
        /// </summary>
        /// <returns>List of unique keys.</returns>
        public HashSet<string> GetComposerKeys()
            => _factory.GetComposerKeys();

        private IJsonRenderer? GetRendererFromKey(string key)
        {
            IJsonRenderer? renderer;

            if (_jsonRenderers.ContainsKey(key))
            {
                renderer = _jsonRenderers[key];
            }
            else
            {
                renderer = _factory.GetJsonRenderer(key);
                if (renderer == null) return null;
                _jsonRenderers[key] = renderer;
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
            IJsonRenderer? renderer = GetRendererFromKey(typeId);

            // render
            return renderer != null? renderer.Render(json) : "";
        }

        private static JsonElement? GetFragmentAt(JsonElement fragments, int index)
        {
            if (index >= fragments.GetArrayLength()) return null;

            int i = 0;
            foreach (JsonElement fr in fragments.EnumerateArray())
            {
                if (i == index) return fr;
                i++;
            }
            return null;
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
            string key = $"{typeId}|{roleId}";

            IJsonRenderer? renderer = GetRendererFromKey(key);

            // extract the targeted fragment
            JsonElement fragments = doc.RootElement
                .GetProperty("fragments");
            JsonElement? fr = GetFragmentAt(fragments, frIndex);
            if (fr == null) return "";

            // render
            string frJson = fr.ToString()!;
            return renderer != null ? renderer.Render(frJson) : "";
        }

        /// <summary>
        /// Builds the text blocks from the specified text part.
        /// </summary>
        /// <param name="id">The part identifier.</param>
        /// <param name="layerPartIds">The IDs of the layers to include in the
        /// rendition.</param>
        /// <param name="layerIds">The optional IDs to assign to each layer
        /// part's range. When specified, it must have the same size of
        /// <paramref name="layerPartIds"/> so that the first entry in it
        /// corresponds to the first entry in layer IDs, the second to the second,
        /// and so forth.</param>
        /// <returns>Rendition.</returns>
        /// <exception cref="ArgumentNullException">id or layerIds</exception>
        public IList<TextBlockRow> BuildTextBlocks(string id,
            IList<string> layerPartIds, IList<string?>? layerIds = null)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            if (layerPartIds is null) throw new ArgumentNullException(nameof(layerPartIds));

            string? json = _repository.GetPartContent(id);
            if (json == null) return Array.Empty<TextBlockRow>();

            // get the part type ID (role ID is always base-text)
            JsonDocument doc = JsonDocument.Parse(json);
            string? typeId = doc.RootElement.GetProperty("typeId").GetString();
            if (typeId == null) return Array.Empty<TextBlockRow>();

            // get the flattener for that type ID
            ITextPartFlattener? flattener;
            if (_flatteners.ContainsKey(typeId))
            {
                flattener = _flatteners[typeId];
            }
            else
            {
                flattener = _factory.GetTextPartFlattener(typeId);
                if (flattener == null) return Array.Empty<TextBlockRow>();
                _flatteners[typeId] = flattener;
            }

            // load part and layers
            IPart? part = _repository.GetPart<IPart>(id);
            if (part == null) return Array.Empty<TextBlockRow>();
            List<IPart> layerParts = layerPartIds
                .Select(lid => _repository.GetPart<IPart>(lid))
                .Where(p => p != null)
                .ToList();

            // flatten them
            var tr = flattener.GetTextRanges(part, layerParts, layerIds);

            // build blocks rows
            if (_blockBuilder == null) _blockBuilder = new();
            return _blockBuilder.Build(tr.Item1, tr.Item2).ToList();
        }
    }
}

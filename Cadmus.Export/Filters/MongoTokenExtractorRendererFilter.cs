using Cadmus.Core.Config;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Cadmus.Mongo;
using Fusi.Tools.Config;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using Cadmus.Core.Layers;
using Fusi.Tools.Text;

namespace Cadmus.Export.Filters
{
    /// <summary>
    /// Mongo-based text extractor for token-based text parts.
    /// This replaces all the text locations matched via a specified regular
    /// expression pattern with the corresponding text from the base text part.
    /// <para>Tag: <c>it.vedph.renderer-filter.mongo-token-extractor</c>.</para>
    /// </summary>
    /// <seealso cref="IRendererFilter" />
    [Tag("it.vedph.renderer-filter.mongo-token-extractor")]
    public sealed class MongoTokenExtractorRendererFilter : IRendererFilter,
        IConfigurable<MongoTokenExtractorRendererFilterOptions>
    {
        private MongoTokenExtractorRendererFilterOptions? _options;
        private Regex? _locRegex;
        private ICadmusRepository? _repository;

        private string? _itemId;
        private TokenTextPart? _part;

        private ICadmusRepository GetRepository()
        {
            TagAttributeToTypeMap map = new();
            map.Add(new[]
            {
                typeof(NotePart).Assembly
            });
            MongoCadmusRepository repository = new(
                new StandardPartTypeProvider(map),
                new StandardItemSortKeyBuilder());
            repository.Configure(new MongoCadmusRepositoryOptions
            {
                // use the default ConnectionStringTemplate (local DB)
                ConnectionString = _options!.ConnectionString
            });
            return repository;
        }

        /// <summary>
        /// Configures this filter with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(MongoTokenExtractorRendererFilterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _locRegex = new Regex(_options.LocationPattern, RegexOptions.Compiled);
        }

        private TokenTextPart? GetTokenTextPart(string itemId)
        {
            IItem? item = _repository!.GetItem(itemId);
            if (item == null) return null;

            IPart? part = _repository.GetItemParts(new[] { itemId },
                typeof(TokenTextPart).GetCustomAttribute<TagAttribute>()?.Tag,
                PartBase.BASE_TEXT_ROLE_ID).FirstOrDefault();

            return part == null ? null : (TokenTextPart)part;
        }

        /// <summary>
        /// Applies this filter to the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="context">The optional renderer context.</param>
        /// <returns>The filtered text.</returns>
        public string Apply(string text, IRendererContext? context = null)
        {
            if (_locRegex == null) return text;

            // get item ID
            string? itemId = context?.Data?.ContainsKey(
                ItemComposer.M_ITEM_ID) == true
                ? context!.Data[ItemComposer.M_ITEM_ID] as string
                : null;
            if (itemId == null) return text;

            if (_repository == null) _repository = GetRepository();

            // get base text part
            TokenTextPart? part;
            if (_itemId == itemId) part = _part!;
            else
            {
                part = GetTokenTextPart(itemId);
                if (part == null) return text;
                // cache the item's base text part
                _itemId = itemId;
                _part = part;
            }

            return _locRegex.Replace(text, (m) =>
            {
                string loc = m.Groups[1].Value;
                if (string.IsNullOrEmpty(loc)) return m.Groups[1].Value;

                // extract
                TokenTextLocation tl = TokenTextLocation.Parse(loc);
                string text = part.GetText(tl,
                    _options?.WholeToken == true,
                    _options?.StartMarker,
                    _options?.EndMarker);

                // cut if required
                if (_options?.TextCutting == true)
                    text = TextCutter.Cut(text, _options)!;

                return text;
            });
        }
    }

    /// <summary>
    /// Options for <see cref="MongoTokenExtractorRendererFilter"/>.
    /// </summary>
    public class MongoTokenExtractorRendererFilterOptions : TextCutterOptions
    {
        /// <summary>
        /// Gets or sets the connection string to the Mongo database
        /// containing the thesauri.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the regular expression pattern representing a text
        /// location expression. It is assumed that the first capture group
        /// in it is the text location.
        /// </summary>
        public string LocationPattern { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to extract the whole token
        /// from the base text, even when the oordinates refer to a portion of
        /// it.
        /// </summary>
        public bool WholeToken { get; set; }

        /// <summary>
        /// Gets or sets the start marker to insert at the beginning of the
        /// token portion when extracting the whole token. Default is <c>[</c>.
        /// </summary>
        public string? StartMarker { get; set; }

        /// <summary>
        /// Gets or sets the end marker to insert at the beginning of the
        /// token portion when extracting the whole token. Default is <c>]</c>.
        /// </summary>
        public string? EndMarker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether text cutting is enabled.
        /// </summary>
        public bool TextCutting { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MongoTokenExtractorRendererFilterOptions"/> class.
        /// </summary>
        public MongoTokenExtractorRendererFilterOptions()
        {
            LocationPattern = @"\@{([^}]+)}";
            StartMarker = "[";
            EndMarker = "]";
        }
    }
}

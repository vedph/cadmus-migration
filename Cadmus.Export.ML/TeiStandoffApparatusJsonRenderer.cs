using Cadmus.Philology.Parts;
using Fusi.Tools.Config;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Linq;

namespace Cadmus.Export.ML
{
    /// <summary>
    /// JSON renderer for standoff TEI apparatus layer part.
    /// This provides apparatus fragments entries rendition. Typically, the
    /// TEI apparatus is encoded inside a <c>standOff</c> element in <c>body</c>.
    /// The <c>@type</c> attribute of this element contains the apparatus layer
    /// part role ID (=fragment's type ID).
    /// <para>In <c>standOff</c> there is a <c>div</c> for each item, with
    /// <c>@xml:id</c> equal to the item's ID.</para>
    /// <para>In this <c>div</c>, each fragment is another <c>div</c> with
    /// an optional <c>@type</c> attribute equal to the fragment's tag, when
    /// present.</para>
    /// <para>Each entry in the fragment is an <c>app</c> entry with these
    /// properties (I add to each its corresponding XML rendition):</para>
    /// <list type="bullet">
    /// <item>
    /// <term>type</term>
    /// <description>a <c>rdg</c> child element or a <c>note</c> child element
    /// according to the type.</description>
    /// </item>
    /// <item>
    /// <term>value</term>
    /// <description>the value of <c>rdg</c>/<c>note</c>.</description>
    /// </item>
    /// <item>
    /// <term>tag</term>
    /// <description><c>@type</c> of <c>rdg</c>/<c>note</c>.</description>
    /// </item>
    /// <item>
    /// <term>note</term>
    /// <description>append to <c>note</c> if existing, else add child <c>note</c>
    /// with this content.</description>
    /// </item>
    /// <item>
    /// <term>witnesses</term>
    /// <description><c>@wit</c> of <c>rdg</c>/<c>note</c>.</description>
    /// </item>
    /// <item>
    /// <term>authors</term>
    /// <description><c>@source</c> of <c>rdg</c>/<c>note</c>.</description>
    /// </item>
    /// </list>
    /// <item>
    /// <term>not rendered</term>
    /// <description>normValue, isAccepted, groupId.</description>
    /// </item>
    /// <para>Tag: <c>it.vedph.json-renderer.tei-standoff.apparatus</c>.</para>
    /// </summary>
    /// <seealso cref="JsonRenderer" />
    /// <seealso cref="IJsonRenderer" />
    [Tag("it.vedph.json-renderer.tei-standoff.apparatus")]
    public sealed class TeiStandoffApparatusJsonRenderer : JsonRenderer,
        IJsonRenderer, IConfigurable<TeiStandoffApparatusJsonRendererOptions>
    {
        private readonly XNamespace XML = "http://www.w3.org/XML/1998/namespace";
        private readonly XNamespace TEI = "http://www.tei-c.org/ns/1.0";
        private readonly JsonSerializerOptions _jsonOptions;

        private TeiStandoffApparatusJsonRendererOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeiStandoffApparatusJsonRenderer"/>
        /// class.
        /// </summary>
        public TeiStandoffApparatusJsonRenderer()
        {
            _jsonOptions = new()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };
            _options = new();
        }

        private string BuildValue(ApparatusEntry entry)
        {
            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(entry.Value)) sb.Append(entry.Value);
            else if (entry.Type == ApparatusEntryType.Replacement &&
                !string.IsNullOrEmpty(_options?.ZeroVariant))
            {
                sb.Append(_options.ZeroVariant);
            }

            if (!string.IsNullOrEmpty(entry.Note))
            {
                if (!string.IsNullOrEmpty(entry.Value) &&
                    !string.IsNullOrEmpty(_options?.NotePrefix))
                {
                    sb.Append(_options.NotePrefix);
                }
                sb.Append(entry.Note);
            }
            return sb.ToString();
        }

        private static string RenderAnnotatedValue(AnnotatedValue av)
        {
            StringBuilder sb = new(av.Value);
            if (!string.IsNullOrEmpty(av.Note))
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(av.Note);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(TeiStandoffApparatusJsonRendererOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Renders the specified JSON code.
        /// </summary>
        /// <param name="json">The input JSON.</param>
        /// <param name="context">The optional renderer context.</param>
        /// <returns>Rendered output.</returns>
        protected override string DoRender(string json,
            IRendererContext? context = null)
        {
            // read fragments array
            JsonNode? root = JsonNode.Parse(json);
            if (root == null) return "";
            ApparatusLayerFragment[]? fragments =
                root["fragments"].Deserialize<ApparatusLayerFragment[]>(_jsonOptions);
            if (fragments == null || context == null) return "";

            // div @xml:id="ITEM_ID"
            XElement itemDiv = new(TEI + "div",
                new XAttribute(XML + "id", context.Data[ItemComposer.M_ITEM_ID]));

            // process each fragment
            foreach (ApparatusLayerFragment fr in fragments)
            {
                // div @type="TAG"
                XElement frDiv = new(TEI + "div");
                if (!string.IsNullOrEmpty(fr.Tag))
                    frDiv.SetAttributeValue("type", fr.Tag);
                itemDiv.Add(frDiv);

                int n = 0;
                foreach (ApparatusEntry entry in fr.Entries)
                {
                    // div/app @n="INDEX + 1" [@type="TAG"]
                    XElement app = new(TEI + "app",
                        new XAttribute("n", ++n));
                    frDiv.Add(app);
                    if (!string.IsNullOrEmpty(entry.Tag))
                        app.SetAttributeValue("type", entry.Tag);

                    // div/rdg or div/note with value[+note]
                    XElement rdgOrNote = new(TEI +
                        (entry.Type == ApparatusEntryType.Note? "note" : "rdg"),
                        BuildValue(entry));
                    app.Add(rdgOrNote);

                    // @wit
                    if (entry.Witnesses?.Count > 0)
                    {
                        rdgOrNote.SetAttributeValue("wit",
                            string.Join(" ", from av in entry.Witnesses
                                             select RenderAnnotatedValue(av)));
                    }

                    // @source
                    if (entry.Authors?.Count > 0)
                    {
                        rdgOrNote.SetAttributeValue("source",
                            string.Join(" ", from av in entry.Witnesses
                                             select RenderAnnotatedValue(av)));
                    }
                }
            }

            return itemDiv.ToString(SaveOptions.OmitDuplicateNamespaces);
        }
    }

    /// <summary>
    /// Options for <see cref="TeiStandoffApparatusJsonRenderer"/>
    /// </summary>
    public class TeiStandoffApparatusJsonRendererOptions
    {
        /// <summary>
        /// Gets or sets the text to output for a zero variant. A zero
        /// variant is a deletion, represented as a text variant with an
        /// empty value. When building an output, you might want to add
        /// some conventional text for it, e.g. <c>del.</c> (delevit),
        /// which is the default value.
        /// </summary>
        public string? ZeroVariant { get; set; }

        /// <summary>
        /// Gets or sets the note prefix. This is an optional string to be
        /// prefixed to the note text after a non-empty value in a <c>rdg</c>
        /// or <c>note</c> element value. Any apparatus entry can have a
        /// value, and an optional note; when it's a variant mostly it has
        /// a value, and eventually a note; when it's a note, mostly it has
        /// only the note without value. The default value is space.
        /// </summary>
        public string? NotePrefix { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TeiStandoffApparatusJsonRendererOptions"/> class.
        /// </summary>
        public TeiStandoffApparatusJsonRendererOptions()
        {
            ZeroVariant = "del.";
            NotePrefix = " ";
        }
    }
}

using Cadmus.Philology.Parts;
using Fusi.Tools.Config;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

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
    /// <para>Each entry in the fragment is an <c>app</c> entry with:</para>
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
        IJsonRenderer
    {
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
                root["fragments"].Deserialize<ApparatusLayerFragment[]>();
            if (fragments == null) return "";

            // process each fragment
            foreach (ApparatusLayerFragment fr in fragments)
            {
                // TODO
            }

            throw new NotImplementedException();
        }
    }
}

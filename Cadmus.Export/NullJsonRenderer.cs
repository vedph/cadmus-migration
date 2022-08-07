using Fusi.Tools.Config;

namespace Cadmus.Export
{
    /// <summary>
    /// Null JSON renderer. This just returns the received JSON, and can be
    /// used for diagnostic purposes.
    /// <para>Tag: <c>it.vedph.json-renderer.null</c>.</para>
    /// </summary>
    /// <seealso cref="IJsonRenderer" />
    [Tag("it.vedph.json-renderer.null")]
    public sealed class NullJsonRenderer : IJsonRenderer
    {
        public string Render(string json)
        {
            return json ?? "(NULL)";
        }
    }
}

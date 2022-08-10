using System.Collections.Generic;
using System.Text;

namespace Cadmus.Export
{
    /// <summary>
    /// Base class for <see cref="IJsonRenderer"/>'s.
    /// </summary>
    public abstract class JsonRenderer
    {
        /// <summary>
        /// Gets the optional filters to apply after the renderer completes.
        /// </summary>
        public IList<IRendererFilter> Filters { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRenderer"/> class.
        /// </summary>
        protected JsonRenderer()
        {
            Filters = new List<IRendererFilter>();
        }

        /// <summary>
        /// Renders the specified JSON code.
        /// </summary>
        /// <param name="json">The input JSON.</param>
        /// <returns>Rendered output.</returns>
        protected abstract string DoRender(string json);

        /// <summary>
        /// Renders the specified JSON code.
        /// </summary>
        /// <param name="json">The input JSON.</param>
        /// <returns>Rendered output.</returns>
        public string Render(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            string result = DoRender(json);

            if (Filters.Count > 0)
            {
                foreach (IRendererFilter filter in Filters)
                    result = filter.Apply(result);
            }

            return result;
        }
    }
}

using DevLab.JmesPath;
using Fusi.Tools.Config;
using Fusi.Xml.Extras.Render;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Cadmus.Export
{
    /// <summary>
    /// XSLT based JSON renderer. This is one of the most customizable renderers,
    /// using an optional pipeline of JMESPath transforms to preprocess the
    /// JSON input, and an XSLT script to render it once converted to XML.
    /// <para>Tag: <c>it.vedph.json-renderer.xslt</c>.</para>
    /// </summary>
    [Tag("it.vedph.json-renderer.xslt")]
    public sealed class XsltJsonRenderer : IJsonRenderer,
        IConfigurable<XsltPartRendererOptions>
    {
        // https://jmespath.org/tutorial.html
        // https://github.com/jdevillard/JmesPath.Net
        private readonly List<string> _jsonExpressions;

        // https://www.newtonsoft.com/json/help/html/ConvertingJSONandXML.htm
        private XsltTransformer? _transformer;

        /// <summary>
        /// Initializes a new instance of the <see cref="XsltJsonRenderer"/> class.
        /// </summary>
        public XsltJsonRenderer()
        {
            _jsonExpressions = new();
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(XsltPartRendererOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            _jsonExpressions.Clear();
            _jsonExpressions.AddRange(options.JsonExpressions);

            if (!string.IsNullOrWhiteSpace(options.Xslt))
                _transformer = new XsltTransformer(options.Xslt);
            else
                _transformer = null;
        }

        /// <summary>
        /// Renders the specified JSON code.
        /// </summary>
        /// <param name="json">The input JSON.</param>
        /// <returns>Rendered output.</returns>
        /// <exception cref="ArgumentNullException">json</exception>
        public string Render(string json)
        {
            if (json is null)
                throw new ArgumentNullException(nameof(json));
            if (_transformer == null) return "";

            // wrap object properties in root
            json = "{\"root\":" + json + "}";

            // transform JSON if required
            JmesPath jmes = new();
            foreach (string e in _jsonExpressions)
            {
                json = jmes.Transform(json, e);
            }

            // if no XSLT, we're done
            if (_transformer == null) return json;

            // convert to XML
            XmlDocument? doc = JsonConvert.DeserializeXmlNode(json);
            if (doc is null) return "";

            // transform via XSLT
            return _transformer.Transform(doc.OuterXml);
        }
    }

    /// <summary>
    /// Options for <see cref="XsltJsonRenderer"/>.
    /// </summary>
    public class XsltPartRendererOptions
    {
        /// <summary>
        /// Gets or sets the JSON transform expressions using JMES Path.
        /// </summary>
        public IList<string> JsonExpressions { get; set; }

        /// <summary>
        /// Gets or sets the XSLT script used to produce the final result.
        /// </summary>
        public string? Xslt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XsltPartRendererOptions"/>
        /// class.
        /// </summary>
        public XsltPartRendererOptions()
        {
            JsonExpressions = new List<string>();
        }
    }
}

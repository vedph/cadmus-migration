using Fusi.Tools.Config;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cadmus.Export.Preview
{
    /// <summary>
    /// Components factory for <see cref="CadmusPreviewer"/>.
    /// </summary>
    /// <seealso cref="ComponentFactoryBase" />
    public class CadmusPreviewFactory : ComponentFactoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PythiaFactory" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="configuration">The configuration.</param>
        public CadmusPreviewFactory(Container container, IConfiguration configuration)
            : base(container, configuration)
        {
        }

        /// <summary>
        /// Configures the container services to use components from
        /// <c>Pythia.Core</c>.
        /// This is just a helper method: at any rate, the configuration of
        /// the container is external to the VSM factory. You could use this
        /// method as a model and create your own, or call this method to
        /// register the components from these two assemblies, and then
        /// further configure the container, or add more assemblies when
        /// calling this via <paramref name="additionalAssemblies"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="additionalAssemblies">The optional additional
        /// assemblies.</param>
        /// <exception cref="ArgumentNullException">container</exception>
        public static void ConfigureServices(Container container,
            params Assembly[] additionalAssemblies)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            // https://simpleinjector.readthedocs.io/en/latest/advanced.html?highlight=batch#batch-registration
            Assembly[] assemblies = new[]
            {
                // Cadmus.Export
                typeof(XsltJsonRenderer).Assembly
            };
            if (additionalAssemblies?.Length > 0)
                assemblies = assemblies.Concat(additionalAssemblies).ToArray();

            container.Collection.Register<IJsonRenderer>(assemblies);
            container.Collection.Register<ITextBlockRenderer>(assemblies);
            container.Collection.Register<ITextPartFlattener>(assemblies);

            // container.RegisterInstance(new UniData())
        }

        private void CollectKeys(string collectionPath, HashSet<string> keys)
        {
            foreach (var entry in
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, collectionPath)
                .Where(e => e.Keys?.Count > 0))
            {
                foreach (string id in entry.Keys!) keys.Add(id);
            }
        }

        /// <summary>
        /// Gets all the keys registered for JSON renderers or text part
        /// flatteners in the configuration of this factory. This is used
        /// by client code to determine for which Cadmus objects a preview
        /// is available.
        /// </summary>
        /// <param name="flatteners">True to get the flatteners keys, false
        /// to get the renderers keys.</param>
        /// <returns>List of unique keys.</returns>
        public HashSet<string> GetKeys(bool flatteners)
        {
            HashSet<string> keys = new();

            if (flatteners) CollectKeys("TextPartFlatteners", keys);
            else CollectKeys("JsonRenderers", keys);

            return keys;
        }

        /// <summary>
        /// Gets the JSON renderer with the specified key.
        /// </summary>
        /// <param name="key">The key of the requested renderer.</param>
        /// <returns>Renderer or null if not found.</returns>
        public IJsonRenderer? GetJsonRenderer(string key)
        {
            return GetComponentByKey<IJsonRenderer>("JsonRenderers", key);
        }

        /// <summary>
        /// Gets the text block renderer with the specified key.
        /// </summary>
        /// <param name="key">The key of the requested renderer.</param>
        /// <returns>Renderer or null if not found.</returns>
        public ITextBlockRenderer? GetTextBlockRenderer(string key)
        {
            return GetComponentByKey<ITextBlockRenderer>("TextBlockRenderers", key);
        }

        /// <summary>
        /// Gets the text part flattener with the specified key.
        /// </summary>
        /// <param name="key">The key of the requested flattener.</param>
        /// <returns>Flattener or null if not found.</returns>
        public ITextPartFlattener? GetTextPartFlattener(string key)
        {
            return GetComponentByKey<ITextPartFlattener>("TextPartFlatteners", key);
        }
    }
}

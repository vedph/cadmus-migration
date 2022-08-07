using Cadmus.General.Parts;
using Fusi.Text.Unicode;
using Fusi.Tools.Config;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cadmus.Export.Preview
{
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
                // Cadmus.General.Parts
                // typeof(NotePart).Assembly,
                typeof(XsltJsonRenderer).Assembly
            };
            if (additionalAssemblies?.Length > 0)
                assemblies = assemblies.Concat(additionalAssemblies).ToArray();

            container.Collection.Register<IJsonRenderer>(assemblies);
            container.Collection.Register<ITextBlockRenderer>(assemblies);
            container.Collection.Register<ITextPartFlattener>(assemblies);

            // container.RegisterInstance(new UniData())
        }

        private T? GetComponentById<T>(string collectionPath, string id)
            where T : class
        {
            IList<ComponentFactoryConfigEntry> entries =
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, collectionPath);

            ComponentFactoryConfigEntry? entry =
                entries.FirstOrDefault(e => e.Id == id);
            if (entry == null) return null;

            return GetComponent<T>(entry.Id!, entry.OptionsPath!);
        }

        /// <summary>
        /// Gets the JSON renderer with the specified ID.
        /// </summary>
        /// <param name="id">The identifier of the requested renderer.</param>
        /// <returns>Renderer or null if not found.</returns>
        public IJsonRenderer? GetJsonRenderer(string id)
        {
            return GetComponentById<IJsonRenderer>("JsonRenderers", id);
        }

        /// <summary>
        /// Gets the text block renderer with the specified ID.
        /// </summary>
        /// <param name="id">The identifier of the requested renderer.</param>
        /// <returns>Renderer or null if not found.</returns>
        public ITextBlockRenderer? GetTextBlockRenderer(string id)
        {
            return GetComponentById<ITextBlockRenderer>("TextBlockRenderers", id);
        }

        /// <summary>
        /// Gets the text part flattener with the specified ID.
        /// </summary>
        /// <param name="id">The identifier of the requested flattener.</param>
        /// <returns>Flattener or null if not found.</returns>
        public ITextPartFlattener? GetTextPartFlattener(string id)
        {
            return GetComponentById<ITextPartFlattener>("TextPartFlatteners", id);
        }
    }
}

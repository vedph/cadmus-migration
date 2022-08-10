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
        /// The name of the connection string property to be supplied
        /// in POCO option objects (<c>ConnectionString</c>).
        /// </summary>
        public const string CONNECTION_STRING_NAME = "ConnectionString";

        /// <summary>
        /// The optional general connection string to supply to any component
        /// requiring an option named <see cref="CONNECTION_STRING_NAME"/>
        /// (=<c>ConnectionString</c>), when this option is not specified
        /// in its configuration.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythiaFactory" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="configuration">The configuration.</param>
        public CadmusPreviewFactory(Container container, IConfiguration configuration)
            : base(container, configuration)
        {
        }

        private static object SupplyProperty(Type optionType,
            PropertyInfo property, object options, object defaultValue)
        {
            // if options have been loaded, supply if not specified
            if (options != null)
            {
                string? value = (string?)property.GetValue(options);
                if (string.IsNullOrEmpty(value))
                    property.SetValue(options, defaultValue);
            }
            // else create empty options and supply it
            else
            {
                options = Activator.CreateInstance(optionType)!;
                property.SetValue(options, defaultValue);
            }

            return options;
        }

        /// <summary>
        /// Does the custom configuration.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="component">The component.</param>
        /// <param name="section">The section.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="optionType">Type of the option.</param>
        /// <returns>True if custom configuration logic applied.</returns>
        protected override bool DoCustomConfiguration<T>(T component,
            IConfigurationSection section, TypeInfo targetType, Type optionType)
        {
            // get the options if specified
            object options = section?.Get(optionType)!;

            // if we have a default connection AND the options type
            // has a ConnectionString property, see if we should supply a value
            // for it
            PropertyInfo? property;
            if (ConnectionString != null
                && (property = optionType.GetProperty(CONNECTION_STRING_NAME)) != null)
            {
                options = SupplyProperty(optionType, property, options, ConnectionString);
            } // conn

            // apply options if any
            if (options != null)
            {
                targetType.GetMethod("Configure")?.Invoke(component,
                    new[] { options });
            }

            return true;
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
            IList<ComponentFactoryConfigEntry> entries =
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, "JsonRenderers");

            ComponentFactoryConfigEntry? entry =
                entries.FirstOrDefault(e => e.Keys?.Contains(key) == true);
            if (entry == null) return null;

            IJsonRenderer? renderer = GetComponent<IJsonRenderer>(
                entry.Id!, entry.OptionsPath!);
            if (renderer == null) return null;

            // add filters if specified in Options/FilterKeys
            string filterKeys = Configuration.GetSection(entry.OptionsPath +
                ":FilterKeys").Get<string>();
            if (!string.IsNullOrWhiteSpace(filterKeys))
            {
                string[] keys = filterKeys.Split(' ',
                    StringSplitOptions.RemoveEmptyEntries);
                foreach (var filter in GetRendererFilters(keys))
                    renderer.Filters.Add(filter);
            }

            return renderer;
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

        /// <summary>
        /// Gets the JSON renderer filters matching any of the specified keys.
        /// Filters are listed under section <c>RendererFilters</c>, each with
        /// one or more keys.
        /// Then, these keys are used to include post-rendition filters by
        /// listing one or more of them in the <c>FilterKeys</c> option,
        /// a space-delimited string.
        /// </summary>
        /// <param name="keys">The desired keys.</param>
        /// <returns>Dictionary with keys and renderers.</returns>
        public IList<IRendererFilter> GetRendererFilters(IList<string> keys)
        {
            IList<ComponentFactoryConfigEntry> entries =
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, "RendererFilters");

            List<IRendererFilter> filters = new();
            foreach (ComponentFactoryConfigEntry entry in entries)
            {
                foreach (string key in entry.Keys!)
                {
                    IRendererFilter? filter = GetComponent<IRendererFilter>(
                        entry.Id!,
                        entry.OptionsPath!);
                    if (filter != null && keys?.Contains(key) != false)
                        filters.Add(filter);
                }
            }
            return filters;
        }
    }
}

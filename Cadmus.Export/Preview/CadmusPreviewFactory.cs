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
    /// <remarks>The JSON configuration has the following sections:
    /// <list type="bullet">
    /// <item>
    /// <term><c>RendererFilters</c></term>
    /// <description>List of renderer filters, each named with a key, and having
    /// its component ID and eventual options. The key is an arbitrary string,
    /// used in the scope of the configuration to reference each filter from
    /// other sections.</description>
    /// </item>
    /// <item>
    /// <term><c>JsonRenderers</c></term>
    /// <description>List of JSON renderers, each named with a key, and having
    /// its component ID and eventual options. The key corresponds to the part
    /// type ID, eventually followed by <c>|</c> and its role ID in the case
    /// of a layer part. This allows mapping each part type to a specific
    /// renderer ID. This key is used in the scope of the configuration to
    /// reference each filter from other sections. Under options, any renderer
    /// can have a <c>FilterKeys</c> property which is an array of filter keys,
    /// representing the filters used by that renderer, to be applied in the
    /// specified order.</description>
    /// </item>
    /// <item>
    /// <term><c>TextPartFlatteners</c></term>
    /// <description>List of text part flatteners, each named with a key, and
    /// having its component ID and eventual options. The key is an arbitrary
    /// string, used in the scope of the configuration to reference each filter
    /// from other sections.</description>
    /// </item>
    /// <item>
    /// <term><c>TextBlockRenderers</c></term>
    /// <description>List of text block renderers, each named with a key, and
    /// having its component ID and eventual options. The key is an arbitrary
    /// string, used in the scope of the configuration to reference each filter
    /// from other sections.</description>
    /// </item>
    /// <item>
    /// <term><c>ItemComposers</c></term>
    /// <description>List of item composers, each named with a key, and having
    /// its component ID and eventual options. The key is an arbitrary string,
    /// not used elsewhere in the context of the configuration. It is used as
    /// an argument for UI which process data export. Each composer can have
    /// among its options a <c>TextPartFlattenerKey</c> and a
    /// <c>TextBlockRendererKey</c>, referencing the corresponding components
    /// by their key, and a <c>JsonRendererKeys</c> array, referencing the
    /// corresponding JSON renderers by their key.</description>
    /// </item>
    /// <item>
    /// <term><c>ItemIdCollector</c></term>
    /// <description>A single item ID collector to use when required. It has
    /// the component ID, and eventual options.</description>
    /// </item>
    /// </list>
    /// </remarks>
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
        /// Initializes a new instance of the <see cref="CadmusPreviewFactory" />
        /// class.
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
                && (property = optionType.GetProperty(CONNECTION_STRING_NAME))
                != null)
            {
                options = SupplyProperty(optionType, property, options,
                    ConnectionString);
            }

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
            container.Collection.Register<IRendererFilter>(assemblies);
            container.Collection.Register<IItemComposer>(assemblies);
            container.Collection.Register<IItemIdCollector>(assemblies);

            // container.RegisterInstance(new UniData())
        }

        private HashSet<string> CollectKeys(string collectionPath)
        {
            HashSet<string> keys = new();
            foreach (var entry in
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, collectionPath)
                .Where(e => e.Keys?.Count > 0))
            {
                foreach (string id in entry.Keys!) keys.Add(id);
            }
            return keys;
        }

        /// <summary>
        /// Gets all the keys registered for JSON renderers in the
        /// configuration of this factory. This is used by client code
        /// to determine for which Cadmus objects a preview is available.
        /// </summary>
        /// <returns>List of unique keys.</returns>
        public HashSet<string> GetJsonRendererKeys()
            => CollectKeys("JsonRenderers");

        /// <summary>
        /// Gets all the keys registered for JSON text part flatteners
        /// in the configuration of this factory. This is used by client code
        /// to determine for which Cadmus objects a preview is available.
        /// </summary>
        /// <returns>List of unique keys.</returns>
        public HashSet<string> GetFlattenerKeys()
            => CollectKeys("TextPartFlatteners");

        /// <summary>
        /// Gets all the keys registered for item composers in the configuration
        /// of this factory.
        /// </summary>
        /// <returns>List of unique keys.</returns>
        public HashSet<string> GetComposerKeys()
            => CollectKeys("ItemComposers");

        private IList<IRendererFilter> GetFilters(string path)
        {
            IConfigurationSection filterKeys = Configuration.GetSection(path);
            if (filterKeys.Exists())
            {
                string[] keys = filterKeys.Get<string[]>();
                return GetRendererFilters(keys).ToList();
            }
            return Array.Empty<IRendererFilter>();
        }

        /// <summary>
        /// Gets the JSON renderer with the specified key. The renderer can
        /// specify filters in its <c>Options:FilterKeys</c> array property.
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

            // add filters if specified in Options:FilterKeys
            foreach (IRendererFilter filter in GetFilters(
                entry.OptionsPath + ":FilterKeys"))
            {
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
            IList<ComponentFactoryConfigEntry> entries =
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, "TextBlockRenderers");

            ComponentFactoryConfigEntry? entry =
                entries.FirstOrDefault(e => e.Keys?.Contains(key) == true);
            if (entry == null) return null;

            ITextBlockRenderer? renderer = GetComponent<ITextBlockRenderer>
                (entry.Id!, entry.OptionsPath!);
            if (renderer == null) return null;

            // add filters if specified in Options:FilterKeys
            foreach (IRendererFilter filter in GetFilters(
                entry.OptionsPath + ":FilterKeys"))
            {
                renderer.Filters.Add(filter);
            }

            return renderer;
        }

        /// <summary>
        /// Gets the text part flattener with the specified key.
        /// </summary>
        /// <param name="key">The key of the requested flattener.</param>
        /// <returns>Flattener or null if not found.</returns>
        public ITextPartFlattener? GetTextPartFlattener(string key)
        {
            return GetComponentByKey<ITextPartFlattener>
                ("TextPartFlatteners", key);
        }

        /// <summary>
        /// Gets the JSON renderer filters matching any of the specified keys.
        /// Filters are listed under section <c>RendererFilters</c>, each with
        /// one or more keys.
        /// Then, these keys are used to include post-rendition filters by
        /// listing one or more of them in the <c>FilterKeys</c> option,
        /// an array of strings.
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

        private IDictionary<string, IJsonRenderer> GetJsonRenderers(
            HashSet<string> keys, string collectionPath)
        {
            IList<ComponentFactoryConfigEntry> entries =
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, collectionPath);

            Dictionary<string, IJsonRenderer> renderers = new();
            foreach (ComponentFactoryConfigEntry entry in entries)
            {
                foreach (string key in entry.Keys!)
                {
                    IJsonRenderer? renderer = GetComponent<IJsonRenderer>(
                        entry.Id!,
                        entry.OptionsPath!);

                    if (renderer != null && keys?.Contains(key) != false)
                        renderers[key] = renderer;
                }
            }
            return renderers;
        }

        /// <summary>
        /// Gets an item composer by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Composer or null.</returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public IItemComposer? GetComposer(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            // ItemComposers: match by key
            IList<ComponentFactoryConfigEntry> entries =
                ComponentFactoryConfigEntry.ReadComponentEntries(
                Configuration, "ItemComposers");

            ComponentFactoryConfigEntry? entry =
                entries.FirstOrDefault(e => e.Keys?.Contains(key) == true);
            if (entry == null) return null;

            // instantiate composer
            IItemComposer? composer = GetComponent<IItemComposer>(
                entry.Id!, entry.OptionsPath!);
            if (composer == null) return null;

            // add text part flattener if specified in Options:TextPartFlattenerKey
            IConfigurationSection section = Configuration.GetSection(
                entry.OptionsPath + ":TextPartFlattenerKey");
            if (section.Exists())
            {
                string cKey = section.Get<string>();
                composer.TextPartFlattener = GetTextPartFlattener(cKey);
            }

            // add text block renderer if specified in Options:TextBlockRendererKey
            section = Configuration.GetSection(
                entry.OptionsPath + ":TextBlockRendererKey");
            if (section.Exists())
            {
                string cKey = section.Get<string>();
                composer.TextBlockRenderer = GetTextBlockRenderer(cKey);
            }

            // add renderers if specified in Options.JsonRendererKeys
            section = Configuration.GetSection(
                entry.OptionsPath + ":JsonRendererKeys");
            if (section.Exists())
            {
                foreach (string cKey in section.Get<string[]>())
                {
                    IJsonRenderer? renderer = GetJsonRenderer(cKey);
                    if (renderer != null) composer.JsonRenderers[cKey] = renderer;
                }
            }

            return composer;
        }

        /// <summary>
        /// Gets the item identifiers collector if any.
        /// </summary>
        /// <returns>The collector defined in this factory configuration,
        /// or null.</returns>
        public IItemIdCollector? GetItemIdCollector()
        {
            ComponentFactoryConfigEntry? entry =
                ComponentFactoryConfigEntry.ReadComponentEntry(Configuration,
                "ItemIdCollector");
            if (entry == null) return null;

            return GetComponent<IItemIdCollector>(entry.Id!, entry.OptionsPath!);
        }
    }
}

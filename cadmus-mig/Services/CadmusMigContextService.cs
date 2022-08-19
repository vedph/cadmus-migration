using Cadmus.Cli.Core;
using Cadmus.Export.Preview;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Cadmus.Migration.Cli.Services
{
    /// <summary>
    /// CLI context service.
    /// </summary>
    public sealed class CadmusMigCliContextService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CadmusMigCliContextService"/>
        /// class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public CadmusMigCliContextService(CadmusMigCliContextServiceConfig config)
        {
        }

        /// <summary>
        /// Gets the preview factory provider with the specified plugin tag
        /// (assuming that the plugin has a (single) implementation of
        /// <see cref="ICadmusPreviewFactoryProvider"/>).
        /// </summary>
        /// <param name="pluginTag">The tag of the component in its plugin,
        /// or null to use the standard preview factory provider.</param>
        /// <returns>The provider.</returns>
        public ICadmusPreviewFactoryProvider? GetFactoryProvider(
            string? pluginTag = null)
        {
            if (pluginTag == null)
                return new StandardPreviewFactoryProvider();

            return PluginFactoryProvider
                .GetFromTag<ICadmusPreviewFactoryProvider>(pluginTag);
        }
    }

    /// <summary>
    /// Configuration for <see cref="CadmusMigCliContextService"/>.
    /// </summary>
    public class CadmusMigCliContextServiceConfig
    {
        /// <summary>
        /// Gets or sets the connection string to the database.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the local directory to use when loading resources
        /// from the local file system.
        /// </summary>
        public string? LocalDirectory { get; set; }
    }
}
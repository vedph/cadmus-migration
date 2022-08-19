using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Fusi.Tools.Config;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System.Reflection;

namespace Cadmus.Export.Preview
{
    /// <summary>
    /// Standard preview factory provider.
    /// </summary>
    /// <seealso cref="ICadmusPreviewFactoryProvider" />
    [Tag("it.vedph.preview-factory-provider.standard")]
    public sealed class StandardPreviewFactoryProvider : ICadmusPreviewFactoryProvider
    {
        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <param name="profile">The JSON configuration profile.</param>
        /// <param name="additionalAssemblies">The optional additional assemblies
        /// to load components from.</param>
        /// <returns>Factory.</returns>
        public CadmusPreviewFactory GetFactory(string profile,
            params Assembly[] additionalAssemblies)
        {
            // build the container
            Container container = new();
            CadmusPreviewFactory.ConfigureServices(container, additionalAssemblies);
            container.Verify();

            // load seed configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryJson(profile);
            var configuration = builder.Build();

            return new CadmusPreviewFactory(container, configuration);
        }
    }
}

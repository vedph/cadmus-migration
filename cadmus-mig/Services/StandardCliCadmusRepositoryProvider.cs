using Cadmus.Core.Config;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Cadmus.Mongo;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using Cadmus.Philology.Parts;
using Cadmus.Cli.Core;

namespace Cadmus.Migration.Cli.Services
{
    public sealed class StandardCliCadmusRepositoryProvider : ICliCadmusRepositoryProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IPartTypeProvider _partTypeProvider;

        public string? ConnectionString { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardRepositoryProvider"/>
        /// class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="ArgumentNullException">configuration</exception>
        public StandardCliCadmusRepositoryProvider(IConfiguration configuration)
        {
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));

            TagAttributeToTypeMap map = new();
            map.Add(new[]
            {
                // Cadmus.General.Parts
                typeof(NotePart).GetTypeInfo().Assembly,
                // Cadmus.Philology.Parts
                typeof(ApparatusLayerFragment).GetTypeInfo().Assembly,
            });

            _partTypeProvider = new StandardPartTypeProvider(map);
        }

        /// <summary>
        /// Gets the part type provider.
        /// </summary>
        /// <returns>part type provider</returns>
        public IPartTypeProvider GetPartTypeProvider()
        {
            return _partTypeProvider;
        }

        /// <summary>
        /// Creates a Cadmus repository.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <returns>repository</returns>
        /// <exception cref="ArgumentNullException">null database</exception>
        public ICadmusRepository CreateRepository(string database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            // create the repository (no need to use container here)
            MongoCadmusRepository repository =
                new(_partTypeProvider, new StandardItemSortKeyBuilder());

            repository.Configure(new MongoCadmusRepositoryOptions
            {
                ConnectionString = ConnectionString
            });

            return repository;
        }
    }
}

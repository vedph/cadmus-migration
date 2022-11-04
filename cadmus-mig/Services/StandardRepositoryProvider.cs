﻿using Cadmus.Core.Config;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Cadmus.Mongo;
using System;
using System.Reflection;
using Cadmus.Philology.Parts;
using Fusi.Tools.Config;

namespace Cadmus.Migration.Cli.Services
{
    /// <summary>
    /// Standard repository provider using the core general and philology parts
    /// from Cadmus.
    /// Tag: <c>repository-provider.standard</c>.
    /// </summary>
    /// <seealso cref="IRepositoryProvider" />
    [Tag("repository-provider.standard")]
    public sealed class StandardRepositoryProvider : IRepositoryProvider
    {
        private readonly IPartTypeProvider _partTypeProvider;

        /// <summary>
        /// The connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardRepositoryProvider"/>
        /// class.
        /// </summary>
        /// <exception cref="ArgumentNullException">configuration</exception>
        public StandardRepositoryProvider()
        {
            ConnectionString = "";
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
        /// <returns>repository</returns>
        /// <exception cref="ArgumentNullException">null database</exception>
        public ICadmusRepository CreateRepository()
        {
            // create the repository (no need to use container here)
            MongoCadmusRepository repository =
                new(_partTypeProvider, new StandardItemSortKeyBuilder());

            repository.Configure(new MongoCadmusRepositoryOptions
            {
                ConnectionString = ConnectionString ??
                throw new InvalidOperationException(
                    "No connection string set for IRepositoryProvider implementation")
            });

            return repository;
        }
    }
}

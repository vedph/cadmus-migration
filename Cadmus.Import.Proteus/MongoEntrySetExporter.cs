using Cadmus.Core.Config;
using Cadmus.Core;
using Cadmus.Mongo;
using Fusi.Tools.Configuration;
using MongoDB.Driver;
using Proteus.Core.Regions;
using System.Text.Json.Nodes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Import.Proteus;

/// <summary>
/// Mongo-based Cadmus entry set exporter.
/// <para>Tag: <c>entry-set-exporter.cadmus.mongo</c>.</para>
/// </summary>
/// <seealso cref="Proteus.Core.Regions.IEntrySetExporter" />
[Tag("entry-set-exporter.cadmus.mongo")]
public sealed class MongoEntrySetExporter : MongoConsumerBase, IEntrySetExporter,
    IConfigurable<MongoEntrySetExporterOptions>
{
    private readonly HashSet<string> _partProps =
    [
        "id", "itemId", "typeId", "roleId", "thesaurusScope",
        "timeCreated", "creatorId", "timeModified", "userId"
    ];

private MongoEntrySetExporterOptions? _options;
    private MongoCadmusRepository? _repository;

    /// <summary>
    /// Configures this exporter with the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    public void Configure(MongoEntrySetExporterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Opens the exporter output. Call this once from outside the pipeline,
    /// when you want to start exporting. This will initialize the database
    /// connection objects.
    /// </summary>
    /// <exception cref="InvalidOperationException">No connection string for
    /// MongoEntrySetExporter</exception>
    public Task OpenAsync()
    {
        if (string.IsNullOrEmpty(_options?.ConnectionString))
        {
            throw new InvalidOperationException("No connection string for "
                + nameof(MongoEntrySetExporter));
        }
        EnsureClientCreated(_options.ConnectionString);

        // we do not need to configure part types as we are using JSON for them
        _repository = new(new StandardPartTypeProvider(new TagAttributeToTypeMap()),
            new StandardItemSortKeyBuilder());
        _repository.Configure(new MongoCadmusRepositoryOptions
        {
            ConnectionString = _options.ConnectionString
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Closes the exporter output. Call this once from outside the pipeline,
    /// when you want to end exporting. Here this method does nothing.
    /// </summary>
    public Task CloseAsync()
    {
        return Task.CompletedTask;
    }

    private JsonNode GetPartNode(JsonNode content)
    {
        JsonNode part = new JsonObject();

        foreach (string pn in _partProps)
        {
            if (content.AsObject().TryGetPropertyValue(pn, out JsonNode? pv))
            {
                part[pn] = pv;
            }
        }

        // add a new property 'content' to the new node, which is equal
        // to the original node
        part["content"] = content;

        return part;
    }

    /// <summary>
    /// Exports the specified entry set.
    /// </summary>
    /// <param name="entrySet">The entry set.</param>
    /// <param name="regionSet">The entry regions set.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException">
    /// No connection string for MongoEntrySetExporter or
    /// Mongo database DBNAME not open.
    /// </exception>
    public Task ExportAsync(EntrySet entrySet, EntryRegionSet regionSet)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(regionSet);

        if (string.IsNullOrEmpty(_options?.ConnectionString))
        {
            throw new InvalidOperationException("No connection string for "
                + nameof(MongoEntrySetExporter));
        }
        if (Client == null)
        {
            throw new InvalidOperationException(
                $"Mongo database {GetDatabaseName(_options.ConnectionString)} not open");
        }

        // get context
        CadmusEntrySetContext context = (CadmusEntrySetContext)entrySet.Context;
        if (context.Items.Count == 0) return Task.CompletedTask;

        // write items
        IMongoDatabase db = Client.GetDatabase(GetDatabaseName(
            _options.ConnectionString));
        foreach (ImportedItem item in context.Items)
        {
            // write item without parts
            _repository!.AddItem(item.ToItem(), !_options.NoHistory);

            // write parts
            foreach (JsonNode partContent in item.Parts)
            {
                IMongoCollection<BsonDocument> parts =
                    db.GetCollection<BsonDocument>(MongoPart.COLLECTION);

                JsonNode part = GetPartNode(partContent);
                BsonDocument document = BsonDocument.Parse(part.ToJsonString());
                parts.InsertOne(document);

                // history if required
                if (!_options.NoHistory)
                {
                    part["referenceId"] = part["id"];
                    part["status"] = 0;
                    part["id"] = Guid.NewGuid().ToString();
                    document = BsonDocument.Parse(part.ToJsonString());
                    parts.InsertOne(document);
                }
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Options for <see cref="MongoEntrySetExporter"/>.
/// </summary>
public class MongoEntrySetExporterOptions
{
    /// <summary>
    /// Gets or sets the connection string to the Cadmus database to write to.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether imported data should not be
    /// inserted in the history.
    /// </summary>
    public bool NoHistory { get; set; }
}

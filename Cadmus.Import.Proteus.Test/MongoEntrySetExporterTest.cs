using Cadmus.Core.Config;
using Cadmus.Core;
using Cadmus.Mongo;
using System;
using System.Threading.Tasks;
using Xunit;
using Cadmus.General.Parts;
using System.Text.Json;
using System.Text.Json.Nodes;
using Proteus.Core.Regions;

namespace Cadmus.Import.Proteus.Test;

// this test requires MongoDB

public sealed class MongoEntrySetExporterTest
{
    private const string CONNECTION_STRING = "mongodb://localhost:27017/cadmus-test";

    private MongoCadmusRepository CreateRepository()
    {
        TagAttributeToTypeMap map = new();
        map.Add([typeof(NotePart).Assembly]);
        MongoCadmusRepository repository = new(
            new StandardPartTypeProvider(map),
            new StandardItemSortKeyBuilder());

        repository.Configure(new MongoCadmusRepositoryOptions
        {
            ConnectionString = CONNECTION_STRING,
        });

        return repository;
    }

    [Fact]
    public async Task ExportAsync_Ok()
    {
        // build data context
        CadmusEntrySetContext context = new();
        string[] guids = [ "061054da-3c43-41d5-806c-2bd0e4957eff",
            "7c3d45dd-0376-48a3-a060-659c1cd9d6f2" ];

        for (int n = 1; n <= 2; n++)
        {
            // item
            ImportedItem item = new()
            {
                Id = guids[n - 1],
                Title = $"Item {n}",
                Description = $"Item nr.{n}",
                FacetId = "default",
                SortKey = $"{n:000}",
                TimeCreated = DateTime.Now,
                CreatorId = "zeus",
                UserId = "zeus",
            };

            // parts
            NotePart part = new()
            {
                ItemId = item.Id,
                Tag = "tag",
                Text = $"Note about {n}",
                CreatorId = "zeus",
                UserId = "zeus",
            };
            string json = JsonSerializer.Serialize(part, part.GetType());
            JsonNode node = JsonNode.Parse(json)!;
            item.Parts.Add(node);

            context.Items.Add(item);
        }

        // create exporter
        MongoEntrySetExporter exporter = new();
        exporter.Configure(new MongoEntrySetExporterOptions
        {
            ConnectionString = CONNECTION_STRING
        });

        // export
        await exporter.ExportAsync(new EntrySet(context), new EntryRegionSet());

        // check
        MongoCadmusRepository repository = CreateRepository();
        for (int n = 1; n <= 2; n++)
        {
            IItem? item = repository.GetItem(guids[0], true);
            Assert.NotNull(item);
            Assert.Equal($"Item {n}", item!.Title);
            Assert.Equal($"Item nr.{n}", item.Description);
            Assert.Equal("default", item.FacetId);
            Assert.Equal($"{n:000}", item.SortKey);
            Assert.Equal("zeus", item.CreatorId);
            Assert.Equal("zeus", item.UserId);
            Assert.Single(item.Parts);
            // note part
            NotePart? part = item.Parts[0] as NotePart;
            Assert.NotNull(part);
            Assert.Equal("tag", part!.Tag);
            Assert.Equal($"Note about {n}", part.Text);
            Assert.Equal(item.Id, part.ItemId);
            Assert.Equal("zeus", part.CreatorId);
            Assert.Equal("zeus", part.UserId);
        }
    }
}
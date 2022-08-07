﻿using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Export.Preview;
using Cadmus.General.Parts;
using Cadmus.Mongo;
using Cadmus.Philology.Parts;
using Cadmus.Refs.Bricks;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using SimpleInjector;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Cadmus.Export.Test
{
    [Collection(nameof(NonParallelResourceCollection))]
    public sealed class CadmusPreviewerTest
    {
        private const string DB_NAME = "cadmus-test";
        private const string TEXT_ID = "9a801c84-0c93-4074-b071-9f4f9885ba66";
        private const string ORTH_ID = "c99072ea-c488-484b-ac37-e22027039dc0";
        private const string COMM_ID = "b7bc0fec-4a69-42d1-835b-862330c6e7fa";

        private readonly MongoClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public CadmusPreviewerTest()
        {
            _client = new MongoClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private MongoPart CreateMongoPart(IPart part)
        {
            string content =
                JsonSerializer.Serialize(part, part.GetType(), _jsonOptions);

            return new MongoPart(part)
            {
                Content = BsonDocument.Parse(content)
            };
        }

        internal static TokenTextPart GetSampleTextPart()
        {
            // text
            TokenTextPart text = new()
            {
                Id = TEXT_ID,
                ItemId = "item",
                CreatorId = "zeus",
                UserId = "zeus",
                Citation = "CIL 1,23",
            };
            text.Lines.Add(new TextLine
            {
                Y = 1,
                Text = "que bixit"
            });
            text.Lines.Add(new TextLine
            {
                Y = 2,
                Text = "annos XX"
            });
            return text;
        }

        private void SeedData(IMongoDatabase db)
        {
            // we need just parts for our tests
            IMongoCollection<MongoPart> parts = db.GetCollection<MongoPart>
                (MongoPart.COLLECTION);

            // 0123456789-1234567
            // que bixit|annos XX
            // ..O............... 1.1@3
            // ....O............. 1.2@1
            // ....CCCCCCCCCCC... 1.2-2.1
            // ................CC 2.2

            // text
            TokenTextPart text = GetSampleTextPart();
            parts.InsertOne(CreateMongoPart(text));

            // orthography
            TokenTextLayerPart<OrthographyLayerFragment> orthLayer = new()
            {
                Id = ORTH_ID,
                ItemId = "item",
                CreatorId = "zeus",
                UserId = "zeus"
            };
            // qu[e]
            orthLayer.Fragments.Add(new OrthographyLayerFragment
            {
                Location = "1.1@3",
                Standard = "ae"
            });
            // [b]ixit
            orthLayer.Fragments.Add(new OrthographyLayerFragment
            {
                Location = "1.2@1",
                Standard = "v"
            });
            parts.InsertOne(CreateMongoPart(orthLayer));

            // comment
            TokenTextLayerPart<CommentLayerFragment> commLayer = new()
            {
                Id = COMM_ID,
                ItemId = "item",
                CreatorId = "zeus",
                UserId = "zeus"
            };
            // bixit annos
            commLayer.AddFragment(new CommentLayerFragment
            {
                Location = "1.2-2.1",
                Text = "acc. rather than abl. is rarer but attested.",
                References = new List<DocReference>
                {
                    new DocReference
                    {
                        Citation = "Sandys 1927 63",
                        Tag = "m",
                        Type = "book"
                    }
                }
            });
            // XX
            commLayer.AddFragment(new CommentLayerFragment
            {
                Location = "2.2",
                Text = "for those morons not knowing this, it's 20."
            });
            parts.InsertOne(CreateMongoPart(commLayer));
        }

        private void InitDatabase()
        {
            // camel case everything:
            // https://stackoverflow.com/questions/19521626/mongodb-convention-packs/19521784#19521784
            ConventionPack pack = new()
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("camel case", pack, _ => true);

            _client.DropDatabase(DB_NAME);
            IMongoDatabase db = _client.GetDatabase(DB_NAME);

            SeedData(db);
        }

        private static ICadmusRepository GetRepository()
        {
            TagAttributeToTypeMap map = new();
            map.Add(new[]
            {
                typeof(NotePart).Assembly,
                typeof(ApparatusLayerFragment).Assembly
            });
            MongoCadmusRepository repository = new(
                new StandardPartTypeProvider(map),
                new StandardItemSortKeyBuilder());
            repository.Configure(new MongoCadmusRepositoryOptions
            {
                // use the default ConnectionStringTemplate (local DB)
                ConnectionString = "mongodb://localhost:27017/" + DB_NAME
            });
            return repository;
        }

        private static CadmusPreviewer GetPreviewer(ICadmusRepository repository)
        {
            CadmusPreviewFactory factory = TestHelper.GetFactory();
            return new(repository, factory);
        }

        [Fact]
        public void RenderPart_NullWithText_Ok()
        {
            InitDatabase();
            ICadmusRepository repository = GetRepository();
            CadmusPreviewer previewer = GetPreviewer(repository);

            string json = previewer.RenderPart(TEXT_ID);

            string json2 = repository.GetPartContent(TEXT_ID);
            Assert.Equal(json, json2);
        }

        private static JsonElement? GetFragmentAt(JsonElement fragments, int index)
        {
            if (index >= fragments.GetArrayLength()) return null;

            int i = 0;
            foreach (JsonElement fr in fragments.EnumerateArray())
            {
                if (i == index) return fr;
                i++;
            }
            return null;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void RenderFragment_NullWithOrth_Ok(int index)
        {
            InitDatabase();
            ICadmusRepository repository = GetRepository();
            CadmusPreviewer previewer = GetPreviewer(repository);

            string json2 = previewer.RenderFragment(ORTH_ID, index);

            string json = repository.GetPartContent(ORTH_ID);
            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement fragments = doc.RootElement
                .GetProperty("fragments");
            JsonElement fr = GetFragmentAt(fragments, index)!.Value;
            json = fr.ToString();

            Assert.Equal(json, json2);
        }

        [Fact]
        public void BuildTextBlocks_Ok()
        {
            // 0123456789-1234567
            // que bixit|annos XX
            // ..O............... 1.1@3   L0F0
            // ....O............. 1.2@1   L0F1
            // ....CCCCCCCCCCC... 1.2-2.1 L1F0
            // ................CC 2.2     L1F1

            InitDatabase();
            ICadmusRepository repository = GetRepository();
            CadmusPreviewer previewer = GetPreviewer(repository);

            IList<TextBlockRow> rows = previewer.BuildTextBlocks(TEXT_ID, new[]
            {
                ORTH_ID,
                COMM_ID
            });

            Assert.Equal(2, rows.Count);

            // row 0
            TextBlockRow row = rows[0];
            Assert.Equal(5, row.Blocks.Count);
            // qu: -
            Assert.Equal("qu", row.Blocks[0].Text);
            Assert.Equal(0, row.Blocks[0].LayerIds.Count);
            // e: AB
            Assert.Equal("e", row.Blocks[1].Text);
            Assert.Equal(1, row.Blocks[1].LayerIds.Count);
            Assert.Equal("L0F0", row.Blocks[1].LayerIds[0]);
            // _: -
            Assert.Equal(" ", row.Blocks[2].Text);
            Assert.Equal(0, row.Blocks[2].LayerIds.Count);
            // b: OC
            Assert.Equal("b", row.Blocks[3].Text);
            Assert.Equal(2, row.Blocks[3].LayerIds.Count);
            Assert.Equal("L0F1", row.Blocks[3].LayerIds[0]);
            Assert.Equal("L1F0", row.Blocks[3].LayerIds[1]);
            // ixit: C
            Assert.Equal("ixit", row.Blocks[4].Text);
            Assert.Equal(1, row.Blocks[4].LayerIds.Count);
            Assert.Equal("L1F0", row.Blocks[4].LayerIds[0]);

            // row 1
            row = rows[1];
            Assert.Equal(3, row.Blocks.Count);
            // annos: C
            Assert.Equal("annos", row.Blocks[0].Text);
            Assert.Equal(1, row.Blocks[0].LayerIds.Count);
            Assert.Equal("L1F0", row.Blocks[0].LayerIds[0]);
            // _: -
            Assert.Equal(" ", row.Blocks[1].Text);
            Assert.Equal(0, row.Blocks[1].LayerIds.Count);
            // XX: D
            Assert.Equal("XX", row.Blocks[2].Text);
            Assert.Equal(1, row.Blocks[2].LayerIds.Count);
            Assert.Equal("L1F1", row.Blocks[2].LayerIds[0]);
        }
    }
}

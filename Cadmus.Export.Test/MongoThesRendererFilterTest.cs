using Cadmus.Core.Config;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Xunit;

namespace Cadmus.Export.Test
{
    public sealed class MongoThesRendererFilterTest
    {
        private const string DB_NAME = "cadmus-test";
        private readonly MongoClient _client;

        public MongoThesRendererFilterTest()
        {
            _client = new MongoClient(TestHelper.CS);
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

            Thesaurus thesaurus = new()
            {
                Id = "colors@en"
            };
            thesaurus.Entries.Add(new ThesaurusEntry
            {
                Id = "r",
                Value = "red"
            });
            thesaurus.Entries.Add(new ThesaurusEntry
            {
                Id = "g",
                Value = "green"
            });
            thesaurus.Entries.Add(new ThesaurusEntry
            {
                Id = "b",
                Value = "blue"
            });
            db.GetCollection<Thesaurus>("thesauri").InsertOne(thesaurus);
        }

        [Fact]
        public void Apply_NoMatch_Unchanged()
        {
            InitDatabase();
            MongoThesRendererFilter filter = new();
            filter.Configure(new MongoThesRendererFilterOptions
            {
                ConnectionString = TestHelper.CS,
            });

            string result = filter.Apply("No match here");

            Assert.Equal("No match here", result);
        }

        [Fact]
        public void Apply_Match_Ok()
        {
            InitDatabase();
            MongoThesRendererFilter filter = new();
            filter.Configure(new MongoThesRendererFilterOptions
            {
                ConnectionString = TestHelper.CS,
            });

            string result = filter.Apply("My color is: $colors:r!");

            Assert.Equal("My color is: red!", result);
        }
    }
}

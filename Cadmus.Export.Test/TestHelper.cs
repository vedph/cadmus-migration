using Cadmus.Export.Preview;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using System.IO;
using System.Reflection;
using System.Text;

namespace Cadmus.Export.Test
{
    internal static class TestHelper
    {
        public static string CS = "mongodb://localhost:27017/cadmus-test";

        public static string LoadResourceText(string name)
        {
            using StreamReader reader = new(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Cadmus.Export.Test.Assets.{name}")!,
                Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public static CadmusPreviewFactory GetFactory()
        {
            Container container = new();
            CadmusPreviewFactory.ConfigureServices(container);

            ConfigurationBuilder cb = new();
            IConfigurationRoot config = cb
                .AddInMemoryJson(LoadResourceText("Preview.json"))
                .Build();
            return new CadmusPreviewFactory(container, config)
            {
                ConnectionString = "mongodb://localhost:27017/cadmus-test"
            };
        }
    }
}

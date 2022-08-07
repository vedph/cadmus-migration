using System.IO;
using System.Reflection;
using System.Text;

namespace Cadmus.Export.Test
{
    internal static class TestHelper
    {
        public static string LoadResourceText(string name)
        {
            using StreamReader reader = new(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Cadmus.Export.Test.Assets.{name}")!,
                Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}

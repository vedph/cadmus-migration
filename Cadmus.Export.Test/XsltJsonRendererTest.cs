using Cadmus.General.Parts;
using System.Text.Json;
using Xunit;

namespace Cadmus.Export.Test
{
    public sealed class XsltJsonRendererTest
    {
        [Fact]
        public void Render_NoJsonTx_Ok()
        {
            XsltJsonRenderer renderer = new();
            renderer.Configure(new XsltPartRendererOptions
            {
                Xslt = TestHelper.LoadResourceText("TokenTextPart.xslt")
            });
            TokenTextPart text = CadmusPreviewerTest.GetSampleTextPart();
            string json = JsonSerializer.Serialize(text, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            string result = renderer.Render(json);

            Assert.NotNull(result);
            Assert.Equal("[CIL 1,23]\n1  que bixit\n2  annos XX\n", result);
        }
    }
}

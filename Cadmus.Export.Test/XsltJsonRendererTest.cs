using Cadmus.Export.Filters;
using Cadmus.General.Parts;
using System.Text.Json;
using Xunit;

namespace Cadmus.Export.Test
{
    public sealed class XsltJsonRendererTest
    {
        [Fact]
        public void Render_XsltOnly_Ok()
        {
            XsltJsonRenderer renderer = new();
            renderer.Configure(new XsltJsonRendererOptions
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
            Assert.Equal("[CIL 1,23]\r\n1  que bixit\r\n2  annos XX\r\n", result);
        }

        [Fact]
        public void Render_JmesPathOnly_Ok()
        {
            XsltJsonRenderer renderer = new();
            renderer.Configure(new XsltJsonRendererOptions
            {
                JsonExpressions = new[] { "root.citation" },
                QuoteStripping = true
            });
            TokenTextPart text = CadmusPreviewerTest.GetSampleTextPart();
            string json = JsonSerializer.Serialize(text, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            string result = renderer.Render(json);

            Assert.NotNull(result);
            Assert.Equal("CIL 1,23", result);
        }

        [Fact]
        public void Render_JmesPathOnlyMd_Ok()
        {
            XsltJsonRenderer renderer = new();
            renderer.Configure(new XsltJsonRendererOptions
            {
                JsonExpressions = new[] { "root.text" },
                QuoteStripping = true,
            });
            MarkdownRendererFilter filter = new();
            filter.Configure(new MarkdownRendererFilterOptions
            {
                Format = "html"
            });
            renderer.Filters.Add(filter);

            NotePart note = new()
            {
                CreatorId = "zeus",
                UserId = "zeus",
                Text = "This is a *note* using MD"
            };
            string json = JsonSerializer.Serialize(note, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            string result = renderer.Render(json);

            Assert.NotNull(result);
            Assert.Equal("<p>This is a <em>note</em> using MD</p>\n", result);
        }
    }
}

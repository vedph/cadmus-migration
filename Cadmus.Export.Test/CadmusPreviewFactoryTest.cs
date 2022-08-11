using Cadmus.Export.Preview;
using System.Collections.Generic;
using Xunit;

namespace Cadmus.Export.Test
{
    public sealed class CadmusPreviewFactoryTest
    {
        [Fact]
        public void GetKeys_Renderers_Ok()
        {
            CadmusPreviewFactory factory = TestHelper.GetFactory();

            HashSet<string>? keys = factory.GetKeys(false);

            Assert.Equal(3, keys.Count);
            Assert.Contains("it.vedph.token-text", keys);
            Assert.Contains("it.vedph.token-text-layer|fr.it.vedph.comment", keys);
            Assert.Contains("it.vedph.token-text-layer|fr.it.vedph.orthography", keys);
        }

        [Fact]
        public void GetKeys_Flatteners_Ok()
        {
            CadmusPreviewFactory factory = TestHelper.GetFactory();

            HashSet<string>? keys = factory.GetKeys(true);

            Assert.Single(keys);
            Assert.Contains("it.vedph.token-text", keys);
        }

        [Fact]
        public void GetJsonRenderer_WithFilters_Ok()
        {
            CadmusPreviewFactory factory = TestHelper.GetFactory();

            IJsonRenderer? renderer = factory.GetJsonRenderer("it.vedph.token-text");

            Assert.NotNull(renderer);
            Assert.Equal(3, renderer.Filters.Count);
            Assert.Equal(typeof(MongoThesRendererFilter),
                renderer.Filters[0].GetType());
            Assert.Equal(typeof(ReplaceRendererFilter),
                renderer.Filters[1].GetType());
            Assert.Equal(typeof(MarkdownRendererFilter),
                renderer.Filters[2].GetType());
        }
    }
}

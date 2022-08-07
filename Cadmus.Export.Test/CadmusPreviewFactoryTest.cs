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
    }
}

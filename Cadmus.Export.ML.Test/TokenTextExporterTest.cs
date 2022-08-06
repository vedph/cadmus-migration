using Cadmus.Core;
using Cadmus.General.Parts;
using Cadmus.Philology.Parts;
using Fusi.Tools.Text;
using System.Collections.Generic;
using Xunit;

namespace Cadmus.Export.ML.Test
{
    public sealed class TokenTextExporterTest
    {
        private static TokenTextPart GetTextPart(IList<string> lines)
        {
            TokenTextPart part = new();
            int y = 1;
            foreach (string line in lines)
            {
                part.Lines.Add(new TextLine
                {
                    Y = y++,
                    Text = line
                });
            }
            return part;
        }

        // 123 12345
        // que bixit
        // 12345 12
        // annos XX
        //    0123456789-1234567
        // => que bixit|annos XX
        private static TokenTextPart GetSampleTextPart()
            => GetTextPart(new[] { "que bixit", "annos XX" });

        private static IList<IPart> GetSampleLayerParts()
        {
            List<IPart> parts = new();

            // qu[e]
            TokenTextLayerPart<OrthographyLayerFragment>? oLayer = new();
            oLayer.Fragments.Add(new OrthographyLayerFragment
            {
                Location = "1.1@3"
            });
            parts.Add(oLayer);

            // qu[e b]ixit
            TokenTextLayerPart<LigatureLayerFragment>? lLayer = new();
            lLayer.Fragments.Add(new LigatureLayerFragment
            {
                Location = "1.1@3-1.2@1"
            });
            parts.Add(lLayer);

            // [bixit annos]
            TokenTextLayerPart<CommentLayerFragment>? cLayer = new();
            cLayer.Fragments.Add(new CommentLayerFragment
            {
                Location = "1.2-2.1"
            });
            // XX
            cLayer.Fragments.Add(new CommentLayerFragment
            {
                Location = "2.2"
            });
            parts.Add(cLayer);

            return parts;
        }

        [Fact]
        public void BuildRangeSet_Ok()
        {
            TokenTextExporter exporter = new();
            TokenTextPart textPart = GetSampleTextPart();
            IList<IPart> layerParts = GetSampleLayerParts();

            var tr = exporter.GetTextRanges(textPart, layerParts);

            // text
            Assert.Equal("que bixit\nannos XX", tr.Item1);

            // ranges
            IList<MergedRange> ranges = tr.Item2;
            Assert.Equal(4, ranges.Count);

            // qu[e]
            MergedRange r = ranges[0];
            Assert.Equal(2, r.Start);
            Assert.Equal(2, r.End);
            Assert.Equal("L0F0", r.Id);
            Assert.Equal("L0", r.GroupId);

            // qu[e b]ixit
            r = ranges[1];
            Assert.Equal(2, r.Start);
            Assert.Equal(4, r.End);
            Assert.Equal("L1F0", r.Id);
            Assert.Equal("L1", r.GroupId);

            // [bixit annos]
            r = ranges[2];
            Assert.Equal(4, r.Start);
            Assert.Equal(14, r.End);
            Assert.Equal("L2F0", r.Id);
            Assert.Equal("L2", r.GroupId);

            // XX
            r = ranges[3];
            Assert.Equal(16, r.Start);
            Assert.Equal(17, r.End);
            Assert.Equal("L2F1", r.Id);
            Assert.Equal("L2", r.GroupId);
        }
    }
}
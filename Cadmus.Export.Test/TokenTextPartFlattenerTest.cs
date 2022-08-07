using Cadmus.Core;
using Cadmus.General.Parts;
using Cadmus.Philology.Parts;
using Fusi.Tools.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Cadmus.Export.Test
{
    public sealed class TokenTextPartFlattenerTest
    {
        internal static TokenTextPart GetTextPart(IList<string> lines)
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
        // que vixit
        // 12345 12
        // annos XX
        //    0123456789-1234567
        // => que vixit|annos XX
        internal static TokenTextPart GetSampleTextPart()
            => GetTextPart(new[] { "que vixit", "annos XX" });

        internal static IList<IPart> GetSampleLayerParts()
        {
            List<IPart> parts = new();

            // qu[e]
            TokenTextLayerPart<OrthographyLayerFragment>? oLayer = new();
            oLayer.Fragments.Add(new OrthographyLayerFragment
            {
                Location = "1.1@3"
            });
            parts.Add(oLayer);

            // qu[e v]ixit
            TokenTextLayerPart<LigatureLayerFragment>? lLayer = new();
            lLayer.Fragments.Add(new LigatureLayerFragment
            {
                Location = "1.1@3-1.2@1"
            });
            parts.Add(lLayer);

            // [vixit annos]
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

        private static string RenderTextWithRanges(string text, MergedRangeSet set)
        {
            StringBuilder sb = new();
            IList<MergedRange> ranges;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    sb.Append("</p>\n<p>");
                    continue;
                }

                if (i == 0 || !set.AreEqualAt(i, i - 1))
                {
                    if (i > 0) sb.Append("</span>");

                    ranges = set.GetRangesAt(i);
                    sb.Append("<span");
                    if (ranges.Count > 0)
                    {
                        sb.Append(" class=\"");
                        sb.AppendJoin(" ", ranges.Select(r => r.Id));
                        sb.Append('"');
                    }
                    sb.Append('>');
                }
                sb.Append(text[i]);
            }
            if (sb.Length > 0) sb.Append("</span>");
            sb.Insert(0, "<p>");
            sb.Append("</p>");

            return sb.ToString();
        }

        [Fact]
        public void GetTextRanges_Ok()
        {
            TokenTextPartFlattener exporter = new();
            TokenTextPart textPart = GetSampleTextPart();
            IList<IPart> layerParts = GetSampleLayerParts();

            var tr = exporter.GetTextRanges(textPart, layerParts);

            // text
            Assert.Equal("que vixit\nannos XX", tr.Item1);

            // ranges
            MergedRangeSet set = tr.Item2;
            Assert.Equal(4, set.Ranges.Count);

            // qu[e]
            MergedRange r = set.Ranges[0];
            Assert.Equal(2, r.Start);
            Assert.Equal(2, r.End);
            Assert.Equal("L0F0", r.Id);
            Assert.Equal("L0", r.GroupId);

            // qu[e b]ixit
            r = set.Ranges[1];
            Assert.Equal(2, r.Start);
            Assert.Equal(4, r.End);
            Assert.Equal("L1F0", r.Id);
            Assert.Equal("L1", r.GroupId);

            // [bixit annos]
            r = set.Ranges[2];
            Assert.Equal(4, r.Start);
            Assert.Equal(14, r.End);
            Assert.Equal("L2F0", r.Id);
            Assert.Equal("L2", r.GroupId);

            // XX
            r = set.Ranges[3];
            Assert.Equal(16, r.Start);
            Assert.Equal(17, r.End);
            Assert.Equal("L2F1", r.Id);
            Assert.Equal("L2", r.GroupId);

            string html = RenderTextWithRanges(tr.Item1, set);
            Assert.Equal("<p>" +
                "<span>qu</span><span class=\"L0F0 L1F0\">e</span>" +
                "<span class=\"L1F0\"> </span><span class=\"L1F0 L2F0\">v</span>" +
                "<span class=\"L2F0\">ixit</p>\n" +
                "<p>annos</span><span> </span><span class=\"L2F1\">XX</span></p>",
                html);
        }
    }
}
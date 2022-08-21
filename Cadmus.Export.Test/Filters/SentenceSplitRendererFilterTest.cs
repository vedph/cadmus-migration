﻿using Cadmus.Export.Filters;
using Xunit;

namespace Cadmus.Export.Test.Filters
{
    public sealed class SentenceSplitRendererFilterTest
    {
        private static SentenceSplitRendererFilter GetFilter(bool trimming = false)
        {
            SentenceSplitRendererFilter filter = new();
            filter.Configure(new SentenceSplitRendererFilterOptions
            {
                EndMarkers = ".?!\u037e\u2026",
                NewLine = "\n",
                Trimming = trimming
            });
            return filter;
        }

        [Fact]
        public void Apply_NoMarker_AppendedNL()
        {
            SentenceSplitRendererFilter filter = GetFilter();

            string result = filter.Apply("Hello, world");

            Assert.Equal("Hello, world\n", result);
        }

        [Fact]
        public void Apply_Markers_Split()
        {
            SentenceSplitRendererFilter filter = GetFilter();

            string result = filter.Apply("Hello! I am world.");

            Assert.Equal("Hello!\n I am world.\n", result);
        }

        [Fact]
        public void Apply_MarkersWithTrim_Split()
        {
            SentenceSplitRendererFilter filter = GetFilter(true);

            string result = filter.Apply("Hello! I am world.");

            Assert.Equal("Hello!\nI am world.\n", result);
        }

        [Fact]
        public void Apply_MarkersWithCrLf_Split()
        {
            SentenceSplitRendererFilter filter = GetFilter();

            string result = filter.Apply("Hello! I\r\nam world.");

            Assert.Equal("Hello!\n I am world.\n", result);
        }

        [Fact]
        public void Apply_MarkersWithCrLfTrim_Split()
        {
            SentenceSplitRendererFilter filter = GetFilter(true);

            string result = filter.Apply("Hello! I\r\nam world.");

            Assert.Equal("Hello!\nI am world.\n", result);
        }
    }
}

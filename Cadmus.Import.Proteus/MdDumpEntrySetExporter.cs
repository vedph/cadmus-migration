﻿using Fusi.Tools.Configuration;
using Proteus.Core;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Cadmus.Import.Proteus;

/// <summary>
/// Markdown dump entry set exporter. This exporter dumps the entry set
/// into a set of output files.
/// <para>Tag: <c>entry-set-exporter.cadmus.md-dump</c>.</para>
/// </summary>
[Tag("entry-set-exporter.cadmus.md-dump")]
public sealed class MdDumpEntrySetExporter : IEntrySetExporter,
    IConfigurable<MdDumpEntrySetExporterOptions>
{
    private readonly JsonSerializerOptions _jsonOptions;
    private int _fileNr;
    private int _setCount;
    private MdDumpEntrySetExporterOptions _options;
    private TextWriter? _writer;
    private DecodedEntryDumper? _dumper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MdDumpEntrySetExporter"/>
    /// class.
    /// </summary>
    public MdDumpEntrySetExporter()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        _options = new();
    }

    /// <summary>
    /// Configures this exporter with the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">options</exception>
    public void Configure(MdDumpEntrySetExporterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.OutputDirectory = EnvarResolver.ResolveTemplate(
            _options.OutputDirectory ?? "")!;
    }

    /// <summary>
    /// Opens the exporter output. Call this once from outside the pipeline,
    /// when you want to start exporting. This will create the first output file.
    /// </summary>
    public Task OpenAsync()
    {
        _fileNr = 1;
        _setCount = 0;

        if (!string.IsNullOrEmpty(_options.OutputDirectory) &&
            !Directory.Exists(_options.OutputDirectory))
        {
            Directory.CreateDirectory(_options.OutputDirectory);
        }
        string path = Path.Combine(_options.OutputDirectory ?? "", "d00001.md");

        _writer = new StreamWriter(path, false, Encoding.UTF8);
        _dumper = new DecodedEntryDumper(new CsvDecodedEntryDataWriter(
            _writer, _options));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Closes the exporter output. Call this once from outside the pipeline,
    /// when you want to end exporting. This will close the output file.
    /// </summary>
    public Task CloseAsync()
    {
        _writer?.Flush();
        _dumper?.Dispose();
        _dumper = null;
        return Task.CompletedTask;
    }

    private void DumpItems(CadmusEntrySetContext context)
    {
        if (context.Items.Count == 0) return;

        _writer!.WriteLine($"### {context.Number} - Items");
        int n = 0;
        foreach (ImportedItem item in context.Items)
        {
            _writer.WriteLine();
            _writer.WriteLine($"#### {++n}: {item.Title}");
            _writer.WriteLine();
            _writer.WriteLine($"- ID: `{item.Id}`");
            _writer.WriteLine($"- description: `{item.Description}`");
            _writer.WriteLine($"- facet ID: `{item.FacetId}`");
            if (!string.IsNullOrEmpty(item.GroupId))
                _writer.WriteLine($"- group ID: `{item.GroupId}`");
            _writer.WriteLine($"- sort key: `{item.SortKey}`");
            _writer.WriteLine($"- created: `{item.TimeCreated}`");
            _writer.WriteLine($"- creator: `{item.CreatorId}`");
            _writer.WriteLine($"- modified: `{item.TimeModified}`");
            _writer.WriteLine($"- user: `{item.UserId}`");
            if (item.Flags != 0)
                _writer.WriteLine($"- flags: `{item.Flags:X4}`");
            if (item.Parts.Count > 0)
            {
                _writer.WriteLine($"- parts: {item.Parts.Count}");
                _writer.WriteLine();

                int pn = 0;
                foreach (JsonNode node in item.Parts)
                {
                    _writer.WriteLine($"- part __{++pn}__ / {item.Parts.Count}:");
                    _writer.WriteLine();
                    _writer.WriteLine("```json");
                    _writer.WriteLine(node.ToJsonString(_jsonOptions));
                    _writer.WriteLine("```");
                    _writer.WriteLine();
                }
            }
        }
    }

    /// <summary>
    /// Exports the specified entry set.
    /// </summary>
    /// <param name="entrySet">The entry set.</param>
    /// <param name="regionSet">The entry regions set.</param>
    public async Task ExportAsync(EntrySet entrySet, EntryRegionSet regionSet)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(regionSet);

        // create a new file if required
        if (_options.MaxEntriesPerDumpFile > 0 &&
            ++_setCount > _options.MaxEntriesPerDumpFile)
        {
            _setCount = 1;
            string path = Path.Combine(_options.OutputDirectory ?? "",
                $"d{++_fileNr:00000}.md");
            await CloseAsync();
            _writer = new StreamWriter(path, false, Encoding.UTF8);
            _dumper = new DecodedEntryDumper(new CsvDecodedEntryDataWriter(
                _writer, _options));
        }

        // dump the set
        CadmusEntrySetContext context = (CadmusEntrySetContext)entrySet.Context;
        _writer!.WriteLine(new string('-', 60));
        _writer.WriteLine("## {0}", context.Number);
        _writer.WriteLine();

        // data
        if (context.Data.Count > 0)
        {
            _writer.WriteLine($"### {context.Number} - Data");
            _writer.WriteLine();
            foreach (string key in context.Data.Keys.OrderBy(s => s))
            {
                _writer.Write(key[0] == '*' ? "- \\" : "- ");
                _writer.WriteLine($"{key}=`{context.Data[key]}`");
            }

            _writer.WriteLine();
        }

        // items with their parts
        DumpItems(context);

        // entries
        _writer.WriteLine($"### {context.Number} - Entries");
        _writer.WriteLine();
        _writer.WriteLine("```tsv");
        _dumper!.Dump(context.Number, entrySet.Entries, regionSet.Regions);
        _writer.WriteLine("```");

        _writer.WriteLine();
    }
}

/// <summary>
/// Options for <see cref="MdDumpEntrySetExporter"/>.
/// </summary>
public class MdDumpEntrySetExporterOptions : DecodedEntryDataWriterOptions
{
    /// <summary>
    /// Gets or sets the output directory. This can include environment
    /// variables in double braces (e.g.
    /// <c>{{HOMEDRIVE}}{{HOMEPATH}}\Desktop\out\</c>).
    /// </summary>
    public string OutputDirectory { get; set; } = "";

    /// <summary>
    /// Gets or sets the maximum number of sets per file.
    /// </summary>
    public int MaxEntriesPerDumpFile { get; set; } = 100;
}
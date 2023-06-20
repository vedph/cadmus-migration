﻿using Cadmus.Core.Config;
using NPOI.SS.UserModel;
using System;
using System.IO;

namespace Cadmus.Import.Excel;

/// <summary>
/// Excel (XLS or XLSX) thesaurus reader.
/// </summary>
/// <seealso cref="IThesaurusReader" />
public sealed class ExcelThesaurusReader : IThesaurusReader
{
    private readonly IWorkbook _workbook;
    private readonly ExcelThesaurusReaderOptions _options;
    private bool _disposed;
    private int _rowIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelThesaurusReader"/>
    /// class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <exception cref="ArgumentNullException">stream</exception>
    public ExcelThesaurusReader(Stream stream)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        _workbook = WorkbookFactory.Create(stream, true);
        _options = new();
        _rowIndex = -1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelThesaurusReader"/>
    /// class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">stream or options</exception>
    public ExcelThesaurusReader(Stream stream,
        ExcelThesaurusReaderOptions options)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        _workbook = WorkbookFactory.Create(stream, true);
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _rowIndex = -1;
    }

    /// <summary>
    /// Read the next thesaurus entry from source.
    /// </summary>
    /// <returns>Thesaurus, or null if no more thesauri in source.</returns>
    /// <exception cref="InvalidOperationException">
    /// Expected thesaurus ID, Expected thesaurus entry ID.
    /// </exception>
    public Thesaurus? Next()
    {
        ISheet sheet = _workbook.GetSheetAt(_options.SheetIndex);

        // skip top N rows if requested
        if (_rowIndex == -1 && _options.RowOffset > 0)
            _rowIndex = _options.RowOffset;

        Thesaurus? thesaurus = null;
        string? thesaurusId = null;

        while (_rowIndex < sheet.LastRowNum)
        {
            // read next row
            IRow? row = sheet.GetRow(_rowIndex);
            if (row == null) break;

            // create thesaurus if this is the first row read
            if (thesaurus == null)
            {
                thesaurusId = row.GetCell(_options.ColumnOffset)?.StringCellValue;
                if (thesaurusId == null)
                {
                    throw new InvalidOperationException("Expected thesaurus ID " +
                        $"at {_rowIndex + 1},{_options.ColumnOffset}");
                }
                thesaurus = new Thesaurus(thesaurusId);
            }

            // read entries:
            // 0=thesaurus ID: stop if another thesaurus begins
            int i = _options.ColumnOffset;
            if (string.IsNullOrEmpty(thesaurusId)) thesaurusId = thesaurus.Id;
            else if (thesaurus.Id != thesaurusId) break;

            // 1=id
            string? id = row.GetCell(i++)?.StringCellValue;

            // 2=value
            string? val = row.GetCell(i++)?.StringCellValue;

            // 3=target id
            string? targetId = row.GetCell(i++)?.StringCellValue;
            if (targetId != null)
            {
                thesaurus.TargetId = targetId;
            }
            else
            {
                if (id == null)
                {
                    throw new InvalidOperationException(
                        $"Expected thesaurus {thesaurus.Id} entry ID " +
                        $"at {_rowIndex + 1},{i + 1}");
                }
                thesaurus.AddEntry(new ThesaurusEntry
                {
                    Id = id, Value = val ?? ""
                });
            }
            _rowIndex++;
        }

        return thesaurus;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _workbook.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Options for <see cref="ExcelThesaurusReader"/>.
/// </summary>
public class ExcelThesaurusReaderOptions
{
    /// <summary>
    /// Gets or sets the index of the sheet.
    /// </summary>
    public int SheetIndex { get; set; }

    /// <summary>
    /// Gets or sets the row offset.
    /// </summary>
    public int RowOffset { get; set; }

    /// <summary>
    /// Gets or sets the column offset.
    /// </summary>
    public int ColumnOffset { get; set; }
}

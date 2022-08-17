# Cadmus Migration

>This is work in progress.

Tools for migrating (importing/exporting) Cadmus data. Export tools also include "preview", i.e. a human-friendly, highly customizable output for each Cadmus object, to be integrated in the editor itself.

- [Documentation](docs/index.md)

## Cadmus.Export

General purpose components used to export Cadmus data.

## Cadmus.Export.ML

Markup related components used to export Cadmus data into some markup language, typically XML.

## History

- 2022-08-17: more conceptual documentation.

### 0.0.8

- 2022-08-15: essential TEI renderer components.
- 2022-08-14:
  - adding `IItemComposer`.
  - `BuildTextBlocks` added assigned layer IDs.

### 0.0.6

- 2022-08-11:
  - refactoring JSON renderers for configurable filters.
  - added thesaurus renderer filters.

### 0.0.5

- 2022-08-08: added Markdown support.

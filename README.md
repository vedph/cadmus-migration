# Cadmus Migration

Tools for migrating (importing/exporting) Cadmus data. Export tools also include "preview", i.e. a human-friendly, highly customizable output for each Cadmus object, to be integrated in the editor itself.

- [Documentation](docs/index.md)

## Cadmus.Export

General purpose components used to export Cadmus data.

## Cadmus.Export.ML

Markup related components used to export Cadmus data into some markup language, typically XML.

## History

- 2022-10-07: added array wrapping to XSLT JSON renderer.
- 2022-10-06:
  - fixes to TEI standoff item composer.
  - minor refactorings to renderer context.

### 0.0.15

- 2022-10-04: moved repository dependency out of database-dependent filters. The repository is now a property of the renderer context.

### 0.0.14

- 2022-10-04: added item ID to previewer and context to its filters.

### 0.0.13

- 2022-10-02: added `TextTemplate` to `MongoTokenExtractorRendererFilter`.

### 0.0.12

- 2022-10-01:
  - added `MongoTokenExtractorRendererFilter`.
  - more methods in `CadmusPreviewer`.
- 2022-08-26: completing `TeiStandoffApparatusJsonRenderer`.

### 0.0.11

- 2022-08-22: refactored sentence splitter.

### 0.0.10

- 2022-08-22: added flags matching.

### 0.0.9

- 2022-08-21: added sentence splitter filter (used to extract Sidonius Apollinaris from its [Cadmus project](https://github.com/vedph/cadmus-sidon-app)).
- 2022-08-19: adding CLI infrastructure and refactored item composer API.
- 2022-08-19: item ID collectors.
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

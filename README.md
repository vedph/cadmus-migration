# Cadmus Migration

Tools for migrating (importing/exporting) Cadmus data. Export tools also include "preview", i.e. a human-friendly, highly customizable output for each Cadmus object, to be integrated in the editor itself.

- [Documentation](docs/index.md)

👀 [Cadmus Page](https://myrmex.github.io/overview/cadmus/)

## Cadmus.Export

General purpose components used to export Cadmus data.

## Cadmus.Export.ML

Markup related components used to export Cadmus data into some markup language, typically XML.

## History

### 3.0.3

- 2023-06-21: completed Excel thesaurus reader.
- 2023-06-20:
  - added CSV thesaurus reader.
  - adding XLS(X) thesaurus reader.
- 2023-06-19: more tests.
- 2023-06-18: adding import.

### 3.0.2

- 2023-06-16: updated packages.

### 3.0.1

- 2023-06-02: updated packages.

### 3.0.0

- 2023-05-23: updated packages (breaking change in general parts introducing [AssertedCompositeId](https://github.com/vedph/cadmus-bricks-shell/blob/master/projects/myrmidon/cadmus-refs-asserted-ids/README.md#asserted-composite-id)).

### 2.0.4

- 2023-05-12: updated packages.

### 2.0.3

- 2023-02-05: updated packages.

### 2.0.2

- 2023-02-01: migrated to new components factory. This is a breaking change for backend components, please see [this page](https://myrmex.github.io/overview/cadmus/dev/history/#2023-02-01---backend-infrastructure-upgrade). Anyway, in the end you just have to update your libraries and a single namespace reference. Benefits include:
  - more streamlined component instantiation.
  - more functionality in components factory, including DI.
  - dropped third party dependencies.
  - adopted standard MS technologies for DI.

### 1.0.0

- 2022-11-10: upgraded to NET 7.

### 0.1.1

- 2022-11-04: updated packages.

### 0.1.0

- 2022-11-04: updated packages (nullability enabled in Cadmus core).

### 0.0.21

- 2022-11-03: updated packages.

### 0.0.20

- 2022-10-24: refactored block ID for TEI standoff export (`Cadmus.Export.ML`).
- 2022-10-14: fixes to render items command.

### 0.0.19

- 2022-10-11: updated Cadmus packages.

### 0.0.18

- 2022-10-10:
  - use XSLT script settings in XSLT JSON renderer.
  - updated packages.

### 0.0.17

- 2022-10-08: fix to array wrapper in XSLT JSON renderer.

### 0.0.16

- 2022-10-08: added ISO639 filter.
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

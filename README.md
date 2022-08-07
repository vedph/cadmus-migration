# Cadmus Migration

>This is experimental work in progress.

Tools for migrating (importing/exporting) Cadmus data.

## Cadmus.Export

General purpose components used to export Cadmus data.

- `IJsonRenderer` is the interface implemented by part/fragment renderers.
- `ITextPartFlattener` is the interface used to flatten a layered text into a set of text blocks. Each text block is a span of text with a number of layer IDs, linking that text with its annotations from layers. This is also the input model for a related HTML visualization leveraging [this brick](https://github.com/vedph/cadmus-bricks-shell/tree/master/projects/myrmidon/cadmus-text-block-view). Once the text and a number of selected layers get flattened, blocks are built using `TextBlockBuilder`.
- `ITextBlockRenderer` is used to render rows of text blocks built by `TextBlockBuilder`.

The `CadmusPreviewer` is a high level class leveraging most of these components to provide configurable rendition of parts and fragments, with a special treatment reserved to layered text. It relies on a JSON configuration consumed by `CadmusPreviewerFactory`, having this form:

```json
{
	"JsonRenderers": [],
	"TextBlockRenderers": [],
	"TextPartFlatteners": []
}
```

Each array contains any number of JSON objects having an `Id` property and an optional `Options` object to configure the component.

## Cadmus.Export.ML

Components used to export Cadmus data into some markup language (typically XML).

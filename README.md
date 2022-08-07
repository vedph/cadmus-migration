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
	"TextPartFlatteners": [],
	"TextBlockRenderers": []
}
```

Each array contains any number of JSON objects having an `Id` property and an optional `Options` object to configure the component. Also, here the `Keys` property is used to include the key of the object type being processed: this is equal to the type ID for parts, and to the part type ID + `|` + part role ID (=fragment type) for fragments. This defines a mapping between object types and the IDs of the components configured to process them.

For instance, in this configuration:

```json
{
  "JsonRenderers": [
    {
      "Keys": "it.vedph.token-text",
      "Id": "it.vedph.json-renderer.null"
    },
    {
      "Keys": "it.vedph.token-text-layer|fr.it.vedph.comment",
      "Id": "it.vedph.json-renderer.null"
    },
    {
      "Keys": "it.vedph.token-text-layer|fr.it.vedph.orthography",
      "Id": "it.vedph.json-renderer.null"
    }
  ],
  "TextPartFlatteners": [
    {
      "Keys": "it.vedph.token-text",
      "Id": "it.vedph.text-flattener.token"
    }
  ]
}
```

there is a JSON renderer ID for each object key:

- part `it.vedph.token-text`: its JSON renderer and its text flattener.
- layer part `it.vedph.token-text-layer` for fragment type `fr.it.vedph.comment`: its JSON renderer.
- layer part `it.vedph.token-text-layer` for fragment type `fr.it.vedph.orthography`: its JSON renderer.

The JSON renderer here is just a "null" renderer which passes back the received JSON, for diagnostic purposes; but any other renderer can be used and configured via its `Options` property.

## Cadmus.Export.ML

Components used to export Cadmus data into some markup language (typically XML).

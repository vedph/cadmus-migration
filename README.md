# Cadmus Migration

- [Cadmus Migration](#cadmus-migration)
  - [Cadmus.Export](#cadmusexport)
    - [JSON Rendering](#json-rendering)
  - [Cadmus.Export.ML](#cadmusexportml)
  - [History](#history)
    - [0.0.5](#005)

>This is work in progress.

Tools for migrating (importing/exporting) Cadmus data. Export tools also include "preview", i.e. a human-friendly, highly customizable output for each Cadmus object, to be integrated in the editor itself.

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

### JSON Rendering

Any Cadmus object, either part or fragment, is ultimately archived as JSON. So, JSON is the starting point when rendering the output. Any component implementing `IJsonRenderer` can be registered in the previewer factory and used in the rendering configuration, with all its settings.

In most cases, unless complex logic is required, you can use the `XsltJsonRenderer`, which was designed with some essential requirements in mind:

- it should be fully customizable by users, who are accustomed to XSLT transformations.
- it should provide a powerful way for transforming JSON data even before submitting it to the XSLT processor.
- it should handle Markdown.
- all transformations should be available, even at the same time. In fact, there are 3 transformations and you can pick any combination of them. They are (in processing order):
  1. JSON transform.
  2. XSLT transform.
  3. Markdown conversion. This can affect the whole output, or just some regions of it, provided that they are marked with any chosen open and close tags. Typically you add such tags when transforming the JSON.

To this end we leverage these technologies:

- [JMESPath](https://jmespath.org/tutorial.html), a powerful selection and transformation language for JSON.
- an automatic [conversion](https://www.newtonsoft.com/json/help/html/ConvertingJSONandXML.htm) from JSON to XML.
- XSLT for transforming XML.
- [Markdig](https://github.com/xoofx/markdig) to eventually convert Markdown regions into HTML/plain text.

Even though this implies more processing stages, it represents a highly customizable component where most users will be at home, as XSLT is a popular and easy way of processing XML for output. Yet, eventually the additional processing power of a specific JSON transformation engine can be leveraged to prepare the data before converting them into XSLT. It might also be the case that there is no XSLT, and the output is directly generated by JMESPath transforms: in this case no conversion from JSON to XML will occur.

As a sample, say we have this token-based text part representing the text `que bixit / annos XX`:

```json
{
  "citation": "CIL 1,23",
  "lines": [
    { "y": 1, "text": "que bixit" },
    { "y": 2, "text": "annos XX" }
  ],
  "id": "9a801c84-0c93-4074-b071-9f4f9885ba66",
  "itemId": "item",
  "typeId": "it.vedph.token-text",
  "roleId": "base-text",
  "thesaurusScope": null,
  "timeCreated": "2022-08-07T14:04:01.3995195Z",
  "creatorId": "zeus",
  "timeModified": "2022-08-07T14:04:01.3995195Z",
  "userId": "zeus"
}
```

When handling it in a `XsltJsonRenderer` configured for a single XSLT-based transformation, first the JSON code is wrapped in a `root` element to ensure it is well-formed for XML conversion, whence:

```json
{
  "root": {
    "citation": "CIL 1,23",
    "lines": [
      { "y": 1, "text": "que bixit" },
      { "y": 2, "text": "annos XX" }
    ],
    "id": "9a801c84-0c93-4074-b071-9f4f9885ba66",
    "itemId": "item",
    "typeId": "it.vedph.token-text",
    "roleId": "base-text",
    "thesaurusScope": null,
    "timeCreated": "2022-08-07T14:04:01.3995195Z",
    "creatorId": "zeus",
    "timeModified": "2022-08-07T14:04:01.3995195Z",
    "userId": "zeus"
  }
}
```

Then, JSON is converted into XML:

```xml
<root><citation>CIL 1,23</citation><lines><y>1</y><text>que bixit</text></lines><lines><y>2</y><text>annos XX</text></lines><id>9a801c84-0c93-4074-b071-9f4f9885ba66</id><itemId>item</itemId><typeId>it.vedph.token-text</typeId><roleId>base-text</roleId><thesaurusScope /><timeCreated>2022-08-07T14:12:44.8640749Z</timeCreated><creatorId>zeus</creatorId><timeModified>2022-08-07T14:12:44.8640749Z</timeModified><userId>zeus</userId></root>
```

At this stage, the XSLT transformation occurs. In this example, it's a simple transform to produce a plain text output:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:tei="http://www.tei-c.org/ns/1.0" version="1.0">
  <xsl:output method="text" encoding="UTF-8"/>
  <xsl:strip-space elements="*" />

  <xsl:template match="citation">[<xsl:value-of select="."/>]<xsl:text xml:space="preserve">&#xA;</xsl:text>
</xsl:template>

  <xsl:template match="lines">
    <xsl:value-of select="y"/>
    <xsl:text xml:space="preserve">  </xsl:text>
    <xsl:value-of select="text"/>
    <xsl:text xml:space="preserve">&#xA;</xsl:text>
  </xsl:template>

  <xsl:template match="root">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="*"></xsl:template>
</xsl:stylesheet>
```

So, the final output is:

```txt
[CIL 1,23]
1  que bixit
2  annos XX
```

## Cadmus.Export.ML

Components used to export Cadmus data into some markup language (typically XML). These rely on the general purpose export components, and add logic targeted to build a full-fledged output for third parties (e.g. TEI).

## History

### 0.0.5

- 2022-08-08: added Markdown support.

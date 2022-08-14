# Cadmus Migration

- [Cadmus Migration](#cadmus-migration)
  - [Cadmus.Export](#cadmusexport)
    - [JSON Rendering](#json-rendering)
    - [Renderer Filters](#renderer-filters)
      - [Thesaurus Lookup Filter](#thesaurus-lookup-filter)
      - [Markdown Conversion Filter](#markdown-conversion-filter)
      - [Text Replacements Filter](#text-replacements-filter)
  - [Cadmus.Export.ML](#cadmusexportml)
  - [History](#history)
    - [0.0.6](#006)
    - [0.0.5](#005)

>This is work in progress.

Tools for migrating (importing/exporting) Cadmus data. Export tools also include "preview", i.e. a human-friendly, highly customizable output for each Cadmus object, to be integrated in the editor itself.

## Cadmus.Export

General purpose components used to export Cadmus data.

There are essentially two types of preview:

- _generic preview_ for any part. This uses an implementation of `IJsonRenderer`, which gets a JSON input and transforms it into any output encoded as a string (usually HTML). This can be applied to any part or fragment.

- _specialized preview_ for layered text parts. This uses an implementation of `ITextBlockRenderer`, and is specifically designed to produce an interactive output from a base text part plus any number of its layer parts. In turn, this renderer uses an `ITextPartFlattener` implementation to flatten a layered text into a set of text blocks. Each text block is a span of text with a number of layer IDs, linking that text with its annotations from layers. This is also the input model for a related HTML visualization leveraging [this brick](https://github.com/vedph/cadmus-bricks-shell/tree/master/projects/myrmidon/cadmus-text-block-view). Once the text and a number of selected layers get flattened, blocks are built using `TextBlockBuilder`. The resulting blocks are an abstraction which can be easily leveraged in an interactive UI.

A high level class embracing all these preview types is represented by `CadmusPreviewer`. It relies on a JSON configuration consumed by `CadmusPreviewerFactory`, having these sections, all modeled as arrays of objects:

- `RendererFilters`: a set of configured renderer filters, each grouped under an arbitrary key. These filters can then be referenced by their key from other sections. Such filters are executed in the order they are specified for each renderer where they are specified, just after its rendition completes. Some prebuilt filters allow you to lookup thesauri (resolving their IDs into values), convert Markdown text into HTML or plain text, and perform text replacements (either based on literals, and on regular expressions).
- `JsonRenderers`: a set of JSON renderers, each grouped under an arbitrary key. The key corresponds to the part type ID, eventually followed by `|` and its role ID in the case of a layer part. This allows mapping each part type to a specific renderer ID.
- `TextPartFlatteners`: a set of text part flatteners, each grouped under an arbitrary key as above for `JsonRenderers`. These are in charge of flattening the text of a base text part, which is a stage in building its block-based representation.
- `TextBlockRenderers`: a set of text block renderers, used to produce some text output starting from text blocks. These can be used when exporting a layered text into some other format, e.g. XML TEI.

Each array in the configuration contains any number of JSON objects having:

- an `Id` property.
- an optional `Options` object to configure the component. All the `JsonRenderers` can have a `FilterKeys` array property, specifying the filters to apply after its rendition. Each entry in the array is one of the filters keys as defined in the `RendererFilters` section.
- a `Keys` property is used to include the key of the object type being processed.

As a sample, consider this configuration:

- 3 renderer filters are defined with their keys: `thes-filter`, `rep-filter`, `md-filter`.
- 3 renderers are defined for 3 different part types (a base text part, and two layer parts). The JSON renderer here is just a "null" renderer which passes back the received JSON, for diagnostic purposes; but any other renderer can be used and configured via its `Options` property.
- 1 text part flattener is defined for the token-based text part type.
- 1 text block renderer is defined to produce simple TEI from a layered text.

```json
{
  "RendererFilters": [
    {
      "Keys": "thes-filter",
      "Id": "it.vedph.renderer-filter.mongo-thesaurus"
    },
    {
      "Keys": "rep-filter",
      "Id": "it.vedph.renderer-filter.replace",
      "Options": {
        "Replacements": [
          {
            "Source": "hello",
            "Target": "HELLO"
          }
        ]
      }
    },
    {
      "Keys": "md-filter",
      "Id": "it.vedph.renderer-filter.markdown",
      "Options": {
        "MarkdownOpen": "<_md>",
        "MarkdownClose": "</_md>",
        "Format": "txt"
      }
    }
  ],
  "JsonRenderers": [
    {
      "Keys": "it.vedph.token-text",
      "Id": "it.vedph.json-renderer.null",
      "Options": {
        "FilterKeys": [
          "thes-filter",
          "rep-filter",
          "md-filter"
        ]
      }
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
  ],
  "TextBlockRenderers": [
    {
      "Id": "it.vedph.text-block-renderer.simple-tei",
      "Options": {
        "RowElement": "div",
        "BlockElement": "seg"
      }
    }
  ]
}
```

### JSON Rendering

Any Cadmus object, either part or fragment, is ultimately archived as JSON. So, JSON is the starting point when rendering the output. Any component implementing `IJsonRenderer` can be registered in the previewer factory and used in the rendering configuration, with all its settings.

In most cases, unless complex logic is required, you can use the `XsltJsonRenderer`, which was designed with some essential requirements in mind:

- it should be fully customizable by users, who are accustomed to XSLT transformations. We must then adapt our JSON data to XML, so that it can be processed via XSLT.
- it should provide a powerful way for transforming JSON data even before submitting it to the XSLT processor. This refers to a true JSON transform, rather than a raw string-based transform, just like XSLT implies a DOM rather than just working on a sequence of characters.
- both the JSON and the XML transformation should be available in any combination: JSON only, XML only, JSON + XML.
- it should be able to convert Markdown text.
- it should be able to lookup thesauri.

To this end we leverage these technologies:

- [JMESPath](https://jmespath.org/tutorial.html), a powerful selection and transformation language for JSON.
- an automatic [conversion](https://www.newtonsoft.com/json/help/html/ConvertingJSONandXML.htm) from JSON to XML.
- XSLT for transforming XML.
- [Markdig](https://github.com/xoofx/markdig) to eventually convert Markdown regions into HTML/plain text.

Thesaurus lookup and Markdown conversion are provided via filters.

Even though this implies more processing stages, it represents a highly customizable component where most users will be at home, as XSLT is a popular and easy way of processing XML for output. Yet, eventually the additional processing power of a specific JSON transformation engine can be leveraged to prepare the data before converting them into XSLT. It might also be the case that there is no XSLT, and the output is directly generated by JMESPath transforms: in this case no conversion from JSON to XML will occur.

Also, more specialized processing is available via filters, which can extend any transformation.

As a sample, say we have this token-based text part, representing the text `que bixit / annos XX`:

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

### Renderer Filters

This component provides some builtin filters, but like for any other component type you are free to add your own.

#### Thesaurus Lookup Filter

Lookup any thesaurus entry by its ID, replacing it with its value when found, or with the entry ID when not found.

- ID: `it.vedph.renderer-filter.mongo-thesaurus`
- options:
  - `ConnectionString`: connection string to the Mongo DB. This is usually omitted and supplied by the client code from its own application settings.
  - `Pattern`: the regular expression pattern representing a thesaurus ID to lookup: it is assumed that this expression has two named captures, `t` for the thesaurus ID, and `e` for its entry ID. The default pattern is a `$` followed by the thesaurus ID, `:`, and the entry ID.

#### Markdown Conversion Filter

Convert Markdown text into HTML or plain text.

- ID: `it.vedph.renderer-filter.markdown`
- options:
  - `MarkdownOpen`: the markdown region opening tag. When not set, it is assumed that the whole text is Markdown.
  - `MarkdownClose`: the markdown region closing tag. When not set, it is assumed that the whole text is Markdown.
  - `Format`: the Markdown regions target format: if not specified, nothing is done; if `txt`, any Markdown region is converted into plain text; if `html`, any Markdown region is converted into HTML.

#### Text Replacements Filter

Perform any text replacements, either literals or based on regular expressions.

- ID: `it.vedph.renderer-filter.replace`
- options:
  - `Replacements`: an array of objects with these properties:
    - `Source`: the text or pattern to find.
    - `Target`: the replacement.
    - `Repetitions`: the max repetitions count, or 0 for no limit (=keep replacing until no more changes).
    - `IsPattern`: `true` if Source is a regular expression pattern rather than a literal.

## Cadmus.Export.ML

Components used to export Cadmus data into some markup language (typically XML). These rely on the general purpose export components, and add logic targeted to build a full-fledged output for third parties (e.g. TEI).

## History

- 2022-08-14:
  - adding `IItemComposer`.
  - `BuildTextBlocks` added assigned layer IDs.

### 0.0.6

- 2022-08-11:
  - refactoring JSON renderers for configurable filters.
  - added thesaurus renderer filters.

### 0.0.5

- 2022-08-08: added Markdown support.

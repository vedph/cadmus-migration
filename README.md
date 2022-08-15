# Cadmus Migration

- [Cadmus Migration](#cadmus-migration)
  - [Cadmus.Export](#cadmusexport)
    - [JSON Rendering](#json-rendering)
    - [Renderer Filters](#renderer-filters)
      - [Thesaurus Lookup Filter](#thesaurus-lookup-filter)
      - [Markdown Conversion Filter](#markdown-conversion-filter)
      - [Text Replacements Filter](#text-replacements-filter)
      - [Fragment Link Filter](#fragment-link-filter)
  - [Cadmus.Export.ML](#cadmusexportml)
  - [History](#history)
    - [0.0.8](#008)
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

Additionally, the XSLT renderer can also inject properties into layer part fragments during JSON preprocessing, thus providing layer keys to be later used to link fragments to text blocks.

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

#### Fragment Link Filter

Map layer keys into target IDs, leveraging the metadata built by the text renderer run before this filter. This is used to link fragments renditions to their base text.

- ID: `it.vedph.renderer-filter.fr-link`
- options:
  - `TagOpen`: the opening tag for fragment key.
  - `TagClose`: the closing tag for fragment key.

## Cadmus.Export.ML

Components used to export Cadmus data into some markup language (typically XML). These rely on the general purpose export components, and add logic targeted to build a full-fledged output for third parties (e.g. TEI).

In general, the simplest approach to building TEI from Cadmus objects is via some form of stand-off notation (see e.g. [E. Spadini, M. Turska, XML-TEI Stand-off Markup: One Step Beyond, Digital Philology 2019 225-239](https://serval.unil.ch/resource/serval:BIB_F671BA825955.P001/REF.pdf)). In fact, stand-off practically is the unique solution to solve serious overlap issues in TEI, typically arising when a lot of different structures are to be encoded in the same text.

In stand-off, annotations are no more embedded in the text, but rather stand side by side to it, and refer to it in various ways. The simplest and probably most popular linking method is just wrapping each portion of text to be annotated with an identifier. For instance, you can just wrap each word in the text in a `seg` element with some `xml:id`, and then refer to it from annotations.

Thus, stand-off implies that you have separated "layers" for:

- the text.
- every layer of annotations referring to the text.

This structure is most compatible with the Cadmus architecture, which is object-based. There, a text is just a set of text items, each representing a portion of it (just like several `div`'s encode the portions of a text in TEI). Every text item has a text part, with the text only, and any number of layer parts, each with its own model. Layer parts are linked to the text part via a coordinates system, so that, differently from TEI, adding layers does not require touching the text part in any way. Of course, this loose coupling is part of the Cadmus strategy for isolating and reusing content.

So, in Cadmus we start from a set of text items, having one text part and many text layer parts. In TEI this gets translated into a TEI document with the text only, and one TEI document for each layer part. These "layer" documents are linked to the text document via IDs annotated in the text document itself.

In this approach, we are not required to split the text document into a predefined level of granularity. This is usually required in TEI stand-off, as you have to provide a text before annotating it. For instance, if you are going to annotate graphical words, and nothing below this level, you will just mechanically wrap each sequence of characters delimited by whitespace into a `seg` or other similar element, assigning to each a unique ID, like a progressive number, like this (Catull. 2,1):

```xml
<l><w xml:id="w1">passer</w>, <w xml:id="w2">deliciae</w> <w xml:id="w3">meae</w> <w xml:id="w4">puellae</w></l>
```

This is of course redundant, as not all the `w` elements (and their IDs) will be effectively used as target of linked annotations; but you need to systematically wrap all the words, as you can't know in advance which words will be annotated.

Also, and more important, this limits the annotation to the chosen granularity level (here the graphical word). Should you want to annotate a single syllable, or a single letter, you would be in trouble.

In Cadmus instead TEI is just one of the many outputs which can be generated from the objects in the database. So, when generating it we produce a sort of "snapshot", where we know in advance which portion of text will get which annotations; we can thus wrap portions of text of variable granularity, without having to stick to a predefined unit. Just like in Cadmus you annotate text by selecting any portion of it, from a single character to several lines, in TEI we will wrap different spans of text corresponding to such selections.

Of course, the main practical issue here is provided by the fact that Cadmus is has multi-layer architecture, where several annotations freely overlap at variable granularity levels. It may well happen that one layer selects a single character of a word, while another selects three words including that same word. We thus need to "flatten" these layers into a single sequence of characters, corresponding to the base text to be annotated.

The key to this flattening process is merging all the layers selections into one. This is done by a specialized component, which produces a model based on "text blocks". A text block is an arbitrary span of text, linked to any number of annotation layers.

For instance, say you have this line of text, where different portions of it are annotated at different levels (each represented by a letter):

```txt
que bixit annos XX
..O...............
....O.............
..PPP.............
....CCCCCCCCCCC...
```

Here, `O` represents an orthographic annotation (`que` for `quae`; `bixit` for `vixit`), which happens at the maximum level of granularity (the character); `P` represents a paleographic annotation (e.g. a graphical connection between `E` and `B`); and `C` a generic comment annotation (`bixit annos` to note the usage of accusative rather than the more usual ablative). As you can see, each annotation has its own extent: the two orthographic annotations rest on single letters (`e`, `b`); the paleographic annotation links two letters (final `e` and initial `b`), including the space between them; and the comment annotation spans for a couple of words. Most of these spans overlap, but this is not an issue as each rests on its own layer.

Now, when flattening these layers into a single sequence, the text gets split into blocks defined as the maximum extent of characters linked to exactly the same layers. So, the resulting blocks will be:

- `qu` linked to none.
- `e` linked to `O`, `P`.
- space linked to `P`.
- `b` linked to `O`, `P`, `C`.
- `ixit annos` linked to `C`.
- space and `XX` linked to none.

Thus, the text segmentation is `qu|e| |b|ixit annos| XX`, defining 6 blocks, variously linked to different layers. These blocks, grouped into rows, are the model output by text part flattener components (`ITextPartFlattener`). The block model is shared among all the flatteners, but different flatteners are required to fit different types of parts representing text. As you may recall, in Cadmus there is no single text part; currently there are two, one for token-based texts, and another for tile-based texts; but, as for any other part, types are open. So, for each text part type you can have a corresponding flattener.

By convention, the links to each fragment in its layer part (the block's layer IDs) is encoded with the part layer type ID, followed by `|` and its role ID, followed by the fragment's index in the layer part's fragments array. This is enough to uniquely identify each layer part's fragment in the context of an item. As data is processed item by item, this fits the renderers architecture. For instance, consider this layer ID:

```txt
it.vedph.token-text-layer|fr.it.vedph.comment0
```

this identifies:

- the layer part with type ID = `it.vedph.token-text-layer` and role ID = `fr.it.vedph.comment`.
- the fragment with index = 0, i.e. the first fragment in the layer part.

Such identifiers are unique in the context of each item; but when rendering a TEI document, we must ensure that identifiers are globally unique in the context of all the generated TEI documents. To this end, the block layer IDs are mapped into target IDs built when rendering the text. These target IDs are built by concatenating three numbers: the item's ordinal number, the blocks row ordinal number, and the fragment's index, all separated by underscores (e.g. `1_2_0`).

When rendering the text, the renderer stores the mappings between each layer ID and its corresponding target ID (e.g. `it.vedph.token-text-layer|fr.it.vedph.comment0` => `1_2_0`). Once the text part is rendered, we have all the mappings in place; the layer parts are then rendered via a set of JSON renderers.

At this stage, even before processing the input JSON code, a special filtering feature of JSON renderers injects a `_key` property into each object representing a fragment in the layer part. The value of this key is represented by the layer ID for the fragment, like `it.vedph.token-text-layer|fr.it.vedph.comment0`. Once each fragment has this property, the JSON renderer can include it in its output, e.g. as an attribute of an XML element (XML being converted from JSON). Then, the same renderer transforms this XML via XSLT, and eventually applies a number of filters on its output.

Among these filters, a special one can leverage the mappings produced by the text renderer to convert each layer ID into a target ID, i.e. the ID of the text block the fragment refers to. This effectively links each fragment on each layer to the corresponding block of text.

All this processing happens in the context of an item composer component (`IItemComposer` interface), which handles one item at a time, orchestrating the rendition of its parts. Like all the other components, these components are configured specifying their text flattener and renderer, and their layer parts renderers. A file-based version of these components can then write into parallel streams the different flows of TEI code produced by the renderers: one for text, and one for each of the layers.

## History

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

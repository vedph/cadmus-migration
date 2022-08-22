# Overview

- [Overview](#overview)
  - [Generic Preview](#generic-preview)
  - [Specialized Preview](#specialized-preview)
  - [Higher Level Components](#higher-level-components)
  - [JSON Rendering and Other Techs](#json-rendering-and-other-techs)

The main components of the Cadmus preview architecture are summarized in Figure 1:

![overview](img/cadmus-export.png)

- _Figure 1: Cadmus preview architecture_

It all starts from the Cadmus **database**, including items with their parts. Some of these parts may represent text (with a text part) or layered text (with a text part and any number of text layer parts). Many other parts may well represent non-textual data (e.g. the codicological description of a manuscript). Usually, items are processed from an **item ID collector**, which gets the IDs of all the matching items in their order (at present we just have a single [builtin collector](collectors.md)).

As for preview, the main distinction is between a **generic preview**, which can be applied to any part; and a **specialized preview** specifically designed for layered texts.

## Generic Preview

The _generic_ preview relies on some **JSON renderer** component. A JSON renderer component is just a component which takes JSON (representing any Cadmus data object: part, fragment, or item), and renders it into some text-based format, like HTML, XML, etc.

In Figure 1, you can see that a JSON renderer picked from a set of available renderers can be used to produce some text-based output from a Cadmus part, whatever its type.

To this end, the JSON renderer or the text block renderer may also use a set of **renderer filters**. Such filters are executed in the order they are defined for each renderer, just after its rendition completes. Each filter has a specific task, often general enough to be reused in other renderers.

For instance, some [prebuilt filters](filters.md) allow you to lookup thesauri (resolving their IDs into values), convert Markdown text into HTML or plain text, perform text replacements (either based on literals, and on regular expressions), or resolve the mapping between layer IDs and target IDs in text.

## Specialized Preview

The _specialized preview_ is for layered text parts.

As you can see from Figure 1, this preview uses a number of components:

- some **text part flattener** (`ITextPartFlattener`) is used to flatten a layered text (i.e. one text part plus any number of text layer parts) into a set of _text blocks_. Each text block is a span of text with a number of layer IDs, linking that text with its annotations from layers. This is also the input model for a related HTML visualization leveraging [this brick](https://github.com/vedph/cadmus-bricks-shell/tree/master/projects/myrmidon/cadmus-text-block-view). Once the text and its layers are flattened, blocks are built using logic shared among all the flatteners (implemented in `TextBlockBuilder`). So, while the flattening logic varies according to the type of the text part (e.g. `TokenTextPart` or `TiledTextPart`), the block building logic stays the same. The resulting blocks are an abstraction, which can be easily leveraged in an interactive UI, as well as represent the source for building some other output like TEI.

- some **text block renderer** can be used to generate a text-based output from these blocks. For instance, you can use a TEI block renderer to build TEI from text blocks.

## Higher Level Components

Higher level components can be used to orchestrate the tasks performed by these various components.

The **item composer** is a configurable component designed to compose the renditions from the parts belonging to a single item. Here you are free to add your own composers with any complex logic. The output of a composer is not limited in any way; for instance, the builtin TEI composer writes a set of TEI documents with stand-off notation.

The **Cadmus previewer** is the surface component used by Cadmus API. This allows rendering a part, a layer part's fragment, and eventually even an item (via an item composer). This previewer is configured in a simple [JSON document](config.md).

## JSON Rendering and Other Techs

Any Cadmus object, either part or fragment, is ultimately archived as JSON. So, JSON is the starting point when rendering the output. As seen above, any component implementing `IJsonRenderer` can be registered in the previewer factory and used in the rendering configuration, with all its settings.

In most cases, unless complex logic is required, all what you have to do to provide a highly configurable output is using the `XsltJsonRenderer`. This component was designed right to provide the frendliest environment for rendering an output starting from any Cadmus object.

As a matter of fact, most users are accustomed to XSLT as a way of producing an HTML output from a (TEI) XML document. XSLT, though not ideal, is thus a highly popular and well-known standard which usually provides enough power to transform the input semantic markup (XML) into a presentational markup (HTML).

Thus, the JSON renderer which will probably used the most relies right on XSLT. Of course, XSLT is a sister technology of XML, and is designed to transform XML input. Yet, here we have JSON. So, to fill the gap this renderer can automatically convert JSON into XML, and then apply a user-provided XSLT to it.

Yet, it may well happen that some transformations (e.g. data selection or projection) are best performed in the context of a JSON-based model, rather than in the XML DOM. After all, JSON is a serialization format for objects, and we might well need to access their structure when transforming it.

So, this renderer not only provides XSLT-based transformation, but also JSON-based transformation. This is accomplished by using [JMESPath](https://jmespath.org/tutorial.html), a powerful selection and transformation language for JSON.

The renderer can thus apply both JSON-based transformations and an XSLT script to the source object; or just any of them.

Additionally, we might also want to adjust the resulting output in some special ways, using logic which can be shared among different renderers. There is a number of tasks which are best located here, just after the renderer transforms, and before returning their result. The _renderer filters_ are in charge of these tasks.

>Finally, there is also the possibility of a special preprocessing for JSON data in this renderer: this happens when rendering layer parts, and consists in providing the basis for mapping block layer IDs to target IDs in TEI stand-off. To this end, the renderer can inject properties into layer part fragments during JSON preprocessing, thus providing layer keys to be later used to link fragments to text blocks. See the [markup](markup.md) section for more.

So, this JSON renderer has a number of requirements:

- it should be fully customizable by users, who are accustomed to XSLT transformations. We must then adapt our JSON data to XML, so that it can be processed via XSLT.
- it should provide a powerful way for transforming JSON data even before submitting it to the XSLT processor. This refers to a true JSON transform, rather than a raw string-based transform, just like XSLT implies a DOM rather than just working on a sequence of characters.
- both the JSON and the XML transformation should be available in any combination: JSON only, XML only, JSON + XML.
- it should be able to convert Markdown text.
- it should be able to lookup thesauri.

To this end we leverage these technologies:

- [JMESPath](https://jmespath.org/tutorial.html).
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

The model for this part (apart from the usual metadata) just contains a _citation_, which can use any type of citational scheme for a text, and a _text_, consisting of any number of lines.

When handling it in a `XsltJsonRenderer` configured for a single XSLT-based transformation, first the JSON code is automatically wrapped in a `root` element by the renderer itself, to ensure it is well-formed for XML conversion, whence:

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

>To get the XML corresponding to each part's (or item's) JSON you can use the [Cadmus CLI tool](https://github.com/vedph/cadmus_tool).

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

Of course, that's just a trivial example, but it should be enough to show the power of this multi-technology approach to JSON rendering. In real-world, the Cadmus editor will use HTML output, thus providing a highly structured presentational markup as the rendition for any part.

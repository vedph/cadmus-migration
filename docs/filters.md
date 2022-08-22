# Renderer Filters

- [Renderer Filters](#renderer-filters)
  - [Fragment Link Filter](#fragment-link-filter)
  - [Markdown Conversion Filter](#markdown-conversion-filter)
  - [Text Replacements Filter](#text-replacements-filter)
  - [Thesaurus Lookup Filter](#thesaurus-lookup-filter)
  - [Sentence Split Filter](#sentence-split-filter)

Cadmus provides some builtin filters which can be used by JSON renderers or text block renderers; as for any other component type, you are free to add your own filters.

The filters to be used are typically specified in a JSON-based [configuration](config.md), where each filter type has its own ID. You can concatenate as many filters as you want, even when they are of the same type. All the filters will be applied in the order they are defined. Here I list the builtin filters.

## Fragment Link Filter

Map layer keys into target IDs, leveraging the metadata built by the text renderer run before this filter. This is used to link fragments renditions to their base text (see about [building TEI](markup.md) for more).

- ID: `it.vedph.renderer-filter.fr-link`
- options:
  - `TagOpen`: the opening tag for fragment key.
  - `TagClose`: the closing tag for fragment key.

## Markdown Conversion Filter

Convert Markdown text into HTML or plain text.

- ID: `it.vedph.renderer-filter.markdown`
- options:
  - `MarkdownOpen`: the markdown region opening tag. When not set, it is assumed that the whole text is Markdown.
  - `MarkdownClose`: the markdown region closing tag. When not set, it is assumed that the whole text is Markdown.
  - `Format`: the Markdown regions target format: if not specified, nothing is done; if `txt`, any Markdown region is converted into plain text; if `html`, any Markdown region is converted into HTML.

## Text Replacements Filter

Perform any text replacements, either literals or based on regular expressions.

- ID: `it.vedph.renderer-filter.replace`
- options:
  - `Replacements`: an array of objects with these properties:
    - `Source`: the text or pattern to find.
    - `Target`: the replacement.
    - `Repetitions`: the max repetitions count, or 0 for no limit (=keep replacing until no more changes).
    - `IsPattern`: `true` if `Source` is a regular expression pattern rather than a literal.

>Note: unless you need to effectively repeat the replacement, always set the `Repetitions` property to 1 to optimize the performance. Otherwise, the default value being 0 (=no limit), all the replacements which need to be executed just once will be repeated a second time just to discover that no repetition is required.

## Thesaurus Lookup Filter

Lookup any thesaurus entry by its ID, replacing it with its value when found, or with the entry ID when not found.

- ID: `it.vedph.renderer-filter.mongo-thesaurus`
- options:
  - `ConnectionString`: connection string to the Mongo DB. This is usually omitted and supplied by the client code from its own application settings.
  - `Pattern`: the regular expression pattern representing a thesaurus ID to lookup: it is assumed that this expression has two named captures, `t` for the thesaurus ID, and `e` for its entry ID. The default pattern is a `$` followed by the thesaurus ID, `:`, and the entry ID.

## Sentence Split Filter

- ID: `it.vedph.renderer-filter.sentence-split`
- options:
  - `EndMarkers`: the end-of-sentence marker characters. Each character in this string is treated as a sentence end marker. Any sequence of such end marker characters is treated as a single end. Default characters are `.`, `?`, `!`, Greek question mark (U+037E), and ellipsis (U+2026).
  - `NewLine`: the newline marker to use. The default value is the/ newline sequence of the host OS.
  - `Trimming`: a value indicating whether trimming spaces/tabs at both sides of any inserted newline is enabled.

# Cadmus Migration

>This is experimental work in progress.

Tools for migrating (importing/exporting) Cadmus data.

## Cadmus.Export.ML

This library contains components used to export Cadmus data into some markup language (typically XML).

As for layered text rendition, the core components here are:

- implementors of `ITextPartFlattener`. These take the part representing the base text, and any number of layer parts connected to it; and return a flat text, with ranges encoding the different spans of the text being linked to any distinct set of annotations from layers.

- `TextBlockBuilder`: this builder gets the output of an `ITextPartFlattener` and builds a number of rows containing text blocks. Each text block is a span of text with a number of layer IDs, linking that text with its annotations from layers. This is also the input model for a related HTML visualization leveraging [this brick](https://github.com/vedph/cadmus-bricks-shell/tree/master/projects/myrmidon/cadmus-text-block-view).

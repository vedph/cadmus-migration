# Item ID Collectors

Currently we have a single builtin item ID collector, based on Cadmus standard item filters.

## MongoDB Item ID Collector

- ID: `it.vedph.item-id-collector.mongo`
- options:
  - `PageNumber` (default=1)
  - `PageSize` (default=20)
  - `Title`
  - `Description`
  - `FacetId`
  - `GroupId`
  - `Flags`
  - `FlagMatching`: -1=no flags, 0=all bits set, 1=any bits set, 2=all bits clear, 3=any bits clear.
  - `UserId`
  - `MinModified`
  - `MaxModified`

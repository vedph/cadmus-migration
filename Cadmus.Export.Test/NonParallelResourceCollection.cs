﻿using Xunit;

namespace Cadmus.Export.Test;

// https://github.com/xunit/xunit/issues/1999

[CollectionDefinition(nameof(NonParallelResourceCollection),
    DisableParallelization = true)]
public class NonParallelResourceCollection { }

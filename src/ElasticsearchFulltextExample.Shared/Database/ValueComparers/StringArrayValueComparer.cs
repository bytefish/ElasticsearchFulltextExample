// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;

namespace ElasticsearchFulltextExample.Web.Database.ValueComparers
{
    public class StringArrayValueComparer : ValueComparer<string[]>
    {
        public StringArrayValueComparer()
            : base((c1, c2) => c1.SequenceEqual(c2), c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), c => c.ToArray()) { }
    }
}

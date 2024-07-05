// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq;
using TinyCsvParser.Tokenizer;

namespace ElasticsearchFulltextExample.Web.Database
{
    public class DelimitedStringValueConverter : ValueConverter<string[], string>
    {
        public DelimitedStringValueConverter(char delimiter)
            : this(delimiter, new QuotedStringTokenizer(delimiter))
        {
        }

        public DelimitedStringValueConverter(char delimiter, ITokenizer tokenizer)
            : base(x => BuildDelimitedLine(x, delimiter), x => tokenizer.Tokenize(x), null)
        {
        }

        private static string BuildDelimitedLine(string[] values, char delimiter)
        {
            var quotedValues = values.Select(value => $"\"{value}\"");

            return string.Join(delimiter, quotedValues);
        }
    }
}

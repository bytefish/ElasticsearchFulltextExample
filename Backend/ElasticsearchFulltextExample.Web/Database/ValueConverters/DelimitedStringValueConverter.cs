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

        private static string BuildDelimitedLine(string[] value, char delimiter)
        {
            var quoted = value.Select(x => $"\"{value}\"");

            return string.Join(delimiter, quoted);
        }
    }
}

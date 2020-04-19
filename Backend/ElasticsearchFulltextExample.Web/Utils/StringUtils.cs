// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace ElasticsearchFulltextExample.Web.Utils
{
    public static class StringUtils
    {
        public static string GetWords(string str, int count)
        {
            if(str == null)
            {
                return null;
            }

            var words = str
                .Split(' ')
                .Take(count);

            return string.Join(' ', words);
        }
    }
}

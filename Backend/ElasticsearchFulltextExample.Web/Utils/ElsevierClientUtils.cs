// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace ElasticsearchFulltextExample.Web.Utils
{
    public class ElsevierClientUtils
    {
        public static string GetContent(ElsevierFulltextApi.Model.Section[] sections)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach(var section in sections)
            {
                WriteSimpleContent(stringBuilder, section);
            }

            return stringBuilder.ToString();
        }

        private static void WriteSimpleContent(StringBuilder stringBuilder, ElsevierFulltextApi.Model.Section section)
        {
            stringBuilder
                .AppendLine(section.Title)
                .AppendLine();

            if (section.Paragraphs != null)
            {
                foreach (var paragraph in section.Paragraphs)
                {
                    WriteSimpleContent(stringBuilder, paragraph);
                }
            }

            if(section.Sections != null)
            {
                foreach (var subsection in section.Sections)
                {
                    WriteSimpleContent(stringBuilder, subsection);
                }
                    
            }
        }

        private static void WriteSimpleContent(StringBuilder stringBuilder, ElsevierFulltextApi.Model.Paragraph paragraph)
        {
            if(paragraph == null)
            {
                return;
            }
            
            stringBuilder
                .AppendLine(paragraph.SimpleContent);
        }

    }
}

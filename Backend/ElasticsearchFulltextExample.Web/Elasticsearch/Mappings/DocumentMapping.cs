// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Elasticsearch.Model;
using Nest;

namespace ElasticsearchFulltextExample.Web.Elasticsearch.Mappings
{
    public static class DocumentMapping
    {
        public static ITypeMapping ConfigureDocumentMapping(TypeMappingDescriptor<Document> mapping)
        {
            return mapping.Properties(properties => ConfigureDocumentProperties(properties));
        }

        private static IPromise<IProperties> ConfigureDocumentProperties(PropertiesDescriptor<Document> properties)
        {
            return properties
                .Text(textField => textField.Name(document => document.Id))
                .Text(textField => textField.Name(document => document.Content))
                .Object<Attachment>(attachment => attachment
                    .Name(document => document.Attachment)
                        .Properties(attachmentProperties => attachmentProperties
                            .Text(t => t.Name(n => n.Name))
                            .Text(t => t.Name(n => n.Content))
                            .Text(t => t.Name(n => n.ContentType))
                            .Number(n => n.Name(nn => nn.ContentLength))
                            .Date(d => d.Name(n => n.Date))
                            .Text(t => t.Name(n => n.Author))
                            .Text(t => t.Name(n => n.Title))
                            .Text(t => t.Name(n => n.Keywords))));
        }
    }
}
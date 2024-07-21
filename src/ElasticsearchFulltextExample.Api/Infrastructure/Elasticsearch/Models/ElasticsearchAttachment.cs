// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using ElasticsearchFulltextExample.Shared.Constants;
using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Models
{
    /// <summary>
    /// Properties extracted by the Tika Plugin (https://www.elastic.co/guide/en/elasticsearch/reference/8.14/attachment.html).
    /// </summary>
    public class ElasticsearchAttachment
    {
        [JsonPropertyName(ElasticConstants.AttachmentNames.Content)]
        public string? Content { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Title)]
        public string? Title { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Author)]
        public string? Author { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Date)]
        public string? Date { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Keywords)]
        public string[]? Keywords { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.ContentType)]
        public string? ContentType { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.ContentLength)]
        public long? ContentLength { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Language)]
        public string? Language { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Modified)]
        public string? Modified { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Format)]
        public string? Format { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Identifier)]
        public string? Identifier { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Contributor)]
        public string? Contributor { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Coverage)]
        public string? Coverage { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Modifier)]
        public string? Modifier { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.CreatorTool)]
        public string? CreatorTool { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Publisher)]
        public string? Publisher { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Relation)]
        public string? Relation { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Rights)]
        public string? Rights { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Source)]
        public string? Source { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Type)]
        public string? Type { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Description)]
        public string? Description { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.PrintDate)]
        public string? PrintDate { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.MetadataDate)]
        public string? MetadataDate { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Latitude)]
        public string? Latitude { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Longitude)]
        public string? Longitude { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Altitude)]
        public string? Altitude { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Rating)]
        public string? Rating { get; set; }

        [JsonPropertyName(ElasticConstants.AttachmentNames.Comments)]
        public string? Comments { get; set; }
    }
}
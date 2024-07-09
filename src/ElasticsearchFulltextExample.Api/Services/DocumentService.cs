﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch;
using ElasticsearchFulltextExample.Api.Infrastructure.Elasticsearch.Models;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Messages;
using ElasticsearchFulltextExample.Api.Logging;
using ElasticsearchFulltextExample.Api.Models;
using ElasticsearchFulltextExample.Database;
using ElasticsearchFulltextExample.Database.Model;
using GitClub.Infrastructure.Outbox;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ElasticsearchFulltextExample.Api.Services
{
    public class DocumentService
    {
        private readonly ILogger<ElasticsearchService> _logger;

        private readonly ApplicationOptions _options;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public DocumentService(ILogger<ElasticsearchService> logger, IOptions<ApplicationOptions> options, IDbContextFactory<ApplicationDbContext> dbContextFactory, FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _logger = logger;
            _options = options.Value;
            _dbContextFactory = dbContextFactory;
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider;
        }

        public async Task CreateDocumentAsync(Document document, List<string>? suggestions, List<string>? keywords, CancellationToken cancellationToken)
        {
            using var context = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            using (var transaction = await context.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false))
            {
                await context
                    .AddAsync(document, cancellationToken)
                    .ConfigureAwait(false);

                // Add Suggestions
                if (suggestions != null)
                {
                    foreach (var s in suggestions)
                    {
                        var suggestion = await context.Suggestions
                            .AsNoTracking()
                            .Where(x => x.Name == s)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (suggestion == null)
                        {
                            suggestion = new Suggestion
                            {
                                Name = s,
                                LastEditedBy = Constants.Users.DataConversionUserId
                            };

                            await context
                                .AddAsync(suggestion, cancellationToken)
                                .ConfigureAwait(false);
                        }

                        var documentSuggestion = new DocumentSuggestion
                        {
                            DocumentId = document.Id,
                            SuggestionId = suggestion.Id,
                            LastEditedBy = Constants.Users.DataConversionUserId
                        };

                        await context
                            .AddAsync(documentSuggestion, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                // Add Keywords
                if (keywords != null)
                {
                    foreach (var k in keywords)
                    {
                        var keyword = await context.Keywords
                            .AsNoTracking()
                            .Where(x => x.Name == k)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (keyword == null)
                        {
                            keyword = new Keyword
                            {
                                Name = k,
                                LastEditedBy = Constants.Users.DataConversionUserId
                            };

                            await context
                                .AddAsync(keyword, cancellationToken)
                                .ConfigureAwait(false);
                        }

                        var documentKeyword = new DocumentKeyword
                        {
                            DocumentId = document.Id,
                            KeywordId = keyword.Id,
                            LastEditedBy = Constants.Users.DataConversionUserId
                        };

                        await context
                            .AddAsync(documentKeyword, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                // Create the Outbox Event to notify about Document Creation. This will 
                // kick off the Elasticsearch Indexing process or whatever is required.
                var outboxEvent = OutboxEventUtils.Create(new DocumentCreatedMessage
                {
                    DocumentId = document.Id
                }, Constants.Users.DataConversionUserId);

                await context
                    .AddAsync(outboxEvent, cancellationToken)
                    .ConfigureAwait(false);

                await context.SaveChangesAsync(cancellationToken);

                // Commit the Transaction
                await transaction
                    .CommitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<(string Filename, string ContentType, byte[] Data)> GetFileInformationByDocumentIdAsync(int documentId, CancellationToken cancellationToken)
        {
            using var context = await _dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var document = await context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken)
                .ConfigureAwait(false);

            if (document == null)
            {
                throw new EntityNotFoundException
                {
                    EntityName = nameof(Document),
                    EntityId = documentId,
                };
            }

            var fileName = document.Filename;
            var fileType = GetContentType(document);
            var fileBytes = document.Data;

            return(fileName, fileType, fileBytes);
        }

        private string GetContentType(Document document)
        {
            if (document == null)
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            if (string.IsNullOrWhiteSpace(document.Filename))
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            if (!_fileExtensionContentTypeProvider.TryGetContentType(document.Filename, out var contentType))
            {
                return System.Net.Mime.MediaTypeNames.Application.Octet;
            }

            return contentType;
        }

        /// <summary>
        /// Loads the Suggestions for a Document given a Document ID.
        /// </summary>
        /// <param name="context">DbContext to read from</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Suggestions associated with a Document</returns>
        private async Task<List<Suggestion>> GetSuggestionsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            // Join Documents, DocumentSuggestions and Suggestions
            var suggestionQueryable = from document in context.Documents
                                      join documentSuggestion in context.DocumentSuggestions
                                          on document.Id equals documentSuggestion.DocumentId
                                      join suggestion in context.Suggestions
                                          on documentSuggestion.SuggestionId equals suggestion.Id
                                      where
                                        document.Id == documentId
                                      select suggestion;

            List<Suggestion> suggestions = await suggestionQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return suggestions;
        }

        /// <summary>
        /// Loads the Keywords associated with a given Document.
        /// </summary>
        /// <param name="context">DbContext to use</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Keywords associated with a given Document</returns>
        private async Task<List<Keyword>> GetKeywordsByDocumentId(ApplicationDbContext context, int documentId, CancellationToken cancellationToken)
        {
            // Join Documents, DocumentKeywords and Keywords
            var keywordQueryable = from document in context.Documents
                                   join documentKeyword in context.DocumentKeywords
                                       on document.Id equals documentKeyword.DocumentId
                                   join keyword in context.Keywords
                                       on documentKeyword.KeywordId equals keyword.Id
                                   where
                                     document.Id == documentId
                                   select keyword;

            List<Keyword> keywords = await keywordQueryable.AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return keywords;
        }
    }
}

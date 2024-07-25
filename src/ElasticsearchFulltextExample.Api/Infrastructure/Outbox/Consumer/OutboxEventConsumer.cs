// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Outbox;
using ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Messages;
using ElasticsearchFulltextExample.Api.Services;
using ElasticsearchFulltextExample.Database.Model;
using ElasticsearchFulltextExample.Shared.Infrastructure;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Consumer
{
    public class OutboxEventConsumer : IOutboxEventConsumer
    {
        private readonly ILogger<OutboxEventConsumer> _logger;

        private readonly ElasticsearchService _elasticsearchService;

        public OutboxEventConsumer(ILogger<OutboxEventConsumer> logger, ElasticsearchService elasticsearchService)
        {
            _logger = logger;
            _elasticsearchService = elasticsearchService;
        }

        public async Task ConsumeOutboxEventAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            if (outboxEvent.Payload == null)
            {
                _logger.LogWarning("Event doesn't contain a JSON Payload");

                return;
            }

            var success = outboxEvent.TryGetOutboxEventPayload(out object? payload);

            // Maybe it's better to throw up, if we receive an event, we can't handle? But probably 
            // this wasn't meant for our Service at all? We don't know, so we log a Warning and go 
            // on with life ...
            if (!success)
            {
                _logger.LogWarning("Failed to get Data from OutboxEvent (Id = {OutboxEventId}, EventType = {OutboxEventType})", outboxEvent.Id, outboxEvent.EventType);

                return;
            }

            await Task.Delay(10);

            // Now handle the given payload ...
            switch (payload)
            {
                case DocumentCreatedMessage documentCreatedMessage:
                    await HandleDocumentCreatedAsync(documentCreatedMessage, cancellationToken).ConfigureAwait(false);
                    break;
                case DocumentUpdatedMessage documentUpdatedMessage:
                    await HandleDocumentUpdatedAsync(documentUpdatedMessage, cancellationToken).ConfigureAwait(false);
                    break;
                case DocumentDeletedMessage documentDeletedMessage:
                    await HandleDocumentDeletedAsync(documentDeletedMessage, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    _logger.LogInformation("Outbox Event: {OutboxEventId}", outboxEvent.Id);
                    break;
            }
        }

        private async Task HandleDocumentCreatedAsync(DocumentCreatedMessage message, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchService
                .IndexDocumentAsync(message.DocumentId, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task HandleDocumentUpdatedAsync(DocumentUpdatedMessage message, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchService
                .UpdateDocumentAsync(message.DocumentId, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task HandleDocumentDeletedAsync(DocumentDeletedMessage message, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await _elasticsearchService
                .DeleteDocumentAsync(message.DocumentId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
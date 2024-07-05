// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Logging;
using ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Consumer;
using ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Messages;
using ElasticsearchFulltextExample.Database.Model;

namespace GitClub.Infrastructure.Outbox.Consumer
{
    public class OutboxEventConsumer : IOutboxEventConsumer
    {
        private readonly ILogger<OutboxEventConsumer> _logger;


        public OutboxEventConsumer(ILogger<OutboxEventConsumer> logger)
        {
            _logger = logger;
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

        private Task HandleDocumentCreatedAsync(DocumentCreatedMessage message, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // ...

            return Task.CompletedTask;
        }

        private Task HandleDocumentUpdatedAsync(DocumentUpdatedMessage message, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // ...

            return Task.CompletedTask;
        }

        private Task HandleDocumentDeletedAsync(DocumentDeletedMessage message, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // ...

            return Task.CompletedTask;
        }

    }
}

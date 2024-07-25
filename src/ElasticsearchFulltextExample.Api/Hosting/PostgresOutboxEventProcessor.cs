// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Configuration;
using ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Consumer;
using ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Postgres;
using ElasticsearchFulltextExample.Shared.Infrastructure;
using Microsoft.Extensions.Options;

namespace ElasticsearchFulltextExample.Api.Hosting
{
    /// <summary>
    /// Processes Outbox Events.
    /// </summary>
    public class PostgresOutboxEventProcessor : BackgroundService
    {
        private readonly ILogger<PostgresOutboxEventProcessor> _logger;

        private readonly PostgresOutboxEventProcessorOptions _options;
        private readonly OutboxEventConsumer _outboxEventConsumer;

        public PostgresOutboxEventProcessor(ILogger<PostgresOutboxEventProcessor> logger, IOptions<PostgresOutboxEventProcessorOptions> options, OutboxEventConsumer outboxEventConsumer)
        {
            _logger = logger;
            _options = options.Value;
            _outboxEventConsumer = outboxEventConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var outboxSubscriberOptions = new PostgresOutboxSubscriberOptions
            {
                ConnectionString = _options.ConnectionString,
                PublicationName = _options.PublicationName,
                ReplicationSlotName = _options.ReplicationSlotName,
                OutboxEventSchemaName = _options.OutboxEventSchemaName,
                OutboxEventTableName = _options.OutboxEventTableName
            };

            var outboxEventStream = new PostgresOutboxSubscriber(_logger, Options.Create(outboxSubscriberOptions));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var outboxEvent in outboxEventStream.StartOutboxEventStreamAsync(cancellationToken))
                    {
                        _logger.LogInformation("Processing OutboxEvent (Id = {OutboxEventId})", outboxEvent.Id);

                        try
                        {
                            await _outboxEventConsumer
                                .ConsumeOutboxEventAsync(outboxEvent, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to handle the OutboxEvent due to an Exception (ID = {OutboxEventId})", outboxEvent.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Logical Replication failed with an Error. Restarting the Stream.");

                    // Probably add some better Retry options ...
                    await Task
                        .Delay(30_000) // Reconnect every 30 Seconds
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
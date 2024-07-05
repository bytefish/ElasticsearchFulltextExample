﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.


// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Database.Model;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Outbox.Consumer
{
    public interface IOutboxEventConsumer
    {
        Task ConsumeOutboxEventAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken);
    }
}
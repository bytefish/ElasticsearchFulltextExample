﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.


// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Errors;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Exceptions
{
    public class EntityNotFoundException : ApplicationErrorException
    {
        /// <inheritdoc/>
        public override string ErrorCode => ErrorCodes.EntityNotFound;

        /// <inheritdoc/>
        public override string ErrorMessage => $"EntityNotFound (Entity = {EntityName}, EntityID = {EntityId})";

        /// <inheritdoc/>
        public override int HttpStatusCode => StatusCodes.Status404NotFound;

        /// <summary>
        /// Gets or sets the Entity Name.
        /// </summary>
        public required string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the EntityId.
        /// </summary>
        public required int EntityId { get; set; }

        /// <summary>
        /// Creates a new <see cref="EntityNotFoundException"/>.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="innerException">Reference to the Inner Exception</param>
        public EntityNotFoundException(string message = "EntityNotFound", Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}

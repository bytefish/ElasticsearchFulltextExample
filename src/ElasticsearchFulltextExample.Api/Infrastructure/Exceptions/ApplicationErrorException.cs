﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Exceptions
{
    /// <summary>
    /// Base Exception for the Application.
    /// </summary>
    public abstract class ApplicationErrorException : Exception
    {
        /// <summary>
        /// Gets the Error Code.
        /// </summary>
        public abstract string ErrorCode { get; }

        /// <summary>
        /// Gets the Error Message.
        /// </summary>
        public abstract string ErrorMessage { get; }

        /// <summary>
        /// Gets the HttpStatusCode.
        /// </summary>
        public abstract int HttpStatusCode { get; }

        protected ApplicationErrorException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

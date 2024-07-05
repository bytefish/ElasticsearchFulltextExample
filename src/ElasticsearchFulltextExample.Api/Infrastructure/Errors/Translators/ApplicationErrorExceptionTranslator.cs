﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Api.Infrastructure.Errors;
using ElasticsearchFulltextExample.Api.Infrastructure.Exceptions;
using GitClub.Infrastructure.Logging;
using GitClub.Models;

namespace ElasticsearchFulltextExample.Api.Infrastructure.Errors.Translators
{
    public class ApplicationErrorExceptionTranslator : IExceptionTranslator
    {
        private readonly ILogger<ApplicationErrorExceptionTranslator> _logger;

        public ApplicationErrorExceptionTranslator(ILogger<ApplicationErrorExceptionTranslator> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public ApplicationErrorResult GetApplicationErrorResult(Exception exception, bool includeExceptionDetails)
        {
            _logger.TraceMethodEntry();

            var applicationErrorException = (ApplicationErrorException)exception;

            return InternalGetApplicationErrorResult(applicationErrorException, includeExceptionDetails);
        }

        private ApplicationErrorResult InternalGetApplicationErrorResult(ApplicationErrorException exception, bool includeExceptionDetails)
        {
            var error = new ApplicationError
            {
                Code = exception.ErrorCode,
                Message = exception.ErrorMessage,
            };

            error.InnerError = new ApplicationInnerError();

            // Create the Inner Error
            if (includeExceptionDetails)
            {
                error.InnerError.Message = exception.Message;
                error.InnerError.StackTrace = exception.StackTrace;
                error.InnerError.Target = exception.GetType().Name;
            }

            return new ApplicationErrorResult
            {
                Error = error,
                HttpStatusCode = exception.HttpStatusCode,
            };
        }

        /// <inheritdoc/>
        public Type ExceptionType => typeof(ApplicationErrorException);
    }
}

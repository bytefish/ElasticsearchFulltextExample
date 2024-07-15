﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Runtime.Serialization;

namespace ElasticsearchFulltextExample.Shared.Client
{
    public class ApiException : Exception
    {
        public ApiException()
        {
        }

        public ApiException(string? message) : base(message)
        {
        }

        public ApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Http status code.
        /// </summary>
        public required HttpStatusCode StatusCode { get; set; }
    }
}
﻿using System.Text.Json.Serialization;

namespace ElasticsearchFulltextExample.Api.Models
{
    /// <summary>
    /// Represents an error payload.
    /// </summary>
    public class ApplicationError
    {
        /// <summary>
        /// The value for the code name/value pair is a non-empty language-independent string. Its value is a service-defined 
        /// error code.This code serves as a sub-status for the HTTP error code specified in the response.It cannot be null.
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// The value for the message name/value pair is a non-empty, language-dependent, human-readable string describing the 
        /// error.The Content-Language header MUST contain the language code from [RFC5646] corresponding to the language in 
        /// which the value for message is written.It cannot be null.
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// The value for the target name/value pair is a potentially empty string indicating the target of the error (for example, 
        /// the name of the property in error). It can be null.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The value for the details name/value pair MUST be an array of JSON objects that MUST contain name/value pairs for code and 
        /// message, and MAY contain a name/value pair for target.
        /// </summary>
        public List<ApplicationErrorDetail> Details { get; set; } = new();

        /// <summary>
        /// The value for the innererror name/value pair MUST be an object. The contents of this object are service-defined. Usually 
        /// this object contains information that will help debug the service.
        /// </summary>
        public ApplicationInnerError InnerError { get; set; }

        /// <summary>
        /// Additional Properties.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalProperties { get; set; } = new();
    }
}
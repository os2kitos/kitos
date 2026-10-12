using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace Presentation.Web.Infrastructure.Attributes
{
    /// <summary>
    /// Decorator for documenting API response types and status codes in Swagger.
    /// Wraps ProducesResponseTypeAttribute to accept HttpStatusCode enum values directly without casting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class ApiResponseAttribute : ProducesResponseTypeAttribute
    {
        /// <summary>
        /// Specify a response status code (e.g., 200, 400, 404).
        /// </summary>
        public ApiResponseAttribute(HttpStatusCode statusCode)
            : base((int)statusCode)
        {
        }

        /// <summary>
        /// Specify a response with a type and status code.
        /// </summary>
        public ApiResponseAttribute(Type responseType, HttpStatusCode statusCode)
            : base(responseType, (int)statusCode)
        {
        }
    }
}

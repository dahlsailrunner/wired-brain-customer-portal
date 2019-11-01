using System;
using Microsoft.AspNetCore.Http;

namespace WiredBrain.Api.Middleware
{
    public class ApiExceptionOptions
    {
        public Action<HttpContext, Exception, ApiError> AddResponseDetails { get; set; }
    }
}

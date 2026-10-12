using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Presentation.Web.Infrastructure.OData;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class NormalizeODataQueryStringMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var queryString = context.Request.QueryString.Value;
            if (context.Request.Path.StartsWithSegments("/odata") && queryString != null)
            {
                // Some upstream clients/proxies rewrite nested OData option separators from ';' to '&'.
                // Normalize them back before ASP.NET Core and OData parse the query string.
                var normalizedQueryString = ODataQueryStringNormalizer.NormalizeNestedQueryOptions(queryString);
                if (normalizedQueryString != queryString)
                {
                    context.Request.QueryString = new QueryString(normalizedQueryString);
                }
            }

            await next(context);
        }
    }
}

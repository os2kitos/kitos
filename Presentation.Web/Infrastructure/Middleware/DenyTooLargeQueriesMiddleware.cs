using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Shared;
using Microsoft.AspNetCore.Http;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class DenyTooLargeQueriesMiddleware : IMiddleware
    {
        private const int MinPageSize = PagingContraints.MinPageSize;
        private const int MaxPageSize = PagingContraints.MaxPageSize;
        private const string TopQuery = "$top";
        private const string TakeQuery = "take";

        private readonly IAuthenticationContext _authenticationContext;

        public DenyTooLargeQueriesMiddleware(IAuthenticationContext authenticationContext)
        {
            _authenticationContext = authenticationContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_authenticationContext.Method == AuthenticationMethod.KitosToken)
            {
                var query = context.Request.Query;
                var pageSizeQuery = MatchPageSizeQuery(query);
                var validRequest = pageSizeQuery
                    .Select(queryParam => MatchValidPageSize(query, queryParam))
                    .GetValueOrFallback(true);

                if (!validRequest)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync($"The value of the '{pageSizeQuery.Value}' parameter must be a number between {MinPageSize} and {MaxPageSize}");
                    return;
                }
            }

            await next(context);
        }

        private static bool MatchValidPageSize(IQueryCollection query, string key)
        {
            return ParseIntegerFrom(query, key)
                .Select(take => take is >= MinPageSize and <= MaxPageSize)
                .GetValueOrFallback(false);
        }

        private static Maybe<string> MatchPageSizeQuery(IQueryCollection collection)
        {
            if (collection.ContainsKey(TakeQuery))
                return TakeQuery;
            if (collection.ContainsKey(TopQuery))
                return TopQuery;
            return Maybe<string>.None;
        }

        private static Maybe<int> ParseIntegerFrom(IQueryCollection collection, string key)
        {
            return int.TryParse(collection[key], out var intValue) ? intValue : Maybe<int>.None;
        }
    }
}

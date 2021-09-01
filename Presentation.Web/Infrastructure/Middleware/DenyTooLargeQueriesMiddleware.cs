using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Shared;
using Infrastructure.Services.Types;
using Microsoft.Owin;
using Ninject;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class DenyTooLargeQueriesMiddleware : OwinMiddleware
    {
        public DenyTooLargeQueriesMiddleware(OwinMiddleware next) : base(next)
        {
        }

        private const int MinPageSize = PagingContraints.MinPageSize;
        private const int MaxPageSize = PagingContraints.MaxPageSize;
        private const string TopQuery = "$top";
        private const string TakeQuery = "take";

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var authenticationContext = kernel.Get<IAuthenticationContext>();
            if (authenticationContext.Method == AuthenticationMethod.KitosToken)
            {
                var query = context.Request.Query;
                var pageSizeQuery = MatchPageSizeQuery(query);
                var validRequest = pageSizeQuery
                    .Select(queryParam => MatchValidPageSize(query, queryParam))
                    .GetValueOrFallback(true); //Fallback to true of no query param

                if (!validRequest)
                {
                    RejectRequest(context, pageSizeQuery.Value);
                    return;
                }
            }

            await Next.Invoke(context);

        }

        private static void RejectRequest(IOwinContext context, string queryParameter)
        {
            context.Response.StatusCode = 400;
            context.Response.Write($"The value of the '{queryParameter}' parameter must be a number between {MinPageSize} and {MaxPageSize}");
        }

        private static bool MatchValidPageSize(IReadableStringCollection query, string key)
        {
            return ParseIntegerFrom(query, key)
                .Select(take => take is >= MinPageSize and <= MaxPageSize)
                .GetValueOrFallback(false);
        }

        private static Maybe<string> MatchPageSizeQuery(IReadableStringCollection collection)
        {
            if (collection.Get(TakeQuery) != null)
            {
                return TakeQuery;
            }

            if (collection.Get(TopQuery) != null)
            {
                return TopQuery;
            }

            return Maybe<string>.None;
        }

        private static Maybe<int> ParseIntegerFrom(IReadableStringCollection collection, string key)
        {
            return int.TryParse(collection.Get(key), out var intValue) ? intValue : Maybe<int>.None;
        }
    }
}
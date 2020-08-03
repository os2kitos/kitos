using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Microsoft.Owin;
using Ninject;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class DenyTooLargeQueriesMiddleware : OwinMiddleware
    {
        public DenyTooLargeQueriesMiddleware(OwinMiddleware next) : base(next)
        {
        }

        private const int MaxPageSize = 100;

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var logger = kernel.Get<ILogger>();
            var authenticationContext = kernel.Get<IAuthenticationContext>();
            if (authenticationContext.Method == AuthenticationMethod.KitosToken)
            {
                var query = context.Request.Query;
                var resultLimiter = ContainsResultLimit(query);
                switch (resultLimiter)
                {
                    case PageSizer.Top:
                        if (int.TryParse(query.Get("$top"), out var topPageSize))
                        {
                            LogIfExcessivePageSize(topPageSize, PageSizer.Top, logger);
                            break;
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Write($"The value of the \"$top\" parameter must be a number between 0 and {MaxPageSize}");
                            return;
                        }
                    case PageSizer.Take:
                        if (int.TryParse(query.Get("take"), out var takePageSize))
                        {
                            LogIfExcessivePageSize(takePageSize, PageSizer.Take, logger);
                            break;
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Write($"The value of the \"take\" parameter must be a number between 0 and {MaxPageSize}");
                            return;
                        }
                    case PageSizer.None:
                    default:
                        break;
                }
            }
            
            await Next.Invoke(context);

        }

        private static void LogIfExcessivePageSize(int pageSize, PageSizer pageSizeType, ILogger logger)
        {
            if (pageSize >= MaxPageSize)
            {
                logger.Warning($"Request asks with a paging type of: {pageSizeType.ToString()}, pagesize of: {pageSize}, which is larger than the max of: {MaxPageSize}");
            }
        }

        private static PageSizer ContainsResultLimit(IReadableStringCollection collection)
        {
            if (collection.Get("take") != null)
            {
                return PageSizer.Take;
            }

            if (collection.Get("$top") != null)
            {
                return PageSizer.Top;
            }

            return PageSizer.None;
        }
    }

    public enum PageSizer
    {
        Top,
        Take,
        None
    }
}
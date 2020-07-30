using System.Threading.Tasks;
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

        private static int _maxPageSize = 100;

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var logger = kernel.Get<ILogger>();

            var query = context.Request.Query;
            var resultLimiter = ContainsResultLimit(query);
            switch (resultLimiter)
            {
                case PageSizer.Top:
                    if (int.TryParse(query.Get("top"), out var topPageSize))
                    {
                        logIfExcessivePageSize(topPageSize, logger);
                        await Next.Invoke(context);
                        break;
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Write($"Værdien af \"top\" parameteret skal være et nummer mellem 0 og {_maxPageSize}");
                        break;
                    }
                case PageSizer.Take:
                    if (int.TryParse(query.Get("take"), out var takePageSize))
                    {
                        logIfExcessivePageSize(takePageSize, logger);
                        await Next.Invoke(context);
                        break;
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Write($"Værdien af \"top\" parameteret skal være et nummer mellem 0 og {_maxPageSize}");
                        break;
                    }
                case PageSizer.None:
                default:
                    await Next.Invoke(context);
                    break;
            }
        }

        private void logIfExcessivePageSize(int pageSize, ILogger logger)
        {
            if (pageSize >= _maxPageSize)
            {
                logger.Warning($"Request asks for too large a pagesize, size is {_maxPageSize}");
            }
        }

        private static PageSizer ContainsResultLimit(IReadableStringCollection collection)
        {
            if (collection.Get("take") != null)
            {
                return PageSizer.Take;
            }

            if (collection.Get("top") != null)
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
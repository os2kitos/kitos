using System.Linq;
using Core.ApplicationServices.Shared;
using Presentation.Web.Models.External.V2;

namespace Presentation.Web.Extensions
{
    public static class QueryablePaginationExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> src, StandardPaginationQuery pagination)
        {
            var paginationPageSize = pagination?.PageSize.GetValueOrDefault(PagingContraints.MaxPageSize) ?? PagingContraints.MaxPageSize;
            var page = pagination?.Page.GetValueOrDefault(0) ?? 0;

            return src.Skip(page * paginationPageSize).Take(paginationPageSize);
        }
    }
}
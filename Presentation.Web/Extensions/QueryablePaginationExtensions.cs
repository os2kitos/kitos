using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Shared;
using Presentation.Web.Models.External.V2.Request;

namespace Presentation.Web.Extensions
{
    public static class QueryablePaginationExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> src, BoundedPaginationQuery pagination)
        {
            var paginationPageSize = GetPaginationPageSize(pagination);
            var page = GetPaginationPage(pagination);

            return src.Skip(page * paginationPageSize).Take(paginationPageSize);
        }

        public static IEnumerable<T> Page<T>(this IEnumerable<T> src, BoundedPaginationQuery pagination)
        {
            var paginationPageSize = GetPaginationPageSize(pagination);
            var page = GetPaginationPage(pagination);

            return src.Skip(page * paginationPageSize).Take(paginationPageSize);
        }

        private static int GetPaginationPage(BoundedPaginationQuery pagination)
        {
            return pagination?.Page ?? 0;
        }

        private static int GetPaginationPageSize(BoundedPaginationQuery pagination)
        {
            return pagination?.PageSize ?? PagingContraints.MaxPageSize;
        }

        //private static int GetPaginationPage(BoundedPaginationQuery pagination)
        //{
        //    return pagination?.Page.GetValueOrDefault(0) ?? 0;
        //}

        //private static int GetPaginationPageSize(BoundedPaginationQuery pagination)
        //{
        //    return pagination?.PageSize.GetValueOrDefault(PagingContraints.MaxPageSize) ?? PagingContraints.MaxPageSize;
        //}
    }
}
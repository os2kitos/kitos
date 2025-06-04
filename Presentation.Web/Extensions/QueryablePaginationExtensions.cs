using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Shared;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;

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

        public static IQueryable<T> Page<T>(this IQueryable<T> src, UnboundedPaginationQuery pagination)
        {
            return pagination
                .FromNullable()
                .Match
                (pagingParameters =>
                    {
                        var page = GetPaginationPage(pagingParameters);

                        var offSetResult = src.Skip(page * pagingParameters.PageSize.GetValueOrDefault(0));

                        return pagingParameters.PageSize.HasValue
                            ? offSetResult.Take(pagingParameters.PageSize.Value)
                            : offSetResult;
                    },
                    () => src
                );
        }

        public static IEnumerable<T> Page<T>(this IEnumerable<T> src, UnboundedPaginationQuery pagination)
        {
            return pagination
                .FromNullable()
                .Match
                (pagingParameters =>
                    {
                        var page = GetPaginationPage(pagingParameters);

                        var offSetResult = src.Skip(page * pagingParameters.PageSize.GetValueOrDefault(0));

                        return pagingParameters.PageSize.HasValue
                            ? offSetResult.Take(pagingParameters.PageSize.Value)
                            : offSetResult;
                    },
                    () => src
                );
        }

        private static int GetPaginationPageSize(BoundedPaginationQuery pagination)
        {
            return pagination?.PageSize.GetValueOrDefault(BoundedPaginationConstraints.MaxPageSize) ?? BoundedPaginationConstraints.MaxPageSize;
        }

        private static int GetPaginationPage(IStandardPaginationQueryParameters pagination)
        {
            return pagination?.Page.GetValueOrDefault(0) ?? 0;
        }
    }
}
﻿using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Shared;
using Presentation.Web.Models.External.V2.Request;

namespace Presentation.Web.Extensions
{
    public static class QueryablePaginationExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> src, StandardPaginationQuery pagination)
        {
            var paginationPageSize = GetPaginationPageSize(pagination);
            var page = GetPaginationPage(pagination);

            return src.Skip(page * paginationPageSize).Take(paginationPageSize);
        }

        public static IEnumerable<T> Page<T>(this IEnumerable<T> src, StandardPaginationQuery pagination)
        {
            var paginationPageSize = GetPaginationPageSize(pagination);
            var page = GetPaginationPage(pagination);

            return src.Skip(page * paginationPageSize).Take(paginationPageSize);
        }

        private static int GetPaginationPage(StandardPaginationQuery pagination)
        {
            return pagination?.Page.GetValueOrDefault(0) ?? 0;
        }

        private static int GetPaginationPageSize(StandardPaginationQuery pagination)
        {
            return pagination?.PageSize.GetValueOrDefault(PagingContraints.MaxPageSize) ?? PagingContraints.MaxPageSize;
        }
    }
}
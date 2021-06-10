using Core.DomainModel;
using Core.DomainModel.ItSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Extensions
{
    public static class PaginationExtensions
    {
        public static IEnumerable<T> Pagination<T>(this IEnumerable<T> source, int pageSize, int pageNumber)
        {
            return source.Skip(pageSize * pageNumber).Take(pageSize);
        }

    }
}
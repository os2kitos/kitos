using System.ComponentModel.DataAnnotations;
using Core.ApplicationServices.Shared;

namespace Presentation.Web.Models.External.V2.Request
{
    public class StandardPaginationQuery
    {
        [Range(0, int.MaxValue)] public int? Page { get; set; } = 0;

        [Range(PagingContraints.MinPageSize, PagingContraints.MaxPageSize)]
        public int? PageSize { get; set; } = PagingContraints.MaxPageSize;
    }
}
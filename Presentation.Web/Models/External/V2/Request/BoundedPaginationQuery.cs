using System.ComponentModel.DataAnnotations;
using Core.ApplicationServices.Shared;

namespace Presentation.Web.Models.External.V2.Request
{
    /// <summary>
    /// Defines the a bounded pagination query parameter set
    /// </summary>
    public class BoundedPaginationQuery : IStandardPaginationQueryParameters

    {
        /// <summary>
        /// 0-based page number. Use this parameter to page through the requested collection.
        /// Offset in the source collection will be (pageSize * page)
        /// Range: [0,2^31] Default: 0
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Page { get; set; } = null;

        /// <summary>
        /// Size of the page referred by 'page'.
        /// Range: [1,100] Default: 100.
        /// </summary>
        [Range(PagingContraints.MinPageSize, PagingContraints.MaxPageSize)]
        public int? PageSize { get; set; } = null;
    }
}
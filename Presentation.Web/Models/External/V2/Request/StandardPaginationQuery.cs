using System.ComponentModel.DataAnnotations;
using Core.ApplicationServices.Shared;

namespace Presentation.Web.Models.External.V2.Request
{
    /// <summary>
    /// Defines the terms used for pagination across the V2 API
    /// </summary>
    public class StandardPaginationQuery
    {
        /// <summary>
        /// 0-based page number. Use this parameter to page through the requested collection.
        /// Default: 0
        /// </summary>
        [Range(0, int.MaxValue)] public int? Page { get; set; } = 0;

        /// <summary>
        /// Size of the page referred by <see cref="Page"/>.
        /// Default: <see cref="PagingContraints.MaxPageSize"/>.
        /// </summary>
        [Range(PagingContraints.MinPageSize, PagingContraints.MaxPageSize)]
        public int? PageSize { get; set; } = PagingContraints.MaxPageSize;
    }
}
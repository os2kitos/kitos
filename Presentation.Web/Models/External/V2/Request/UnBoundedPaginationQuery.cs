using System.ComponentModel.DataAnnotations;
using Core.ApplicationServices.Shared;

namespace Presentation.Web.Models.External.V2.Request
{
    /// <summary>
    /// Defines the an unbounded pagination query parameter set
    /// </summary>
    public class UnboundedPaginationQuery : IStandardPaginationQueryParameters

    {
        /// <summary>
        /// 0-based page number. Use this parameter to page through the requested collection.
        /// Range: [0,2^31] Default: 0
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Page { get; set; } = null;

        /// <summary>
        /// Size of the page referred by 'page'.
        /// Range: [1,2^31] Default: null.
        /// If left unspecified, the entire result set will be returned.
        /// </summary>
        [Range(PagingContraints.MinPageSize, PagingContraints.MaxPageSize)]
        public int? PageSize { get; set; } = null;
    }
}
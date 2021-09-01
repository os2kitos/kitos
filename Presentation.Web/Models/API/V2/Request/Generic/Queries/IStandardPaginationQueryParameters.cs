namespace Presentation.Web.Models.API.V2.Request.Generic.Queries
{
    public interface IStandardPaginationQueryParameters
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}

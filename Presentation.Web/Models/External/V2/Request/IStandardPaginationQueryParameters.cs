namespace Presentation.Web.Models.External.V2.Request
{
    public interface IStandardPaginationQueryParameters
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

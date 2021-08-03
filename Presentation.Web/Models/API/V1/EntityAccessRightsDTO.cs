namespace Presentation.Web.Models.API.V1
{
    public class EntityAccessRightsDTO
    {
        public int Id { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanView { get; set; }
    }
}
namespace Presentation.Web.Models.API.V1
{
    public abstract class ItProjectStatusDTO
    {
        public int Id { get; set; }
        public string HumanReadableId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public int TimeEstimate { get; set; }
        public int ObjectOwnerId { get; set; }
        public virtual UserDTO ObjectOwner { get; set; }
        public int? AssociatedPhaseNum { get; set; }
        public int? AssociatedItProjectId { get; set; }
        public int? AssociatedUserId { get; set; }
        public virtual UserDTO AssociatedUser { get; set; }
    }
}

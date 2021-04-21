namespace Presentation.Web.Models
{
    public class HierachyNodeDTO : NamedEntityDTO
    {
        public int? ParentId { get; set; }

        public HierachyNodeDTO(int id, string name, int? parentId)
            : base(id, name)
        {
            ParentId = parentId;
        }
    }
}
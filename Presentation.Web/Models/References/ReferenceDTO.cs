namespace Presentation.Web.Models.References
{
    public class ReferenceDTO : NamedEntityDTO
    {
        public ReferenceDTO(int id, string name)
            : base(id, name)
        {

        }

        public string ReferenceId { get; set; }

        public string Url { get; set; }

        public bool MasterReference { get; set; }
    }
}
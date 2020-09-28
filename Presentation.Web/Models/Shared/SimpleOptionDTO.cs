namespace Presentation.Web.Models.Shared
{
    public class SimpleOptionDTO : NamedEntityDTO
    {
        public SimpleOptionDTO(int id, string name)
            : base(id, name)
        {}

        public string Note { get; set; }
    }
}
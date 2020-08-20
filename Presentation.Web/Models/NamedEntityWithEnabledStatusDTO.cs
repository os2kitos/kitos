namespace Presentation.Web.Models
{
    public class NamedEntityWithEnabledStatusDTO : NamedEntityDTO
    {
        public bool Disabled { get; set; }

        public NamedEntityWithEnabledStatusDTO(int id, string name, bool disabled)
            : base(id, name)
        {
            Disabled = disabled;
        }
    }
}
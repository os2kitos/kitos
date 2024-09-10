namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityWithEnabledStatusV2DTO : NamedEntityV2DTO
    {
        public bool Disabled { get; set; }

        public NamedEntityWithEnabledStatusV2DTO(int id, string name, bool disabled)
            : base(id, name)
        {
            Disabled = disabled;
        }
    }
}
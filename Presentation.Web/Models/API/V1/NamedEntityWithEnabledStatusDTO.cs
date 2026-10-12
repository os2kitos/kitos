using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1
{
    public class NamedEntityWithEnabledStatusDTO : NamedEntityDTO
    {
        public bool Disabled { get; set; }

        [SetsRequiredMembers]
        public NamedEntityWithEnabledStatusDTO(int id, string name, bool disabled)
            : base(id, name)
        {
            Disabled = disabled;
        }
    }
}
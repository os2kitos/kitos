using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1.Shared
{
    public class OptionWithDescriptionDTO : NamedEntityDTO
    {
        public string Description { get; set; }
        [SetsRequiredMembers]
        public OptionWithDescriptionDTO(int id, string name, string description)
            : base(id, name)
        {
            Description = description;
        }

    }
}
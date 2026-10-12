using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1
{
    public class NamedEntityDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }

        public NamedEntityDTO()
        {
        }

        [SetsRequiredMembers]
        public NamedEntityDTO(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
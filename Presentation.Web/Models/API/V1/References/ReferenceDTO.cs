using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1.References
{
    public class ReferenceDTO : NamedEntityDTO
    {
        [SetsRequiredMembers]
        public ReferenceDTO(int id, string name)
            : base(id, name)
        {

        }

        public required string? ReferenceId { get; set; }

        public required string? Url { get; set; }

        public bool MasterReference { get; set; }

        public NamedEntityDTO? LastChangedByUser { get; set; }

        public DateTime LastChanged { get; set; }
    }
}
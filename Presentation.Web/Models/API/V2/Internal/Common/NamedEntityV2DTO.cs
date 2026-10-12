using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityV2DTO
    {
        public required int Id { get; set; }
        public Guid? Uuid { get; set; }
        public required string Name { get; set; }

        public NamedEntityV2DTO()
        {
        }

        [SetsRequiredMembers]
        public NamedEntityV2DTO(int id, Guid? uuid, string name)
        {
            Id = id;
            Uuid = uuid;
            Name = name;
        }
    }
}
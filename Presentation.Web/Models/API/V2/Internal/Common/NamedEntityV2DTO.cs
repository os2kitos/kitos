using System;

namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityV2DTO
    {
        public int Id { get; set; }
        public Guid? Uuid { get; set; }
        public string Name { get; set; }

        public NamedEntityV2DTO()
        {
        }

        public NamedEntityV2DTO(int id, Guid? uuid, string name)
        {
            Id = id;
            Uuid = uuid;
            Name = name;
        }
    }
}
using System;

namespace Presentation.Web.Models.API.V1
{
    public class NamedEntityWithUuidDTO : NamedEntityDTO
    {
        public NamedEntityWithUuidDTO()
        {
        }

        public NamedEntityWithUuidDTO(int id, string name, Guid uuid) : base(id, name)
        {
            Uuid = uuid;
        }

        public Guid Uuid { get; set; }
    }
}
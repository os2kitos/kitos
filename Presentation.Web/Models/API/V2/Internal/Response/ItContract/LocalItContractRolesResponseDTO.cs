using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItContract
{
    public class LocalItContractRolesResponseDTO
    {
        public LocalItContractRolesResponseDTO(int id, Guid uuid, string name)
        {
            Id = id;
            Uuid = uuid;
            Name = name;
        }

        public LocalItContractRolesResponseDTO()
        {
            
        }

        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
    }
}
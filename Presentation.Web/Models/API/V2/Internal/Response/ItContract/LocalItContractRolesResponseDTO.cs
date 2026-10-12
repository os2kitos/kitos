using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItContract
{
    public class LocalItContractRolesResponseDTO
    {
        [SetsRequiredMembers]
        public LocalItContractRolesResponseDTO(int id, Guid uuid, string name)
        {
            Id = id;
            Uuid = uuid;
            Name = name;
        }

        public LocalItContractRolesResponseDTO()
        {
            
        }

        public required int Id { get; set; }
        public required Guid Uuid { get; set; }
        public required string Name { get; set; }
    }
}
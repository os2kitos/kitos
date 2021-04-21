using System;

namespace Presentation.Web.Models.External.V2
{
    public class IdentityNamePairResponseDTO
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }

        public IdentityNamePairResponseDTO(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
    }
}
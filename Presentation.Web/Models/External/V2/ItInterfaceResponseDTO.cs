using System;

namespace Presentation.Web.Models.External.V2
{
    public class ItInterfaceResponseDTO
    {
        public Guid Uuid { get; set; }
        public Guid ExposedBySystemUuid { get; set; }
        public string Name { get; set; }
        public string InterfaceId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string UrlReference { get; set; }
        // TODO: Did we decide to include these?
        //public bool Deactivated { get; set; }
        //public string DeactivationReason { get; set; }
        public DateTime Created { get; set; }
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
    }
}
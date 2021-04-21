using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2
{
    public class ItSystemResponseDTO
    {
        public Guid Uuid { get; set; }
        public Guid? ParentUuid { get; set; }
        public string Name { get; set; }
        public string? FormerName { get; set; }
        public string Description { get; set; }
        public string UrlReference { get; set; }
        public Guid? BusinessTypeUuid { get; set; }
        public IEnumerable<IdentityNamePairResponseDTO> KLERepresentations { get; set; }
        public bool Deactivated { get; set; }
        public string DeactivationReason { get; set; }
        public IdentityNamePairResponseDTO BusinessType { get; set; }
        public IdentityNamePairResponseDTO RightsHolder { get; set; }
        public IEnumerable<Guid> ExposedInterfaces { get; set; }
        public DateTime Created { get; set; }
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
        public string RecommendedArchiveDutyComment { get; set; }
        public RecommendedArchiveDutyResponseDTO RecommendedArchiveDutyResponse { get; set; }
    }
}
using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class DataProcessingRegistrationResponseDTO : IHasNameExternal, IHasUuidExternal, IHasEntityCreator, IHasLastModified
    {
        public string Name { get; set; }
        public Guid Uuid { get; set; }
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
    }
}
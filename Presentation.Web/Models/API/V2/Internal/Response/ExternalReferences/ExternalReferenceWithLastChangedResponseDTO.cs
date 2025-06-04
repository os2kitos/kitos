using System;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Models.API.V2.Internal.Response.ExternalReferences
{
    public class ExternalReferenceWithLastChangedResponseDTO : ExternalReferenceDataResponseDTO
    {
        public string LastChangedByUsername { get; set; }
        public DateTime LastChangedDate { get; set; }
    }
}
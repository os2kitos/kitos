using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Response.LocalOptions
{
    public class LocalRoleOptionResponseDTO: LocalRegularOptionResponseDTO
    {
        public bool WriteAccess { get; set; }
        public bool IsExternallyUsed { get; set; }
        public string ExternallyUsedDescription { get; set; }

        [SetsRequiredMembers]
        public LocalRoleOptionResponseDTO(Guid uuid, string name, string description, bool isLocallyAvailable, bool isObligatory, bool writeAccess, bool isExternallyUsed, string externallyUsedDescription) 
            : base(uuid, name, description, isLocallyAvailable, isObligatory)
        {
            WriteAccess = writeAccess;
            IsExternallyUsed = isExternallyUsed;
            ExternallyUsedDescription = externallyUsedDescription;
        }
    }
}
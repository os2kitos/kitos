using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class DataProcessingBusinessRoleDTO : BusinessRoleDTO
    {
        [SetsRequiredMembers]
        public DataProcessingBusinessRoleDTO(int id, string name, bool expired, bool hasWriteAccess, string note, Guid uuid) : base(id, name, expired, hasWriteAccess, note)
        {
            Uuid = uuid;
        }

        public Guid Uuid { get; set; }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityWithUserFullNameV2DTO : NamedEntityV2DTO
    {
        public string UserFullName { get; set; }
        [SetsRequiredMembers]
        public NamedEntityWithUserFullNameV2DTO(int id, Guid? uuid, string name, string userName)
            : base(id, uuid, name)
        {
            UserFullName = userName;
        }
    }
}
using System;

namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityWithUserFullNameV2DTO : NamedEntityV2DTO
    {
        public string UserFullName { get; set; }
        public NamedEntityWithUserFullNameV2DTO(int id, Guid? uuid, string name, string userName)
            : base(id, uuid, name)
        {
            UserFullName = userName;
        }

        public NamedEntityWithUserFullNameV2DTO()
        {
            
        }
    }
}
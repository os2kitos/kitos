using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationCreateRequestDTO : OrganizationBaseRequestDTO
    {
        [Required]
        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        [Required]
        [Range(1, 4)]
        public new int TypeId
        {
            get => base.TypeId;
            set => base.TypeId = value;
        }
    }
}

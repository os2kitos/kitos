using Presentation.Web.Models.API.V2.Types.Organization;
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
        public new OrganizationType Type
        {
            get => base.Type;
            set => base.Type = value;
        }
    }
}

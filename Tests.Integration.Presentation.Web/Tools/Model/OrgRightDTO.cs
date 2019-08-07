using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;

namespace Tests.Integration.Presentation.Web.Tools.Model
{
    class OrgRightDTO
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string OrganizationId { get; set; }
    }
}

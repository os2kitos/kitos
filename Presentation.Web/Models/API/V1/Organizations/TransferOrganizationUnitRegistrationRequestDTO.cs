using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class TransferOrganizationUnitRegistrationRequestDTO : ChangeOrganizationUnitRegistrationRequestDTO
    {
        public Guid TargetUnitUuid { get; set; }
    }
}
using System;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class TransferOrganizationUnitRegistrationRequestDTO : ChangeOrganizationUnitRegistrationRequestDTO
    {
        public Guid TargetUnitUuid { get; set; }
    }
}
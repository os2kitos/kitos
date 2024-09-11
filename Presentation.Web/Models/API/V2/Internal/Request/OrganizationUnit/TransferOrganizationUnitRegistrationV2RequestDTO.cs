using System;

namespace Presentation.Web.Models.API.V2.Internal.Request.OrganizationUnit
{
    public class TransferOrganizationUnitRegistrationV2RequestDTO : ChangeOrganizationUnitRegistrationV2RequestDTO
    {
        public Guid TargetUnitUuid { get; set; }
    }
}
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Organizations.Write;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using System;
using Core.ApplicationServices.Model.UiCustomization;
using Presentation.Web.Models.API.V2.Internal.Request;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IOrganizationWriteModelMapper
    {
        OrganizationMasterDataUpdateParameters ToMasterDataUpdateParameters(OrganizationMasterDataRequestDTO dto);
        OrganizationMasterDataRolesUpdateParameters ToMasterDataRolesUpdateParameters(Guid organizationUuid,
            OrganizationMasterDataRolesRequestDTO dto);

        UIModuleCustomizationParameters ToUIModuleCustomizationParameters(Guid organizationUuid,
            UIModuleCustomizationRequestDTO dto);
    }
}

using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Organizations.Write;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using System;
using Core.ApplicationServices.Model.UiCustomization;
using Core.Abstractions.Types;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IOrganizationWriteModelMapper
    {
        OrganizationMasterDataUpdateParameters ToMasterDataUpdateParameters(OrganizationMasterDataRequestDTO dto);
        OrganizationMasterDataRolesUpdateParameters ToMasterDataRolesUpdateParameters(Guid organizationUuid,
            OrganizationMasterDataRolesRequestDTO dto);

        OrganizationUpdateParameters ToOrganizationUpdateParameters(OrganizationUpdateRequestDTO dto);
        
        Result<UIModuleCustomizationParameters, OperationError> ToUIModuleCustomizationParameters(Guid organizationUuid, string moduleName,
            UIModuleCustomizationRequestDTO dto);

        UIRootConfigUpdateParameters ToUIRootConfigUpdateParameters(UIRootConfigUpdateRequestDTO dto);
    }
}

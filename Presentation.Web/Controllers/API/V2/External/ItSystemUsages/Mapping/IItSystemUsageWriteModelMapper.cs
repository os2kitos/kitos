using System.Collections.Generic;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public interface IItSystemUsageWriteModelMapper
    {
        SystemUsageUpdateParameters FromPOST(CreateItSystemUsageRequestDTO request);
        SystemUsageUpdateParameters FromPUT(UpdateItSystemUsageRequestDTO request);
        UpdatedSystemUsageGDPRProperties MapGDPR(GDPRWriteRequestDTO request);
        UpdatedSystemUsageArchivingParameters MapArchiving(ArchivingWriteRequestDTO archiving);
        IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> references);
        UpdatedSystemUsageKLEDeviationParameters MapKle(LocalKLEDeviationsRequestDTO kle);
        UpdatedSystemUsageGeneralProperties MapGeneralDataUpdate(GeneralDataUpdateRequestDTO generalData);
        UpdatedSystemUsageOrganizationalUseParameters MapOrganizationalUsage(OrganizationUsageWriteRequestDTO input);
        UpdatedSystemUsageRoles MapRoles(IEnumerable<RoleAssignmentRequestDTO> roles);
        UpdatedSystemUsageGeneralProperties MapGeneralData(GeneralDataWriteRequestDTO generalData);
    }
}
using System.Collections.Generic;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public interface IItContractWriteModelMapper
    {
        ItContractModificationParameters FromPOST(CreateNewContractRequestDTO dto);
        ItContractModificationParameters FromPUT(UpdateContractRequestDTO dto);
        ItContractGeneralDataModificationParameters MapGeneralData(ContractGeneralDataWriteRequestDTO generalDataDto);
        ItContractProcurementModificationParameters MapProcurement(ContractProcurementDataWriteRequestDTO request);
        ItContractResponsibleDataModificationParameters MapResponsible(ContractResponsibleDataWriteRequestDTO dto);
        ItContractSupplierModificationParameters MapSupplier(ContractSupplierDataWriteRequestDTO dto);
        IEnumerable<ItContractHandoverTrialUpdate> MapHandOverTrials(IEnumerable<HandoverTrialRequestDTO> dtos);
        IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> dtos);
        IEnumerable<UserRolePair> MapRoles(IEnumerable<RoleAssignmentRequestDTO> dtos);
        ItContractPaymentModelModificationParameters MapPaymentModel(ContractPaymentModelDataWriteRequestDTO request);
    }
}

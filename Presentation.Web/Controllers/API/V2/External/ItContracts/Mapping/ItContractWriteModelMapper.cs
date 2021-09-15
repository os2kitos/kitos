using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public class ItContractWriteModelMapper : WriteModelMapperBase, IItContractWriteModelMapper
    {
        public ItContractWriteModelMapper(ICurrentHttpRequest currentHttpRequest)
            : base(currentHttpRequest)
        {
        }

        public ItContractModificationParameters FromPOST(CreateNewContractRequestDTO dto)
        {
            return Map(dto);
        }

        public ItContractModificationParameters FromPUT(UpdateContractRequestDTO dto)
        {
            return Map(dto);
        }

        private ItContractModificationParameters Map<T>(T dto) where T : ContractWriteRequestDTO, IHasNameExternal
        {
            var generalData = WithResetDataIfPropertyIsDefined(dto.General, nameof(ContractWriteRequestDTO.General));
            var responsibleData = WithResetDataIfPropertyIsDefined(dto.Responsible, nameof(ContractWriteRequestDTO.Responsible));
            var procurement = WithResetDataIfPropertyIsDefined(dto.Procurement, nameof(ContractWriteRequestDTO.Procurement));
            var supplier = WithResetDataIfPropertyIsDefined(dto.Supplier, nameof(ContractWriteRequestDTO.Supplier));
            var handoverTrials = WithResetDataIfPropertyIsDefined(dto.HandoverTrials, nameof(ContractWriteRequestDTO.HandoverTrials), () => new List<HandoverTrialRequestDTO>());
            var references = WithResetDataIfPropertyIsDefined(dto.ExternalReferences, nameof(ContractWriteRequestDTO.ExternalReferences), () => new List<ExternalReferenceDataDTO>());
            var systemUsageUuids = WithResetDataIfPropertyIsDefined(dto.SystemUsageUuids, nameof(ContractWriteRequestDTO.SystemUsageUuids), () => new List<Guid>());
            var roleAssignments = WithResetDataIfPropertyIsDefined(dto.Roles, nameof(ContractWriteRequestDTO.Roles), () => new List<RoleAssignmentRequestDTO>());

            return new ItContractModificationParameters
            {
                Name = ClientRequestsChangeTo(nameof(IHasNameExternal.Name)) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                ParentContractUuid = ClientRequestsChangeTo(nameof(ContractWriteRequestDTO.ParentContractUuid)) ? dto.ParentContractUuid.AsChangedValue() : OptionalValueChange<Guid?>.None,
                General = generalData.FromNullable().Select(MapGeneralData),
                Procurement = procurement.FromNullable().Select(MapProcurement),
                SystemUsageUuids = systemUsageUuids.FromNullable(),
                Responsible = responsibleData.FromNullable().Select(MapResponsible),
                Supplier = supplier.FromNullable().Select(MapSupplier),
                HandoverTrials = handoverTrials.FromNullable().Select(MapHandOverTrials),
                ExternalReferences = references.FromNullable().Select(MapReferences),
                Roles = roleAssignments.FromNullable().Select(MapRoles)
            };
        }

        public IEnumerable<UserRolePair> MapRoles(IEnumerable<RoleAssignmentRequestDTO> dtos)
        {
            return BaseMapRoleAssignments(dtos.ToList()).Value
                .Match(assignments => assignments, Array.Empty<UserRolePair>);
        }

        public IEnumerable<ItContractHandoverTrialUpdate> MapHandOverTrials(IEnumerable<HandoverTrialRequestDTO> dtos)
        {
            return dtos.Select(x => new ItContractHandoverTrialUpdate()
            {
                HandoverTrialTypeUuid = x.HandoverTrialTypeUuid,
                ApprovedAt = x.ApprovedAt,
                ExpectedAt = x.ExpectedAt
            }).ToList();
        }

        public ItContractSupplierModificationParameters MapSupplier(ContractSupplierDataWriteRequestDTO dto)
        {
            return new()
            {
                OrganizationUuid = dto.OrganizationUuid.AsChangedValue(),
                Signed = dto.Signed.AsChangedValue(),
                SignedAt = dto.SignedAt.AsChangedValue(),
                SignedBy = dto.SignedBy.AsChangedValue()
            };
        }

        public ItContractResponsibleDataModificationParameters MapResponsible(ContractResponsibleDataWriteRequestDTO dto)
        {
            return new()
            {
                OrganizationUnitUuid = dto.OrganizationUnitUuid.AsChangedValue(),
                Signed = dto.Signed.AsChangedValue(),
                SignedAt = dto.SignedAt.AsChangedValue(),
                SignedBy = dto.SignedBy.AsChangedValue()
            };
        }

        public ItContractGeneralDataModificationParameters MapGeneralData(ContractGeneralDataWriteRequestDTO dto)
        {
            return new()
            {
                ContractId = dto.ContractId.AsChangedValue(),
                ContractTypeUuid = dto.ContractTypeUuid.AsChangedValue(),
                ContractTemplateUuid = dto.ContractTemplateUuid.AsChangedValue(),
                AgreementElementUuids = (dto.AgreementElementUuids ?? new List<Guid>()).AsChangedValue(),
                Notes = dto.Notes.AsChangedValue(),
                ValidFrom = (dto.Validity?.ValidFrom ?? Maybe<DateTime>.None).AsChangedValue(),
                ValidTo = (dto.Validity?.ValidTo ?? Maybe<DateTime>.None).AsChangedValue(),
                EnforceValid = (dto.Validity?.EnforcedValid ?? Maybe<bool>.None).AsChangedValue()
            };
        }

        public ItContractProcurementModificationParameters MapProcurement(ContractProcurementDataWriteRequestDTO request)
        {
            return new()
            {
                ProcurementStrategyUuid = request.ProcurementStrategyUuid.AsChangedValue(),
                PurchaseTypeUuid = request.PurchaseTypeUuid.AsChangedValue(),
                ProcurementPlan = MapProcurementPlan(request.ProcurementPlan).AsChangedValue()
            };
        }

        public IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> dtos)
        {
            return BaseMapReferences(dtos);
        }

        private static Maybe<(byte half, int year)> MapProcurementPlan(ProcurementPlanDTO plan)
        {
            return plan == null ? Maybe<(byte half, int year)>.None : (plan.HalfOfYear, plan.Year);
        }
    }
}
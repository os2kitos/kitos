﻿using System;
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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return Map(dto);
        }

        public ItContractModificationParameters FromPUT(UpdateContractRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

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
            var dataProcessingRegistrationUuids = WithResetDataIfPropertyIsDefined(dto.DataProcessingRegistrationUuids, nameof(ContractWriteRequestDTO.DataProcessingRegistrationUuids), () => new List<Guid>());
            var agreementPeriod = WithResetDataIfPropertyIsDefined(dto.AgreementPeriod, nameof(ContractWriteRequestDTO.AgreementPeriod));
            var paymentModel = WithResetDataIfPropertyIsDefined(dto.PaymentModel, nameof(ContractWriteRequestDTO.PaymentModel));
            var payments = WithResetDataIfPropertyIsDefined(dto.Payments, nameof(ContractWriteRequestDTO.Payments));

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
                Roles = roleAssignments.FromNullable().Select(MapRoles),
                DataProcessingRegistrationUuids = dataProcessingRegistrationUuids.FromNullable(),
                PaymentModel = paymentModel.FromNullable().Select(MapPaymentModel),
                AgreementPeriod = agreementPeriod.FromNullable().Select(MapAgreementPeriod),
                Payments = payments.FromNullable().Select(MapPayments)
            };
        }

        public ItContractPaymentDataModificationParameters MapPayments(ContractPaymentsDataWriteRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            dto.External ??= new List<PaymentRequestDTO>();
            dto.Internal ??= new List<PaymentRequestDTO>();
            return new ItContractPaymentDataModificationParameters()
            {
                ExternalPayments = dto.External.Select(MapPayment).ToList().AsChangedValue<IEnumerable<ItContractPayment>>(),
                InternalPayments = dto.Internal.Select(MapPayment).ToList().AsChangedValue<IEnumerable<ItContractPayment>>(),
            };
        }

        private static ItContractPayment MapPayment(PaymentRequestDTO dto)
        {
            return new ItContractPayment()
            {
                Note = dto.Note,
                AccountingEntry = dto.AccountingEntry,
                Acquisition = dto.Acquisition,
                Operation = dto.Operation,
                Other = dto.Other,
                AuditDate = dto.AuditDate,
                AuditStatus = dto.AuditStatus.ToTrafficLight(),
                OrganizationUnitUuid = dto.OrganizationUnitUuid
            };
        }

        public ItContractAgreementPeriodModificationParameters MapAgreementPeriod(ContractAgreementPeriodDataWriteRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new()
            {
                DurationMonths = dto.DurationMonths.AsChangedValue(),
                DurationYears = dto.DurationYears.AsChangedValue(),
                ExtensionOptionsUsed = dto.ExtensionOptionsUsed.AsChangedValue(),
                ExtensionOptionsUuid = dto.ExtensionOptionsUuid.AsChangedValue(),
                IrrevocableUntil = dto.IrrevocableUntil.AsChangedValue(),
                IsContinuous = dto.IsContinuous.AsChangedValue()
            };
        }

        public IEnumerable<UserRolePair> MapRoles(IEnumerable<RoleAssignmentRequestDTO> dtos)
        {
            if (dtos == null)
                throw new ArgumentNullException(nameof(dtos));

            return BaseMapRoleAssignments(dtos.ToList()).Value
                .Match(assignments => assignments, Array.Empty<UserRolePair>);
        }

        public IEnumerable<ItContractHandoverTrialUpdate> MapHandOverTrials(IEnumerable<HandoverTrialRequestDTO> dtos)
        {
            if (dtos == null)
                throw new ArgumentNullException(nameof(dtos));

            return dtos.Select(x => new ItContractHandoverTrialUpdate()
            {
                HandoverTrialTypeUuid = x.HandoverTrialTypeUuid,
                ApprovedAt = x.ApprovedAt,
                ExpectedAt = x.ExpectedAt
            }).ToList();
        }

        public ItContractPaymentModelModificationParameters MapPaymentModel(ContractPaymentModelDataWriteRequestDTO dto)
        {
            return new()
            {
                OperationsRemunerationStartedAt = (dto.OperationsRemunerationStartedAt?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue(),
                PaymentFrequencyUuid = dto.PaymentFrequencyUuid.AsChangedValue(),
                PaymentModelUuid = dto.PaymentModelUuid.AsChangedValue(),
                PriceRegulationUuid = dto.PriceRegulationUuid.AsChangedValue(),
                PaymentMileStones = dto.PaymentMileStones?.Select(x => new ItContractPaymentMilestone()
                {
                    Title = x.Title,
                    Approved = x.Approved,
                    Expected = x.Expected
                }).ToList()
            };
        }

        public ItContractSupplierModificationParameters MapSupplier(ContractSupplierDataWriteRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

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

        public ItContractProcurementModificationParameters MapProcurement(ContractProcurementDataWriteRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new()
            {
                ProcurementStrategyUuid = dto.ProcurementStrategyUuid.AsChangedValue(),
                PurchaseTypeUuid = dto.PurchaseTypeUuid.AsChangedValue(),
                ProcurementPlan = MapProcurementPlan(dto.ProcurementPlan).AsChangedValue()
            };
        }

        public IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> dtos)
        {
            if (dtos == null)
                throw new ArgumentNullException(nameof(dtos));

            return BaseMapReferences(dtos);
        }

        private static Maybe<(byte half, int year)> MapProcurementPlan(ProcurementPlanDTO plan)
        {
            return plan == null ? Maybe<(byte half, int year)>.None : (plan.HalfOfYear, plan.Year);
        }
    }
}
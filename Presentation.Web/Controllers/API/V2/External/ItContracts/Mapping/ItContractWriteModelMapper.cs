﻿using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel.ItContract;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
            return Map(dto, false);
        }

        public ItContractModificationParameters FromPATCH(UpdateContractRequestDTO dto)
        {
            return Map(dto, false);
        }

        public ItContractModificationParameters FromPUT(UpdateContractRequestDTO dto)
        {
            return Map(dto, true);
        }

        private ItContractModificationParameters Map<T>(T dto, bool enforceFallbackIfNotProvided) where T : ContractWriteRequestDTO, IHasNameExternal
        {
            var rule = CreateChangeRule<T>(enforceFallbackIfNotProvided);
            TSection WithResetDataIfSectionIsNotDefined<TSection>(TSection deserializedValue,
                Expression<Func<ContractWriteRequestDTO, TSection>> propertySelection) where TSection : new() =>
                WithResetDataIfPropertyIsDefined(deserializedValue,
                    propertySelection, enforceFallbackIfNotProvided);

            TSection WithResetDataIfSectionIsNotDefinedWithFallback<TSection>(TSection deserializedValue,
                Expression<Func<ContractWriteRequestDTO, TSection>> propertySelection,
                Func<TSection> fallbackFactory) =>
                WithResetDataIfPropertyIsDefined(deserializedValue,
                    propertySelection, fallbackFactory, enforceFallbackIfNotProvided);

            dto.General = WithResetDataIfSectionIsNotDefined(dto.General, x => x.General);
            dto.Responsible = WithResetDataIfSectionIsNotDefined(dto.Responsible, x => x.Responsible);
            dto.Procurement = WithResetDataIfSectionIsNotDefined(dto.Procurement, x => x.Procurement);
            dto.Supplier = WithResetDataIfSectionIsNotDefined(dto.Supplier, x => x.Supplier);
            dto.HandoverTrials = WithResetDataIfSectionIsNotDefinedWithFallback(dto.HandoverTrials,
                x => x.HandoverTrials, Array.Empty<HandoverTrialRequestDTO>);
            dto.ExternalReferences = WithResetDataIfSectionIsNotDefinedWithFallback(dto.ExternalReferences,
                x => x.ExternalReferences, Array.Empty<ExternalReferenceDataDTO>);
            dto.SystemUsageUuids = WithResetDataIfSectionIsNotDefinedWithFallback(dto.SystemUsageUuids,
                x => x.SystemUsageUuids, () => new List<Guid>());
            dto.Roles = WithResetDataIfSectionIsNotDefinedWithFallback(dto.Roles, x => x.Roles,
                Array.Empty<RoleAssignmentRequestDTO>);
            dto.DataProcessingRegistrationUuids = WithResetDataIfSectionIsNotDefinedWithFallback(
                dto.DataProcessingRegistrationUuids, x => x.DataProcessingRegistrationUuids, () => new List<Guid>());
            dto.AgreementPeriod = WithResetDataIfSectionIsNotDefined(dto.AgreementPeriod, x => x.AgreementPeriod);
            dto.PaymentModel = WithResetDataIfSectionIsNotDefined(dto.PaymentModel, x => x.PaymentModel);
            dto.Payments = WithResetDataIfSectionIsNotDefined(dto.Payments, x => x.Payments);
            dto.Termination = WithResetDataIfSectionIsNotDefined(dto.Termination, x => x.Termination);

            return new ItContractModificationParameters
            {
                Name = rule.MustUpdate(x=>x.Name) ? dto.Name.AsChangedValue() : OptionalValueChange<string>.None,
                ParentContractUuid = rule.MustUpdate(x => x.ParentContractUuid)
                    ? dto.ParentContractUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,
                General = dto.General.FromNullable().Select(generalData => MapGeneralData(generalData, rule)),
                Procurement = dto.Procurement.FromNullable().Select(procurement => MapProcurement(procurement, rule)),
                SystemUsageUuids = dto.SystemUsageUuids.FromNullable(),
                Responsible = dto.Responsible.FromNullable().Select(responsible => MapResponsible(responsible, rule)),
                Supplier = dto.Supplier.FromNullable().Select(supplier => MapSupplier(supplier, rule)),
                HandoverTrials = dto.HandoverTrials.FromNullable().Select(MapHandOverTrials),
                ExternalReferences = dto.ExternalReferences.FromNullable().Select(MapReferences),
                Roles = dto.Roles.FromNullable().Select(MapRoles),
                DataProcessingRegistrationUuids = dto.DataProcessingRegistrationUuids.FromNullable(),
                PaymentModel = dto.PaymentModel.FromNullable().Select(payment => MapPaymentModel(payment, rule)),
                AgreementPeriod = dto.AgreementPeriod.FromNullable().Select(agreementPeriod => MapAgreementPeriod(agreementPeriod, rule)),
                Payments = dto.Payments.FromNullable().Select(MapPayments),
                Termination = dto.Termination.FromNullable().Select(termination => MapTermination(termination, rule))
            };
        }

        private static ItContractPaymentDataModificationParameters MapPayments(ContractPaymentsDataWriteRequestDTO dto)
        {
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
            return new ItContractPayment
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

        private static ItContractAgreementPeriodModificationParameters MapAgreementPeriod<TRootDto>(ContractAgreementPeriodDataWriteRequestDTO dto, IPropertyUpdateRule<TRootDto> rule) where TRootDto : ContractWriteRequestDTO
        {
            return new ItContractAgreementPeriodModificationParameters
            {
                DurationMonths = rule.MustUpdate(x => x.AgreementPeriod.DurationMonths)
                    ? dto.DurationMonths.AsChangedValue()
                    : OptionalValueChange<int?>.None,

                DurationYears = rule.MustUpdate(x => x.AgreementPeriod.DurationYears)
                    ? dto.DurationYears.AsChangedValue()
                    : OptionalValueChange<int?>.None,

                ExtensionOptionsUsed = rule.MustUpdate(x => x.AgreementPeriod.ExtensionOptionsUsed)
                    ? dto.ExtensionOptionsUsed.AsChangedValue()
                    : OptionalValueChange<int>.None,

                ExtensionOptionsUuid = rule.MustUpdate(x => x.AgreementPeriod.ExtensionOptionsUuid)
                    ? dto.ExtensionOptionsUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                IrrevocableUntil = rule.MustUpdate(x => x.AgreementPeriod.IrrevocableUntil)
                    ? dto.IrrevocableUntil.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                IsContinuous = rule.MustUpdate(x => x.AgreementPeriod.IsContinuous)
                    ? dto.IsContinuous.AsChangedValue()
                    : OptionalValueChange<bool>.None
            };
        }

        private static IEnumerable<UserRolePair> MapRoles(IEnumerable<RoleAssignmentRequestDTO> dtos)
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

        private static ItContractPaymentModelModificationParameters MapPaymentModel<TRootDto>(ContractPaymentModelDataWriteRequestDTO dto, IPropertyUpdateRule<TRootDto> rule) where TRootDto : ContractWriteRequestDTO
        {
            return new ItContractPaymentModelModificationParameters
            {
                OperationsRemunerationStartedAt = rule.MustUpdate(x => x.PaymentModel.OperationsRemunerationStartedAt)
                    ? (dto.OperationsRemunerationStartedAt?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,

                PaymentFrequencyUuid = rule.MustUpdate(x => x.PaymentModel.PaymentFrequencyUuid)
                    ? dto.PaymentFrequencyUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                PaymentModelUuid = rule.MustUpdate(x => x.PaymentModel.PaymentModelUuid)
                    ? dto.PaymentModelUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                PriceRegulationUuid = rule.MustUpdate(x => x.PaymentModel.PriceRegulationUuid)
                    ? dto.PriceRegulationUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                PaymentMileStones = rule.MustUpdate(x => x.PaymentModel.PaymentMileStones)
                    ? dto.PaymentMileStones
                        .FromNullable()
                        .Select(x => x
                            .Select(y => new ItContractPaymentMilestone()
                            {
                                Title = y.Title,
                                Approved = y.Approved,
                                Expected = y.Expected
                            })).AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<ItContractPaymentMilestone>>>.None
            };
        }

        private static ItContractTerminationParameters MapTermination<TRootDto>(ContractTerminationDataWriteRequestDTO dto, IPropertyUpdateRule<TRootDto> rule) where TRootDto : ContractWriteRequestDTO
        {
            return new ItContractTerminationParameters
            {
                TerminatedAt = rule.MustUpdate(x => x.Termination.TerminatedAt)
                    ? (dto.TerminatedAt?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,

                NoticePeriodMonthsUuid = rule.MustUpdate(x => x.Termination.Terms.NoticePeriodMonthsUuid)
                    ? (dto.Terms?.NoticePeriodMonthsUuid).AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                NoticePeriodExtendsCurrent = rule.MustUpdate(x => x.Termination.Terms.NoticePeriodExtendsCurrent)
                    ? (dto.Terms?.NoticePeriodExtendsCurrent?.ToYearSegmentOption() ?? Maybe<YearSegmentOption>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<YearSegmentOption>>.None,

                NoticeByEndOf = rule.MustUpdate(x => x.Termination.Terms.NoticeByEndOf)
                    ? (dto.Terms?.NoticeByEndOf?.ToYearSegmentOption() ?? Maybe<YearSegmentOption>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<YearSegmentOption>>.None
            };
        }

        private static ItContractSupplierModificationParameters MapSupplier<TRootDto>(ContractSupplierDataWriteRequestDTO dto, IPropertyUpdateRule<TRootDto> rule) where TRootDto : ContractWriteRequestDTO
        {
            return new ItContractSupplierModificationParameters
            {
                OrganizationUuid = rule.MustUpdate(x => x.Supplier.OrganizationUuid)
                    ? dto.OrganizationUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                Signed = rule.MustUpdate(x => x.Supplier.Signed)
                    ? dto.Signed.AsChangedValue()
                    : OptionalValueChange<bool>.None,

                SignedAt = rule.MustUpdate(x => x.Supplier.SignedAt)
                    ? dto.SignedAt.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                SignedBy = rule.MustUpdate(x => x.Supplier.SignedBy)
                    ? dto.SignedBy.AsChangedValue()
                    : OptionalValueChange<string>.None
            };
        }

        private static ItContractResponsibleDataModificationParameters MapResponsible<TRootDto>(ContractResponsibleDataWriteRequestDTO dto, IPropertyUpdateRule<TRootDto> rule) where TRootDto : ContractWriteRequestDTO
        {
            return new ItContractResponsibleDataModificationParameters
            {
                OrganizationUnitUuid = rule.MustUpdate(x => x.Responsible.OrganizationUnitUuid)
                    ? dto.OrganizationUnitUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                Signed = rule.MustUpdate(x => x.Responsible.Signed)
                    ? dto.Signed.AsChangedValue()
                    : OptionalValueChange<bool>.None,

                SignedAt = rule.MustUpdate(x => x.Responsible.SignedAt)
                    ? dto.SignedAt.AsChangedValue()
                    : OptionalValueChange<DateTime?>.None,

                SignedBy = rule.MustUpdate(x => x.Responsible.SignedBy)
                    ? dto.SignedBy.AsChangedValue()
                    : OptionalValueChange<string>.None
            };
        }

        private static ItContractGeneralDataModificationParameters MapGeneralData<TRootDto>(ContractGeneralDataWriteRequestDTO dto, IPropertyUpdateRule<TRootDto> rule) where  TRootDto : ContractWriteRequestDTO
        {
            return new ItContractGeneralDataModificationParameters
            {
                ContractId = rule.MustUpdate(x => x.General.ContractId)
                    ? dto.ContractId.AsChangedValue()
                    : OptionalValueChange<string>.None,

                ContractTypeUuid = rule.MustUpdate(x => x.General.ContractTypeUuid)
                    ? dto.ContractTypeUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                ContractTemplateUuid = rule.MustUpdate(x => x.General.ContractTemplateUuid)
                    ? dto.ContractTemplateUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                AgreementElementUuids = rule.MustUpdate(x => x.General.AgreementElementUuids)
                    ? (dto.AgreementElementUuids ?? new List<Guid>()).AsChangedValue()
                    : OptionalValueChange<IEnumerable<Guid>>.None,

                Notes = rule.MustUpdate(x => x.General.Notes)
                    ? dto.Notes.AsChangedValue()
                    : OptionalValueChange<string>.None,

                ValidFrom = rule.MustUpdate(x => x.General.Validity.ValidFrom)
                    ? (dto.Validity?.ValidFrom ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,

                ValidTo = rule.MustUpdate(x => x.General.Validity.ValidTo)
                    ? (dto.Validity?.ValidTo ?? Maybe<DateTime>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<DateTime>>.None,

                EnforceValid = rule.MustUpdate(x => x.General.Validity.EnforcedValid)
                    ? (dto.Validity?.EnforcedValid ?? Maybe<bool>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<bool>>.None
            };
        }

        private static ItContractProcurementModificationParameters MapProcurement<TRootDto>(ContractProcurementDataWriteRequestDTO dto, IPropertyUpdateRule<TRootDto> rule) where TRootDto : ContractWriteRequestDTO
        {
            return new ItContractProcurementModificationParameters
            {
                ProcurementStrategyUuid = rule.MustUpdate(x => x.Procurement.ProcurementStrategyUuid)
                    ? dto.ProcurementStrategyUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                PurchaseTypeUuid = rule.MustUpdate(x => x.Procurement.PurchaseTypeUuid)
                    ? dto.PurchaseTypeUuid.AsChangedValue()
                    : OptionalValueChange<Guid?>.None,

                ProcurementPlan = rule.MustUpdate(x => x.Procurement.ProcurementPlan)
                    ? MapProcurementPlan(dto.ProcurementPlan).AsChangedValue()
                    : OptionalValueChange<Maybe<(byte half, int year)>>.None
            };
        }

        private IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> dtos)
        {
            return BaseMapReferences(dtos);
        }

        private static Maybe<(byte quarter, int year)> MapProcurementPlan(ProcurementPlanDTO plan)
        {
            return plan == null ? Maybe<(byte quarter, int year)>.None : (plan.QuarterOfYear, plan.Year);
        }
    }
}
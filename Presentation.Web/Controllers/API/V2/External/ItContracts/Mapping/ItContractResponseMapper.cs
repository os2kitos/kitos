using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Contracts;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public class ItContractResponseMapper : IItContractResponseMapper
    {
        private readonly IExternalReferenceResponseMapper _externalReferenceResponseMapper;

        public ItContractResponseMapper(IExternalReferenceResponseMapper externalReferenceResponseMapper)
        {
            _externalReferenceResponseMapper = externalReferenceResponseMapper;
        }

        public ItContractResponseDTO MapContractDTO(ItContract contract)
        {
            return new ItContractResponseDTO
            {
                Uuid = contract.Uuid,
                Name = contract.Name,
                ParentContract = contract.Parent?.MapIdentityNamePairDTO(),
                OrganizationContext = contract.Organization?.MapShallowOrganizationResponseDTO(),
                CreatedBy = contract.ObjectOwner?.MapIdentityNamePairDTO(),
                LastModified = contract.LastChanged,
                LastModifiedBy = contract.LastChangedByUser?.MapIdentityNamePairDTO(),
                General = MapGeneral(contract),
                Procurement = MapProcurement(contract),
                Supplier = MapSupplier(contract),
                Responsible = MapResponsible(contract),
                SystemUsages = contract.AssociatedSystemUsages?.Select(x => x.ItSystemUsage?.MapIdentityNamePairDTO()).ToList() ?? new List<IdentityNamePairResponseDTO>(),
                DataProcessingRegistrations = contract.DataProcessingRegistrations?.Select(x => x.MapIdentityNamePairDTO()).ToList() ?? new List<IdentityNamePairResponseDTO>(),
                PaymentModel = MapPaymentModel(contract),
                AgreementPeriod = MapAgreementPeriod(contract),
                Termination = MapTermination(contract),
                Payments = MapPayments(contract),
                Roles = MapRoles(contract),
                ExternalReferences = _externalReferenceResponseMapper.MapExternalReferences(contract.ExternalReferences)
            };
        }

        public ItContractPermissionsResponseDTO MapPermissions(ContractPermissions permissions)
        {
            return new ItContractPermissionsResponseDTO
            {
                Delete = permissions.BasePermissions.Delete,
                Modify = permissions.BasePermissions.Modify,
                Read = permissions.BasePermissions.Read
            };
        }

        private static ContractAgreementPeriodDataResponseDTO MapAgreementPeriod(ItContract contract)
        {
            return new ()
            {
                DurationYears = contract.DurationYears,
                DurationMonths = contract.DurationMonths,
                IsContinuous = contract.DurationOngoing,
                ExtensionOptions = contract.OptionExtend?.MapIdentityNamePairDTO(),
                ExtensionOptionsUsed = contract.ExtendMultiplier,
                IrrevocableUntil = contract.IrrevocableTo
            };
        }

        private static ContractTerminationDataResponseDTO MapTermination(ItContract contract)
        {
            return new ContractTerminationDataResponseDTO
            {
                TerminatedAt = contract.Terminated,
                Terms = MapTerminationTerms(contract)
            };
        }

        private static ContractTerminationTermsResponseDTO MapTerminationTerms(ItContract contract)
        {
            return new ContractTerminationTermsResponseDTO
            {
                NoticePeriodMonths = contract.TerminationDeadline?.MapIdentityNamePairDTO(),
                NoticeByEndOf = contract.ByEnding?.ToYearSegmentChoice(), 
                NoticePeriodExtendsCurrent = contract.Running?.ToYearSegmentChoice()
            };
        }

        private static ContractPaymentsDataResponseDTO MapPayments(ItContract contract)
        {
            return new ContractPaymentsDataResponseDTO
            {
                External = contract.ExternEconomyStreams?.Select(MapPaymentResponseDTO).ToList() ?? new List<PaymentResponseDTO>(),
                Internal = contract.InternEconomyStreams?.Select(MapPaymentResponseDTO).ToList() ?? new List<PaymentResponseDTO>()
            };
        }

        private static PaymentResponseDTO MapPaymentResponseDTO(EconomyStream economyStream)
        {
            return new PaymentResponseDTO
            {
                Id = economyStream.Id,
                AccountingEntry = economyStream.AccountingEntry,
                Acquisition = economyStream.Acquisition,
                AuditDate = economyStream.AuditDate,
                AuditStatus = economyStream.AuditStatus.ToPaymentAuditStatus(),
                Note = economyStream.Note,
                Operation = economyStream.Operation,
                OrganizationUnit = economyStream.OrganizationUnit != null ? ToUnitResponseDTO(economyStream.OrganizationUnit) : null,
                Other = economyStream.Other
            };
        }

        private static List<RoleAssignmentResponseDTO> MapRoles(ItContract contract)
        {
            return contract.Rights?.Select(ToRoleResponseDTO).ToList() ?? new List<RoleAssignmentResponseDTO>();
        }

        private static ContractPaymentModelDataResponseDTO MapPaymentModel(ItContract contract)
        {
            return new ContractPaymentModelDataResponseDTO
            {
                OperationsRemunerationStartedAt = contract.OperationRemunerationBegun,
                PaymentModel = contract.PaymentModel?.MapIdentityNamePairDTO(),
                PaymentFrequency = contract.PaymentFreqency?.MapIdentityNamePairDTO(),
                PriceRegulation = contract.PriceRegulation?.MapIdentityNamePairDTO(),
            };
        }

        private static ContractResponsibleDataResponseDTO MapResponsible(ItContract contract)
        {
            return new ContractResponsibleDataResponseDTO
            {
                SignedBy = contract.ContractSigner,
                Signed = contract.IsSigned,
                SignedAt = contract.SignedDate,
                OrganizationUnit = contract.ResponsibleOrganizationUnit?.MapIdentityNamePairDTO()
            };
        }

        private static ContractSupplierDataResponseDTO MapSupplier(ItContract contract)
        {
            return new ContractSupplierDataResponseDTO
            {
                SignedBy = contract.SupplierContractSigner,
                Signed = contract.HasSupplierSigned,
                SignedAt = contract.SupplierSignedDate,
                Organization = contract.Supplier?.MapShallowOrganizationResponseDTO()
            };
        }

        private static ContractProcurementDataResponseDTO MapProcurement(ItContract contract)
        {
            return new ContractProcurementDataResponseDTO
            {
                ProcurementStrategy = contract.ProcurementStrategy?.MapIdentityNamePairDTO(),
                PurchaseType = contract.PurchaseForm?.MapIdentityNamePairDTO(),
                ProcurementPlan = MapProcurementPlan(contract),
                ProcurementInitiated = contract.ProcurementInitiated?.ToYesNoUndecidedChoice(),
            };
        }

        private static ProcurementPlanDTO MapProcurementPlan(ItContract contract)
        {
            if (!contract.ProcurementPlanQuarter.HasValue)
                return null;

            if (!contract.ProcurementPlanYear.HasValue)
                return null;

            return new ProcurementPlanDTO
            {
                QuarterOfYear = Convert.ToByte(contract.ProcurementPlanQuarter.Value),
                Year = contract.ProcurementPlanYear.Value
            };
        }

        private static ContractGeneralDataResponseDTO MapGeneral(ItContract contract)
        {
            return new ContractGeneralDataResponseDTO
            {
                ContractId = contract.ItContractId,
                Notes = contract.Note,
                ContractTemplate = contract.ContractTemplate?.MapIdentityNamePairDTO(),
                ContractType = contract.ContractType?.MapIdentityNamePairDTO(),
                AgreementElements = contract.AssociatedAgreementElementTypes?.Select(x => x.AgreementElementType?.MapIdentityNamePairDTO()).ToList() ?? new List<IdentityNamePairResponseDTO>(),
                Criticality = contract.Criticality?.MapIdentityNamePairDTO(),
                Validity = new ContractValidityResponseDTO
                {
                    EnforcedValid = contract.Active,
                    Valid = contract.IsActive,
                    ValidFrom = contract.Concluded,
                    ValidTo = contract.ExpirationDate,
                    RequireValidParent = contract.RequireValidParent,
                    ValidationErrors = contract.Validate().ValidationErrors.Select(x => x.ToItContractValidationErrorChoice()).ToList()
                }
            };
        }

        private static RoleAssignmentResponseDTO ToRoleResponseDTO(ItContractRight right)
        {
            return new RoleAssignmentResponseDTO
            {
                Role = right.Role.MapIdentityNamePairDTO(),
                User = right.User.MapIdentityNamePairDTO()
            };
        }

        private static OrganizationUnitResponseDTO ToUnitResponseDTO(OrganizationUnit unit)
        {
            return new OrganizationUnitResponseDTO
            {
                Uuid = unit.Uuid,
                Name = unit.Name,
                Ean = unit.Ean,
                ParentOrganizationUnit = unit.Parent?.MapIdentityNamePairDTO()
            };
        }
    }
}
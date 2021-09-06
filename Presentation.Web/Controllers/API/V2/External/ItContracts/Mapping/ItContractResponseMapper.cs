using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Generic.Validity;
using Presentation.Web.Models.API.V2.Types.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public class ItContractResponseMapper : IItContractResponseMapper
    {
        public ItContractResponseDTO MapContractDTO(ItContract contract)
        {
            return new ItContractResponseDTO()
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
                SystemUsages = contract.AssociatedSystemUsages.Select(x => x.ItSystemUsage?.MapIdentityNamePairDTO()).ToList(),
                DataProcessingRegistrations = contract.DataProcessingRegistrations.Select(x => x.MapIdentityNamePairDTO()).ToList(),
                HandoverTrials = MapHandoverTrials(contract),
                PaymentModel = MapPaymentModel(contract),
                AgreementPeriod = MapAgreementPeriod(contract),
                Termination = MapTermination(contract),
                Payments = MapPayments(contract),
                Roles = MapRoles(contract),
                ExternalReferences = MapExternalReferences(contract)
            };
        }

        private ContractAgreementPeriodDataResponseDTO MapAgreementPeriod(ItContract contract)
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

        private ContractTerminationDataResponseDTO MapTermination(ItContract contract)
        {
            return new ()
            {
                TerminatedAt = contract.Terminated,
                Terms = MapTerminationTerms(contract)
            };
        }

        private ContractTerminationTermsResponseDTO MapTerminationTerms(ItContract contract)
        {
            return new ()
            {
                NoticePeriodMonths = contract.TerminationDeadline?.MapIdentityNamePairDTO(),
                //NoticeByEndOf = contract.Running, //TODO: Fix string => enum
                //NoticePeriodExtendsCurrent = contract.ByEnding //TODO: Fix string => enum
            };
        }

        private ContractPaymentsDataResponseDTO MapPayments(ItContract contract)
        {
            return new ()
            {
                External = contract.ExternEconomyStreams.Select(MapPaymentResponseDTO).ToList(),
                Internal = contract.InternEconomyStreams.Select(MapPaymentResponseDTO).ToList()
            };
        }

        private PaymentResponseDTO MapPaymentResponseDTO(EconomyStream economyStream)
        {
            return new ()
            {
                AccountingEntry = economyStream.AccountingEntry,
                Acquisition = economyStream.Acquisition,
                AuditDate = economyStream.AuditDate,
                AuditStatus = economyStream.AuditStatus.ToPaymentAuditStatus(),
                Note = economyStream.Note,
                Operation = economyStream.Operation,
                OrganizationUnit = economyStream.OrganizationUnit?.MapIdentityNamePairDTO(),
                Other = economyStream.Other
            };
        }

        private List<RoleAssignmentResponseDTO> MapRoles(ItContract contract)
        {
            return contract.Rights.Select(ToRoleResponseDTO).ToList();
        }

        private List<ExternalReferenceDataDTO> MapExternalReferences(ItContract contract)
        {
            return contract.ExternalReferences.Select(x => MapExternalReferenceDTO(contract, x)).ToList();
        }

        private ContractPaymentModelDataResponseDTO MapPaymentModel(ItContract contract)
        {
            return new ()
            {
                OperationsRemunerationStartedAt = contract.OperationRemunerationBegun,
                PaymentModel = contract.PaymentModel?.MapIdentityNamePairDTO(),
                PaymentFrequency = contract.PaymentFreqency?.MapIdentityNamePairDTO(),
                PriceRegulation = contract.PriceRegulation?.MapIdentityNamePairDTO(),
                PaymentMileStones = contract.PaymentMilestones.Select(MapPaymentMilestones).ToList()
            };
        }

        private PaymentMileStoneDTO MapPaymentMilestones(PaymentMilestone paymentMilestone)
        {
            return new ()
            {
                Title = paymentMilestone.Title,
                Approved = paymentMilestone.Approved,
                Expected = paymentMilestone.Expected
            };
        }

        private List<HandoverTrialResponseDTO> MapHandoverTrials(ItContract contract)
        {
            return contract.HandoverTrials.Select(MapHandoverTrial).ToList();
        }

        private HandoverTrialResponseDTO MapHandoverTrial(HandoverTrial handoverTrial)
        {
            return new ()
            {
                HandoverTrialType = handoverTrial.HandoverTrialType?.MapIdentityNamePairDTO(),
                ApprovedAt = handoverTrial.Approved,
                ExpectedAt = handoverTrial.Expected
            };
        }

        private ContractResponsibleDataResponseDTO MapResponsible(ItContract contract)
        {
            return new ()
            {
                SignedBy = contract.ContractSigner,
                Signed = contract.IsSigned,
                SignedAt = contract.SignedDate,
                OrganizationUnit = contract.ResponsibleOrganizationUnit?.MapIdentityNamePairDTO()
            };
        }

        private ContractSupplierDataResponseDTO MapSupplier(ItContract contract)
        {
            return new ()
            {
                SignedBy = contract.SupplierContractSigner,
                Signed = contract.HasSupplierSigned,
                SignedAt = contract.SupplierSignedDate,
                SupplierOrganization = contract.Supplier?.MapShallowOrganizationResponseDTO()
            };
        }

        private ContractProcurementDataResponseDTO MapProcurement(ItContract contract)
        {
            return new()
            {
                ProcurementStrategy = contract.ProcurementStrategy?.MapIdentityNamePairDTO(),
                PurchaseType = contract.PurchaseForm?.MapIdentityNamePairDTO(),
                ProcurementPlan = MapProcurementPlan(contract)
            };
        }

        private ProcurementPlanDTO MapProcurementPlan(ItContract contract)
        {
            //TODO: Fix
            throw new System.NotImplementedException();
        }

        private ContractGeneralDataResponseDTO MapGeneral(ItContract contract)
        {
            return new()
            {
                ContractId = contract.ItContractId,
                Notes = contract.Note,
                ContractTemplate = contract.ContractTemplate?.MapIdentityNamePairDTO(),
                ContractType = contract.ContractType?.MapIdentityNamePairDTO(),
                AgreementElements = contract.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType?.MapIdentityNamePairDTO()).ToList(),
                Validity = new ValidityResponseDTO
                {
                    EnforcedValid = contract.Active,
                    Valid = contract.IsActive,
                    ValidFrom = contract.Concluded,
                    ValidTo = contract.ExpirationDate
                }
            };
        }

        private static ExternalReferenceDataDTO MapExternalReferenceDTO(ItContract contract, ExternalReference reference)
        {
            return new()
            {
                DocumentId = reference.ExternalReferenceId,
                Title = reference.Title,
                Url = reference.URL,
                MasterReference = contract.Reference?.Id.Equals(reference.Id) == true
            };
        }

        private static RoleAssignmentResponseDTO ToRoleResponseDTO(ItContractRight right)
        {
            return new()
            {
                Role = right.Role.MapIdentityNamePairDTO(),
                User = right.User.MapIdentityNamePairDTO()
            };
        }
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class ItContractDTO
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string ItContractId { get; set; }
        public string Esdh { get; set; }
        public string Folder { get; set; }
        public string SupplierContractSigner { get; set; }
        public bool HasSupplierSigned { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? SupplierSignedDate { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? OperationTestExpected { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? OperationTestApproved { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? OperationalAcceptanceTestExpected { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? OperationalAcceptanceTestApproved { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? Concluded { get; set; }
        public int Duration { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? IrrevocableTo { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? ExpirationDate { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? OperationRemunerationBegun { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? Terminated { get; set; }
        public int ExtendMultiplier { get; set; }

        public int? TerminationDeadlineId { get; set; }
        public int? PaymentFreqencyId { get; set; }
        public int? PaymentModelId { get; set; }
        public int? PriceRegulationId { get; set; }
        public int? OptionExtendId { get; set; }
        public int? ContractSignerId { get; set; }
        public UserDTO ContractSigner { get; set; }

        public bool IsSigned { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? SignedDate { get; set; }
        public int? ResponsibleOrganizationUnitId { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int? ProcurementStrategyId { get; set; }
        public int? ProcurementPlanHalf { get; set; }
        public int? ProcurementPlanYear { get; set; }
        public int? ContractTemplateId { get; set; }
        public int? ContractTypeId { get; set; }
        public int? PurchaseFormId { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public IEnumerable<OptionDTO> AgreementElements { get; set; }

        public IEnumerable<ItSystemUsageSimpleDTO> AssociatedSystemUsages { get; set; }
        public IEnumerable<InterfaceUsageDTO> AssociatedInterfaceUsages { get; set; }
        public IEnumerable<InterfaceExposureDTO> AssociatedInterfaceExposures { get; set; } 

        public IEnumerable<EconomyStreamDTO> InternEconomyStreams { get; set; }
        public IEnumerable<EconomyStreamDTO> ExternEconomyStreams { get; set; }

        public IEnumerable<AdviceDTO> Advices { get; set; }
        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }

        public string ObjectOwnerName { get; set; }
        public string ObjectOwnerLastName { get; set; }
        public string ObjectOwnerFullName
        {
            get { return ObjectOwnerName + " " + ObjectOwnerLastName; }
        }
    }
}

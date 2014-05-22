using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UI.MVC4.Filters;

namespace UI.MVC4.Models
{
    public class ItContractDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string ItContractId { get; set; }
        public string Esdh { get; set; }
        public string Folder { get; set; }
        public string SupplierContractSigner { get; set; }
        public bool HasSupplierSigned { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? SupplierSignedDate { get; set; }
        public UserDTO ContractSigner { get; set; }
        public bool IsSigned { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? SignedDate { get; set; }
        public int ResponsibleOrganizationUnitId { get; set; }
        public int? SupplierId { get; set; }
        public int? ProcurementStrategyId { get; set; }
        public int? ProcurementPlanId { get; set; }
        public int? ContractTemplateId { get; set; }
        public int? ContractTypeId { get; set; }
        public int? PurchaseFormId { get; set; }
        public int? ParentId { get; set; }
        public ICollection<OptionDTO> AgreementElements { get; set; }
        public ICollection<CustomAgreementElementDTO> CustomAgreementElements { get; set; }
    }
}
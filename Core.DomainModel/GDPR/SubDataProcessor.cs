using Core.DomainModel.Shared;

namespace Core.DomainModel.GDPR
{
    public class SubDataProcessor : Entity
    {
        public SubDataProcessor()
        {
        }

        public int SubDataProcessorOrganizationId { get; set; }
        public virtual Organization.Organization SubDataProcessorOrganization { get; set; }
        public int? SubDataProcessorBasisForTransferId { get; set; }
        public virtual DataProcessingBasisForTransferOption SubDataProcessorBasisForTransfer { get; set; }
        public YesNoUndecidedOption? TransferToInsecureCountries { get; set; }
        public int? InsecureCountryId { get; set; }
        public virtual DataProcessingCountryOption InsecureCountry { get; set; }
        public int? DataProcessingRegistrationId { get; set; }
        public virtual DataProcessingRegistration DataProcessingRegistration { get; set; }
    }
}

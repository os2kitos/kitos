using Core.Abstractions.Types;
using Core.DomainModel.Shared;

namespace Core.DomainModel.GDPR
{
    public class SubDataProcessor : Entity, IOwnedByOrganization
    {
        public SubDataProcessor()
        {
        }

        public int OrganizationId { get; set; }
        public virtual Organization.Organization Organization { get; set; }
        public int? SubDataProcessorBasisForTransferId { get; set; }
        public virtual DataProcessingBasisForTransferOption SubDataProcessorBasisForTransfer { get; set; }
        public YesNoUndecidedOption? TransferToInsecureCountry { get; set; }
        public int? InsecureCountryId { get; set; }
        public virtual DataProcessingCountryOption InsecureCountry { get; set; }
        public int? DataProcessingRegistrationId { get; set; }
        public virtual DataProcessingRegistration DataProcessingRegistration { get; set; }

        public void UpdateBasisForTransfer(Maybe<DataProcessingBasisForTransferOption> basisForTransfer)
        {
            SubDataProcessorBasisForTransfer = basisForTransfer.GetValueOrDefault();
        }

        public Maybe<OperationError> UpdateTransferToInsecureThirdCountries(YesNoUndecidedOption? transferToInsecureThirdCountry, Maybe<DataProcessingCountryOption> insecureCountry)
        {
            if (insecureCountry.HasValue && transferToInsecureThirdCountry != YesNoUndecidedOption.Yes)
            {
                return new OperationError($"{nameof(insecureCountry)} be provided if {nameof(transferToInsecureThirdCountry)} does not equal {nameof(YesNoUndecidedOption.Yes)}", OperationFailure.BadInput);
            }

            TransferToInsecureCountry = transferToInsecureThirdCountry;
            InsecureCountry = insecureCountry.GetValueOrDefault();
            return Maybe<OperationError>.None;
        }
    }
}

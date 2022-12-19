using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.GDPR.SubDataProcessor.Write
{
    public class TransferToInsecureCountryParameters
    {
        public YesNoUndecidedOption? Transfer { get; }
        public int? InsecureCountryOptionId { get; }

    }
}

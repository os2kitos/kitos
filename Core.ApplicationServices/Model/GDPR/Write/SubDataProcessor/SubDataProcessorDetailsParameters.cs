namespace Core.ApplicationServices.Model.GDPR.Write.SubDataProcessor
{
    public class SubDataProcessorDetailsParameters
    {
        public int? BasisForTransferOptionId { get; }
        public TransferToInsecureCountryParameters InsecureCountryParameters { get; }

        public SubDataProcessorDetailsParameters(int? basisForTransferOptionId, TransferToInsecureCountryParameters insecureCountryParameters)
        {
            BasisForTransferOptionId = basisForTransferOptionId;
            InsecureCountryParameters = insecureCountryParameters;
        }
    }
}

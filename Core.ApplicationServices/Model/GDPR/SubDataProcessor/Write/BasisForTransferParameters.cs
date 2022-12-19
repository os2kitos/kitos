namespace Core.ApplicationServices.Model.GDPR.SubDataProcessor.Write
{
    public class BasisForTransferParameters
    {
        public int? BasisForTransferOptionId { get; }
        public TransferToInsecureCountryParameters InsecureCountryParameters { get; }

        public BasisForTransferParameters(int? basisForTransferOptionId, TransferToInsecureCountryParameters insecureCountryParameters)
        {
            BasisForTransferOptionId = basisForTransferOptionId;
            InsecureCountryParameters = insecureCountryParameters;
        }
    }
}

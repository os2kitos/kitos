using System;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class SubDataProcessorParameter
    {
        public Guid OrganizationUuid { get; }
        public Guid? BasisForTransferOptionUuid { get; }
        public YesNoUndecidedOption? TransferToInsecureThirdCountry { get; }
        public Guid? InsecureCountrySubjectToDataTransferUuid { get; }

        public SubDataProcessorParameter(
            Guid organizationUuid, 
            Guid? basisForTransferOptionUuid, 
            YesNoUndecidedOption? transferToInsecureThirdCountry, 
            Guid? insecureCountrySubjectToDataTransferUuid)
        {
            OrganizationUuid = organizationUuid;
            BasisForTransferOptionUuid = basisForTransferOptionUuid;
            TransferToInsecureThirdCountry = transferToInsecureThirdCountry;
            InsecureCountrySubjectToDataTransferUuid = insecureCountrySubjectToDataTransferUuid;
        }
    }
}

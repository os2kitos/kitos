using System;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class SubDataProcessorParameter
    {
        public Guid OrganizationUuid { get; set; }
        public Guid? BasisForTransferOptionUuid { get; set; }
        public YesNoUndecidedOption? TransferToInsecureThirdCountry { get; set; }
        public Guid? InsecureCountrySubjectToDataTransferUuid { get; set; }
    }
}

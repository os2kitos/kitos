﻿using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.GDPR.Write.SubDataProcessor
{
    public class TransferToInsecureCountryParameters
    {
        public YesNoUndecidedOption? Transfer { get; }
        public int? InsecureCountryOptionId { get; }

        public TransferToInsecureCountryParameters(YesNoUndecidedOption? transfer, int? insecureCountryOptionId)
        {
            Transfer = transfer;
            InsecureCountryOptionId = insecureCountryOptionId;
        }
    }
}

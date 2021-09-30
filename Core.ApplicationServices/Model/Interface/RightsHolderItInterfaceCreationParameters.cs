using Core.ApplicationServices.Model.Shared;
using System;

namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceCreationParameters
    {
        public Guid? RightsHolderProvidedUuid { get; }
        public RightsHolderItInterfaceUpdateParameters AdditionalValues { get; }

        public RightsHolderItInterfaceCreationParameters(Guid? rightsHolderProvidedUuid, RightsHolderItInterfaceUpdateParameters additionalValues)
        {
            RightsHolderProvidedUuid = rightsHolderProvidedUuid;
            AdditionalValues = additionalValues;
        }
    }
}
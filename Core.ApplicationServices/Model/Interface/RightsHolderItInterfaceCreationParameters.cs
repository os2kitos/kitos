using System;

namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceCreationParameters : RightsHolderItInterfaceParameters
    {
        public Guid? RightsHolderProvidedUuid { get; }

        public RightsHolderItInterfaceCreationParameters(Guid? uuid, string name, string interfaceId, string version, string description, string urlReference)
            : base(name, interfaceId, version, description, urlReference)
        {
            RightsHolderProvidedUuid = uuid;
        }
    }
}
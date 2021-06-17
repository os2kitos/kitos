using System;

namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceCreationParameters
    {
        public Guid? RightsHolderProvidedUuid { get; }
        public string Name { get; }
        public string InterfaceId { get; }
        public string Version { get; }
        public string Description { get; }
        public string UrlReference { get; }

        public RightsHolderItInterfaceCreationParameters(Guid? uuid, string name, string interfaceId, string version, string description, string urlReference)
        {
            RightsHolderProvidedUuid = uuid;
            Name = name;
            InterfaceId = interfaceId;
            Version = version;
            Description = description;
            UrlReference = urlReference;
        }
    }
}
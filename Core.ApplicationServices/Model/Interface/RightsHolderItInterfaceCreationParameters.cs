using System;

namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceCreationParameters : IRightsHolderWriteableItInterfaceParameters
    {
        public Guid? RightsHolderProvidedUuid { get; }
        public Guid ExposingSystemUuid { get; }
        public string Name { get; }
        public string InterfaceId { get; }
        public string Version { get; }
        public string Description { get; }
        public string UrlReference { get; }

        public RightsHolderItInterfaceCreationParameters(Guid? rightsHolderProvidedUuid, Guid exposingSystemUuid, string name, string interfaceId, string version, string description, string urlReference)
        {
            RightsHolderProvidedUuid = rightsHolderProvidedUuid;
            ExposingSystemUuid = exposingSystemUuid;
            Name = name;
            InterfaceId = interfaceId;
            Version = version;
            Description = description;
            UrlReference = urlReference;
        }
    }
}
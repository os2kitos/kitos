using System;

namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceUpdateParameters : IRightsHolderWriteableItInterfaceParameters
    {
        public Guid ExposingSystemUuid { get; }
        public string Name { get; }
        public string InterfaceId { get; }
        public string Version { get; }
        public string Description { get; }
        public string UrlReference { get; }

        public RightsHolderItInterfaceUpdateParameters(Guid exposingSystemUuid, string name, string interfaceId, string version, string description, string urlReference)
        {
            ExposingSystemUuid = exposingSystemUuid;
            Name = name;
            InterfaceId = interfaceId;
            Version = version;
            Description = description;
            UrlReference = urlReference;
        }
    }
}

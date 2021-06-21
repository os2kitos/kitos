using System;

namespace Core.ApplicationServices.Model.Interface
{
    public interface IRightsHolderWriteableItInterfaceParameters
    {
        public Guid ExposingSystemUuid { get; }
        public string Name { get; }
        public string InterfaceId { get; }
        public string Version { get; }
        public string Description { get; }
        public string UrlReference { get; }
    }
}

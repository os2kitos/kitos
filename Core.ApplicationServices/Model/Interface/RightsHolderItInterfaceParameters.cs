namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceParameters
    {
        public string Name { get; }
        public string InterfaceId { get; }
        public string Version { get; }
        public string Description { get; }
        public string UrlReference { get; }

        public RightsHolderItInterfaceParameters(string name, string interfaceId, string version, string description, string urlReference)
        {
            Name = name;
            InterfaceId = interfaceId;
            Version = version;
            Description = description;
            UrlReference = urlReference;
        }
    }
}

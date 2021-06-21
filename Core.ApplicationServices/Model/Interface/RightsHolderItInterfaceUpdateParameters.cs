namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceUpdateParameters : RightsHolderItInterfaceParameters
    {

        public RightsHolderItInterfaceUpdateParameters(string name, string interfaceId, string version, string description, string urlReference)
            : base(name, interfaceId, version, description, urlReference)
        {

        }
    }
}

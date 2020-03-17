namespace Core.ApplicationServices.SSO
{
    public class SsoFlowConfiguration
    {
        public string SamlEntityId { get; }

        public SsoFlowConfiguration(string samlEntityId)
        {
            SamlEntityId = samlEntityId;
        }
    }
}

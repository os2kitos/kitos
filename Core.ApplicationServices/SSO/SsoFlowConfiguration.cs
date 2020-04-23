using System;

namespace Core.ApplicationServices.SSO
{
    public class SsoFlowConfiguration
    {
        public string PrivilegePrefix { get; }

        public SsoFlowConfiguration(string samlEntityId)
        {
            //NOTE: STS adgangsstyring adds http in front of priviliges in stead of our entity id's https
            PrivilegePrefix = new Uri(samlEntityId).Host;
        }
    }
}

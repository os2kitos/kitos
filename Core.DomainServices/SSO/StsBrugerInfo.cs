using System.Collections.Generic;

namespace Core.DomainServices.SSO
{
    public class StsBrugerInfo
    {
        public IEnumerable<string> Emails { get; set; }
        public string BelongsToOrganizationUuid { get; set; }
        public string MunicipalityCvr { get; set; }

        public StsBrugerInfo(IEnumerable<string> emails, string belongsToOrganizationUuid, string municipalityCvr)
        {
            Emails = emails;
            BelongsToOrganizationUuid = belongsToOrganizationUuid;
            MunicipalityCvr = municipalityCvr;
        }
    }
}
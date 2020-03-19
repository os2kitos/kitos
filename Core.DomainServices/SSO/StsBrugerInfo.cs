using System;
using System.Collections.Generic;

namespace Core.DomainServices.SSO
{
    public class StsBrugerInfo
    {
        public Guid Uuid { get; }
        public IEnumerable<string> Emails { get; }
        public string BelongsToOrganizationUuid { get; }
        public string MunicipalityCvr { get; }

        public StsBrugerInfo(Guid uuid, IEnumerable<string> emails, string belongsToOrganizationUuid, string municipalityCvr)
        {
            Uuid = uuid;
            Emails = emails;
            BelongsToOrganizationUuid = belongsToOrganizationUuid;
            MunicipalityCvr = municipalityCvr;
        }
    }
}
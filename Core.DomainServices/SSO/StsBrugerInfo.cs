using System;
using System.Collections.Generic;

namespace Core.DomainServices.SSO
{
    public class StsBrugerInfo
    {
        public Guid Uuid { get; }
        public Guid BelongsToOrganizationUuid { get; }
        public IEnumerable<string> Emails { get; }
        public string MunicipalityCvr { get; }

        public StsBrugerInfo(Guid uuid, IEnumerable<string> emails, Guid belongsToOrganizationUuid, string municipalityCvr)
        {
            Uuid = uuid;
            Emails = emails;
            BelongsToOrganizationUuid = belongsToOrganizationUuid;
            MunicipalityCvr = municipalityCvr;
        }
    }
}
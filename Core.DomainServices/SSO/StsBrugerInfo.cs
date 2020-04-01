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
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public StsBrugerInfo(Guid uuid, IEnumerable<string> emails, Guid belongsToOrganizationUuid, string municipalityCvr, string firstName, string lastName)
        {
            Uuid = uuid;
            Emails = emails;
            BelongsToOrganizationUuid = belongsToOrganizationUuid;
            MunicipalityCvr = municipalityCvr;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
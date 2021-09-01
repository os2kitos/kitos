using System;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageCreationParameters
    {
        public Guid SystemUuid { get; }
        public Guid OrganizationUuid { get; }

        public SystemUsageUpdateParameters AdditionalValues { get; }

        public SystemUsageCreationParameters(Guid systemUuid, Guid organizationUuid, SystemUsageUpdateParameters additionalValues)
        {
            SystemUuid = systemUuid;
            OrganizationUuid = organizationUuid;
            AdditionalValues = additionalValues;
        }
    }
}

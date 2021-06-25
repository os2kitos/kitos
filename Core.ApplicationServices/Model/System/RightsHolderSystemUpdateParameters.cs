using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.System
{
    public class RightsHolderSystemUpdateParameters : IRightsHolderWritableSystemProperties
    {
        public string Name { get; }
        public Guid? ParentSystemUuid { get; }
        public string FormerName { get; }
        public string Description { get; }
        public string UrlReference { get; }
        public Guid? BusinessTypeUuid { get; }
        public IEnumerable<string> TaskRefKeys { get; }
        public IEnumerable<Guid> TaskRefUuids { get; }

        public RightsHolderSystemUpdateParameters(
            string name,
            Guid? parentSystemUuid,
            string formerName,
            string description,
            string urlReference,
            Guid? businessTypeUuid,
            IEnumerable<string> taskRefKeys,
            IEnumerable<Guid> taskRefUuids)
        {
            Name = name;
            ParentSystemUuid = parentSystemUuid;
            FormerName = formerName;
            Description = description;
            UrlReference = urlReference;
            BusinessTypeUuid = businessTypeUuid;
            TaskRefKeys = taskRefKeys;
            TaskRefUuids = taskRefUuids;
        }
    }
}

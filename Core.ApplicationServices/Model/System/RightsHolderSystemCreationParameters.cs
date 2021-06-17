using System;
using System.Collections.Generic;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.System
{
    public class RightsHolderSystemCreationParameters
    {
        public string Name { get; }
        public Guid? RightsHolderProvidedUuid { get; }
        public Guid? ParentSystemUuid { get; }
        public string FormerName { get; }
        public string Description { get; }
        public string UrlReference { get; }
        public Guid? BusinessTypeUuid { get; }
        public IEnumerable<string> TaskRefKeys { get; }
        public IEnumerable<Guid> TaskRefUuids { get; }

        public RightsHolderSystemCreationParameters(
            string name,
            Guid? rightsHolderProvidedUuid,
            Guid? parentSystemUuid,
            string formerName,
            string description,
            string urlReference,
            Guid? businessTypeUuid,
            IEnumerable<string> taskRefKeys,
            IEnumerable<Guid> taskRefUuids)
        {
            Name = name;
            RightsHolderProvidedUuid = rightsHolderProvidedUuid;
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

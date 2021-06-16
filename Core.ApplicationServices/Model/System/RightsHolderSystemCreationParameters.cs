using System;
using System.Collections.Generic;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.System
{
    public class RightsHolderSystemCreationParameters
    {
        public string Name { get; }
        public Maybe<Guid> RightsHolderProvidedUuid { get; }
        public Maybe<Guid> ParentSystemUuid { get; }
        public Maybe<string> FormerName { get; }
        public string Description { get; }
        public string UrlReference { get; }
        public Maybe<Guid> BusinessTypeUuid { get; }
        public Maybe<IEnumerable<string>> TaskRefKeys { get; }
        public Maybe<IEnumerable<Guid>> TaskRefUuids { get; }

        public RightsHolderSystemCreationParameters(
            string name,
            Maybe<Guid> rightsHolderProvidedUuid,
            Maybe<Guid> parentSystemUuid,
            Maybe<string> formerName,
            string description,
            string urlReference,
            Maybe<Guid> businessTypeUuid,
            Maybe<IEnumerable<string>> taskRefKeys,
            Maybe<IEnumerable<Guid>> taskRefUuids)
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

using System;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemRelationParameters
    {
        public Guid ToSystemUsageUuid { get; }
        public Guid? UsingInterfaceUuid { get; }
        public Guid? AssociatedContractUuid { get; }
        public Guid? RelationFrequencyUuid { get; }
        public string Description { get; }
        public string UrlReference { get; }

        public SystemRelationParameters(
            Guid toSystemUsageUuid,
            Guid? usingInterfaceUuid,
            Guid? associatedContractUuid,
            Guid? relationFrequencyUuid,
            string description,
            string urlReference)
        {
            ToSystemUsageUuid = toSystemUsageUuid;
            UsingInterfaceUuid = usingInterfaceUuid;
            AssociatedContractUuid = associatedContractUuid;
            RelationFrequencyUuid = relationFrequencyUuid;
            Description = description;
            UrlReference = urlReference;
        }
    }
}

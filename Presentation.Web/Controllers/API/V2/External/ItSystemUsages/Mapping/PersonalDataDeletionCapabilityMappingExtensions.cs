using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class PersonalDataDeletionCapabilityMappingExtensions
    {
        private static readonly EnumMap<PersonalDataDeletionCapabilityChoice, PersonalDataDeletionCapability> Mapping = new(
                (PersonalDataDeletionCapabilityChoice.No, PersonalDataDeletionCapability.No),
                (PersonalDataDeletionCapabilityChoice.IndividualRecords, PersonalDataDeletionCapability.IndividualRecords),
                (PersonalDataDeletionCapabilityChoice.BulkDeletion, PersonalDataDeletionCapability.BulkDeletion));

        public static PersonalDataDeletionCapability ToDomain(this PersonalDataDeletionCapabilityChoice value) => Mapping.FromLeftToRight(value);
        public static PersonalDataDeletionCapabilityChoice ToChoice(this PersonalDataDeletionCapability value) => Mapping.FromRightToLeft(value);
    }
}

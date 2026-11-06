using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class PersonalDataDeletionMethodMappingExtensions
    {
        private static readonly EnumMap<PersonalDataDeletionMethodChoice, PersonalDataDeletionMethod> Mapping = new(
                (PersonalDataDeletionMethodChoice.AutomaticallyBySystem, PersonalDataDeletionMethod.AutomaticallyBySystem),
                (PersonalDataDeletionMethodChoice.ManuallyByOrganization, PersonalDataDeletionMethod.ManuallyByOrganization),
                (PersonalDataDeletionMethodChoice.BySupplierOnRequest, PersonalDataDeletionMethod.BySupplierOnRequest));

        public static PersonalDataDeletionMethod ToDomain(this PersonalDataDeletionMethodChoice value) => Mapping.FromLeftToRight(value);
        public static PersonalDataDeletionMethodChoice ToChoice(this PersonalDataDeletionMethod value) => Mapping.FromRightToLeft(value);
    }
}

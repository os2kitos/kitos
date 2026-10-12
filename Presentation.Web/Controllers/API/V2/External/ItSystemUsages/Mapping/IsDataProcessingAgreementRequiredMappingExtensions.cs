using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class IsDataProcessingAgreementRequiredMappingExtensions
    {
        private static readonly EnumMap<IsDataProcessingAgreementRequiredChoice, IsDataProcessingAgreementRequired> Mapping;

        static IsDataProcessingAgreementRequiredMappingExtensions()
        {
            Mapping = new EnumMap<IsDataProcessingAgreementRequiredChoice, IsDataProcessingAgreementRequired>
            (
                (IsDataProcessingAgreementRequiredChoice.No, IsDataProcessingAgreementRequired.No),
                (IsDataProcessingAgreementRequiredChoice.Yes, IsDataProcessingAgreementRequired.Yes),
                (IsDataProcessingAgreementRequiredChoice.DecidedByLaw, IsDataProcessingAgreementRequired.DecidedByLaw)
            );
        }

        public static IsDataProcessingAgreementRequired FromChoice(this IsDataProcessingAgreementRequiredChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static IsDataProcessingAgreementRequiredChoice ToChoice(this IsDataProcessingAgreementRequired value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}

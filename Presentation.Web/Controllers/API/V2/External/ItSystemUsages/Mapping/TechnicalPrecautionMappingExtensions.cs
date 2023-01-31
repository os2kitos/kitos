using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class TechnicalPrecautionMappingExtensions
    {
        private static readonly EnumMap<TechnicalPrecautionChoice, TechnicalPrecaution> Mapping;

        static TechnicalPrecautionMappingExtensions()
        {
            Mapping = new EnumMap<TechnicalPrecautionChoice, TechnicalPrecaution>
            (
                (TechnicalPrecautionChoice.AccessControl, TechnicalPrecaution.AccessControl),
                (TechnicalPrecautionChoice.Encryption, TechnicalPrecaution.Encryption),
                (TechnicalPrecautionChoice.Logging, TechnicalPrecaution.Logging),
                (TechnicalPrecautionChoice.Pseudonymization, TechnicalPrecaution.Pseudonymization)
            );
        }

        public static TechnicalPrecaution ToTechnicalPrecaution(this TechnicalPrecautionChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static TechnicalPrecautionChoice ToTechnicalPrecautionChoice(this TechnicalPrecaution value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
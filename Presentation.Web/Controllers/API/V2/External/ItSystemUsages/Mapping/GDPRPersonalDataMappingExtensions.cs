using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class GDPRPersonalDataMappingExtensions
    {

        private static readonly EnumMap<GDPRPersonalDataChoice, GDPRPersonalDataOption> Mapping;

        static GDPRPersonalDataMappingExtensions()
        {
            Mapping = new EnumMap<GDPRPersonalDataChoice, GDPRPersonalDataOption>
            (
                (GDPRPersonalDataChoice.CprNumber, GDPRPersonalDataOption.CprNumber),
                (GDPRPersonalDataChoice.OtherPrivateMatters, GDPRPersonalDataOption.OtherPrivateMatters),
                (GDPRPersonalDataChoice.SocialProblems, GDPRPersonalDataOption.SocialProblems)
            );
        }

        public static GDPRPersonalDataOption ToGDPRPersonalDataOption(this GDPRPersonalDataChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static GDPRPersonalDataChoice ToGDPRPersonalDataChoice(this GDPRPersonalDataOption value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
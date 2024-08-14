using Core.Abstractions.Types;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public static class OversightIntervalMappingExtensions
    {
        private static readonly EnumMap<OversightIntervalChoice, YearMonthIntervalOption> Mapping;
        static OversightIntervalMappingExtensions()
        {
            Mapping = new EnumMap<OversightIntervalChoice, YearMonthIntervalOption>
            (
                (OversightIntervalChoice.BiYearly, YearMonthIntervalOption.Half_yearly),
                (OversightIntervalChoice.Yearly, YearMonthIntervalOption.Yearly),
                (OversightIntervalChoice.EveryOtherYear, YearMonthIntervalOption.Every_second_year),
                (OversightIntervalChoice.Other, YearMonthIntervalOption.Other),
                (OversightIntervalChoice.Undecided, YearMonthIntervalOption.Undecided)
            );
        }

        public static YearMonthIntervalOption ToIntervalOption(this OversightIntervalChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static OversightIntervalChoice ToIntervalChoice(this YearMonthIntervalOption value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
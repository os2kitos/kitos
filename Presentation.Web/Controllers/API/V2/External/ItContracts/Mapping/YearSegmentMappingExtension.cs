using Core.Abstractions.Types;
using Core.DomainModel.ItContract;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public static class YearSegmentMappingExtension
    {
        private static readonly EnumMap<YearSegmentChoice, YearSegmentOption> Mapping;
        static YearSegmentMappingExtension()
        {
            Mapping = new EnumMap<YearSegmentChoice, YearSegmentOption>
            (
                (YearSegmentChoice.EndOfCalendarYear, YearSegmentOption.EndOfCalendarYear),
                (YearSegmentChoice.EndOfQuarter, YearSegmentOption.EndOfQuarter),
                (YearSegmentChoice.EndOfMonth, YearSegmentOption.EndOfMonth)
            );
        }

        public static YearSegmentOption ToYearSegmentOption(this YearSegmentChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static YearSegmentChoice ToYearSegmentChoice(this YearSegmentOption value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
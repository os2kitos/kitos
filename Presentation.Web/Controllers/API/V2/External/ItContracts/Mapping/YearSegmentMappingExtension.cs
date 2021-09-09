using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public static class YearSegmentMappingExtension
    {
        private static readonly IReadOnlyDictionary<YearSegmentChoice, YearSegmentOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<YearSegmentOption, YearSegmentChoice> DataToApiMap;

        static YearSegmentMappingExtension()
        {
            ApiToDataMap = new Dictionary<YearSegmentChoice, YearSegmentOption>
            {
                { YearSegmentChoice.EndOfCalendarYear, YearSegmentOption.EndOfCalendarYear },
                { YearSegmentChoice.EndOfQuarter, YearSegmentOption.EndOfQuarter },
                { YearSegmentChoice.EndOfMonth, YearSegmentOption.EndOfMonth }
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static YearSegmentOption ToYearSegmentOption(this YearSegmentChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static YearSegmentChoice ToYearSegmentChoice(this YearSegmentOption value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public static class OversightIntervalMappingExtensions
    {
        private static readonly IReadOnlyDictionary<OversightIntervalChoice, YearMonthIntervalOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<YearMonthIntervalOption, OversightIntervalChoice> DataToApiMap;

        static OversightIntervalMappingExtensions()
        {
            ApiToDataMap = new Dictionary<OversightIntervalChoice, YearMonthIntervalOption>
            {
                { OversightIntervalChoice.BiYearly, YearMonthIntervalOption.Half_yearly },
                { OversightIntervalChoice.Yearly, YearMonthIntervalOption.Yearly },
                { OversightIntervalChoice.EveryOtherYear, YearMonthIntervalOption.Every_second_year },
                { OversightIntervalChoice.Other, YearMonthIntervalOption.Other },
                { OversightIntervalChoice.Undecided, YearMonthIntervalOption.Undecided }
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static YearMonthIntervalOption ToIntervalOption(this OversightIntervalChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static OversightIntervalChoice ToIntervalChoice(this YearMonthIntervalOption value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}
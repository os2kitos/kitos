using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public static class YesNoIrrelevantMappingExtensions
    {
        private static readonly IReadOnlyDictionary<YesNoIrrelevantChoice, YesNoIrrelevantOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<YesNoIrrelevantOption, YesNoIrrelevantChoice> DataToApiMap;

        static YesNoIrrelevantMappingExtensions()
        {
            ApiToDataMap = new Dictionary<YesNoIrrelevantChoice, YesNoIrrelevantOption>
            {
                { YesNoIrrelevantChoice.Yes, YesNoIrrelevantOption.YES },
                { YesNoIrrelevantChoice.No, YesNoIrrelevantOption.NO },
                { YesNoIrrelevantChoice.Irrelevant, YesNoIrrelevantOption.IRRELEVANT },
                { YesNoIrrelevantChoice.Undecided, YesNoIrrelevantOption.UNDECIDED }
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static YesNoIrrelevantOption ToDataOptions(this YesNoIrrelevantChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static YesNoIrrelevantChoice ToYesNoDontKnowChoice(this YesNoIrrelevantOption value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public static class YesNoUndecidedMappingExtensions
    {
        private static readonly IReadOnlyDictionary<YesNoUndecidedChoice, YesNoUndecidedOption> ApiToDataMap;
        private static readonly IReadOnlyDictionary<YesNoUndecidedOption, YesNoUndecidedChoice> DataToApiMap;

        static YesNoUndecidedMappingExtensions()
        {
            ApiToDataMap = new Dictionary<YesNoUndecidedChoice, YesNoUndecidedOption>
            {
                { YesNoUndecidedChoice.Yes, YesNoUndecidedOption.Yes },
                { YesNoUndecidedChoice.No, YesNoUndecidedOption.No },
                { YesNoUndecidedChoice.Undecided, YesNoUndecidedOption.Undecided }
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static YesNoUndecidedOption ToYesNoUndecidedOption(this YesNoUndecidedChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static YesNoUndecidedChoice ToYesNoUndecidedChoice(this YesNoUndecidedOption value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.Result;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class DataOptionsMappingExtensions
    {
        private static readonly IReadOnlyDictionary<YesNoDontKnowChoice, DataOptions> apiToDataMap;
        private static readonly IReadOnlyDictionary<DataOptions, YesNoDontKnowChoice> dataToApiMap;

        static DataOptionsMappingExtensions()
        {
            apiToDataMap = new Dictionary<YesNoDontKnowChoice, DataOptions>()
            {
                { YesNoDontKnowChoice.DontKnow, DataOptions.DONTKNOW },
                { YesNoDontKnowChoice.Yes, DataOptions.YES },
                { YesNoDontKnowChoice.No, DataOptions.NO },
                { YesNoDontKnowChoice.Undecided, DataOptions.UNDECIDED }
            }.AsReadOnly();
            dataToApiMap = apiToDataMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key).AsReadOnly();
        }

        public static DataOptions ToDataOptions(this YesNoDontKnowChoice value)
        {
            return apiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static YesNoDontKnowChoice ToYesNoDontKnowChoice(this DataOptions value)
        {
            return dataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class SensitiveDataLevelMappingExtensions
    {
        private static readonly IReadOnlyDictionary<DataSensitivityLevelChoice, SensitiveDataLevel> apiToDataMap;
        private static readonly IReadOnlyDictionary<SensitiveDataLevel, DataSensitivityLevelChoice> dataToApiMap;

        static SensitiveDataLevelMappingExtensions()
        {
            apiToDataMap = new Dictionary<DataSensitivityLevelChoice, SensitiveDataLevel>
            {
                { DataSensitivityLevelChoice.LegalData ,SensitiveDataLevel.LEGALDATA},
                { DataSensitivityLevelChoice.PersonData ,SensitiveDataLevel.PERSONALDATA},
                { DataSensitivityLevelChoice.SensitiveData ,SensitiveDataLevel.SENSITIVEDATA},
                { DataSensitivityLevelChoice.None,SensitiveDataLevel.NONE},
            }.AsReadOnly();
            dataToApiMap = apiToDataMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key).AsReadOnly();
        }

        public static SensitiveDataLevel ToSensitiveDataLevel(this DataSensitivityLevelChoice value)
        {
            return apiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static DataSensitivityLevelChoice ToDataSensitivityLevelChoice(this SensitiveDataLevel value)
        {
            return dataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}
using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class SensitiveDataLevelMappingExtensions
    {
        private static readonly EnumMap<DataSensitivityLevelChoice, SensitiveDataLevel> Mapping;

        static SensitiveDataLevelMappingExtensions()
        {
            Mapping = new EnumMap<DataSensitivityLevelChoice, SensitiveDataLevel>
            (
                (DataSensitivityLevelChoice.LegalData, SensitiveDataLevel.LEGALDATA),
                (DataSensitivityLevelChoice.PersonData, SensitiveDataLevel.PERSONALDATA),
                (DataSensitivityLevelChoice.SensitiveData, SensitiveDataLevel.SENSITIVEDATA),
                (DataSensitivityLevelChoice.None, SensitiveDataLevel.NONE)
            );
        }

        public static SensitiveDataLevel ToSensitiveDataLevel(this DataSensitivityLevelChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static DataSensitivityLevelChoice ToDataSensitivityLevelChoice(this SensitiveDataLevel value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
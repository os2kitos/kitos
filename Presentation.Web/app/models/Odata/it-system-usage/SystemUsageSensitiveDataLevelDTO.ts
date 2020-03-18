module Kitos.Models.Odata.ItSystemUsage {

    export interface IItSystemUsageSensitiveDataLevelDTO {
        Id: number;

        ItSystemUsage: Models.ItSystemUsage.IItSystemUsage;

        //Odata maps enums to string https://github.com/OData/WebApi/issues/364
        SensitivityDataLevel: string;
    }

    export class SensitiveDataLevelMapper {
        static readonly textValueToTextMap = Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.getTextValueToTextMap();
        static map(input: string) {
            return SensitiveDataLevelMapper.textValueToTextMap[input];
        }
    }
}
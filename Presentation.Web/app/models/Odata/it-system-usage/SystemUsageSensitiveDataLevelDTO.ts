module Kitos.Models.Odata.ItSystemUsage {

    export interface IItSystemUsageSensitiveDataLevelDTO {
        Id: number;

        ItSystemUsage: Models.ItSystemUsage.IItSystemUsage;

        //Odata maps enums to string https://github.com/OData/WebApi/issues/364
        SensitivityDataLevel: string;
    }

    export class SensitiveDataLevelMapper {
        static map(input: string) {
            return Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.getTextValueToTextMap()[input];
        }
    }
}
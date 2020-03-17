module Kitos.Models.Odata.ItSystemUsage {

    export interface IItSystemUsageSensitiveDataLevelDTO {
        Id: number;

        ItSystemUsage: Models.ItSystemUsage.IItSystemUsage;

        //Odata maps enums to string https://github.com/OData/WebApi/issues/364
        SensitivityDataLevel: string;
    }

    export class SensitiveDataLevelMapper {
        static map(input: string) {
            switch (input) {
                case "NONE":
                    return "Ingen persondata";
                case "PERSONALDATA":
                    return "Almindelige persondata";
                case "SENSITIVEDATA":
                    return "Følsomme persondata";
                case "LEGALDATA":
                    return "Straffedomme og lovovertrædelser";
            }
        }
    }
}
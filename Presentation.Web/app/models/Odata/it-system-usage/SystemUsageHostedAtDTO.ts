module Kitos.Models.Odata.ItSystemUsage {

    export interface IItSystemUsageHostedAtDTO {
        //Odata maps enums to string https://github.com/OData/WebApi/issues/364
        HostedAt: string;
    }

    export class HostedAtMapper {
        static map(input: string) {
            if (input != null) {
                return Models.ViewModel.ItSystemUsage.HostedAtViewModel.getTextValueToTextMap()[input];
            } else {
                return "";
            }
        }
    }
}
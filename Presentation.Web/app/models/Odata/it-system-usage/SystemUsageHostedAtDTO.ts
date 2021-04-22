module Kitos.Models.Odata.ItSystemUsage {

    export interface IItSystemUsageHostedAtDTO {
        //Odata maps enums to string https://github.com/OData/WebApi/issues/364
        HostedAt: number;
    }

    export class HostedAtMapper {
        static map(input: number) {
            if (input != null) {
                return Models.ViewModel.ItSystemUsage.HostedAtOptions.getoptionalObjectContextToTextMap()[input];
            } else {
                return "";
            }
        }
    }
}
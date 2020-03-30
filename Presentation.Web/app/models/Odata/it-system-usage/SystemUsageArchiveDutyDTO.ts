module Kitos.Models.Odata.ItSystemUsage {

    export interface IItSystemUsageArchiveDutyDTO {
        //Odata maps enums to string https://github.com/OData/WebApi/issues/364
        ArchiveDuty: string;
    }

    export class ArchiveDutyMapper {
        static map(input: string) {
            if (input != null) {
                return Models.ViewModel.ItSystemUsage.ArchiveDutyViewModel.getTextValueToTextMap()[input];
            } else {
                return "";
            }
        }
    }
}
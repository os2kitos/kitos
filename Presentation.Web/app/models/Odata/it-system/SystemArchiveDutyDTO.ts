module Kitos.Models.Odata.ItSystem {

    export interface IItSystemArchiveDutyRecommendationDTO {
        //Odata maps enums to string https://github.com/OData/WebApi/issues/364
        ArchiveDuty: string;
    }

    export class ArchiveDutyRecommendationMapper {
        static map(input: string) {
            if (input != null) {
                return Models.ViewModel.ItSystem.ArchiveDutyRecommendationViewModel.getTextValueToTextMap()[input];
            } else {
                return "";
            }
        }
    }
}
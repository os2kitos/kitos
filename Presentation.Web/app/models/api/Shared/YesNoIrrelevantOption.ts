module Kitos.Models.Api.Shared {
    export enum YesNoIrrelevantOption {
        NO = 0,
        YES = 1,
        IRRELEVANT = 2,
        UNDECIDED = 3
    }

    export class YesNoIrrelevantOptionMapper {
        static mapFromIdAsString(idAsString: string): YesNoIrrelevantOption {
            return parseInt(idAsString);
        }
    }
}
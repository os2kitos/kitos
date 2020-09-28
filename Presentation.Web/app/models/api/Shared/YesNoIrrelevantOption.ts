module Kitos.Models.Api.Shared {
    export enum YesNoIrrelevantOption {
        NO = 0,
        YES = 1,
        IRRELEVANT = 2,
        UNDECIDED = 3
    }

    export class YesNoIrrelevantOptionMapper {
        static getValueToTextMap() {
            return Object
                .keys(YesNoIrrelevantOption)
                .filter(k=>isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                    const text = Models.ViewModel.Shared.YesNoIrrelevantOptions.getText(parseInt(next));

                    //Set by numeric and text value
                    acc[next] = text;
                    acc[YesNoIrrelevantOption[next]] = text;
                    return acc;
                },
                    {}
                );
        }
    }
}
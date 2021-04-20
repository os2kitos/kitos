module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YesNoIrrelevantOptions {

        private static getValueToTextMap() {
            return Object
                .keys(Models.Api.Shared.YesNoIrrelevantOption)
                .filter(k => isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                        var text = "";

                        switch (parseInt(next) as Models.Api.Shared.YesNoIrrelevantOption) {
                        case Models.Api.Shared.YesNoIrrelevantOption.YES:
                            text = "Ja";
                            break;
                        case Models.Api.Shared.YesNoIrrelevantOption.NO:
                            text = "Nej";
                            break;
                        case Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT:
                            text = "Ikke relevant";
                            break;
                        }

                        //Set by numeric and text value
                        acc[next] = text;
                        acc[Models.Api.Shared.YesNoIrrelevantOption[next]] = text;
                        return acc;
                    },
                    {}
                );
        }

        //Cache the names for quick lookup
        private static readonly valueToNameMap = YesNoIrrelevantOptions.getValueToTextMap();

        static getText(option: Models.Api.Shared.YesNoIrrelevantOption) {
            return YesNoIrrelevantOptions.valueToNameMap[option];
        }

        options: Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>[];

        getById(id: number): Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption> {
            if (id === null) {
                return null;
            }
            return this.options.filter(x => x.id === id)[0];
        }

        constructor() {
            this.options = [
                Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED,
                Models.Api.Shared.YesNoIrrelevantOption.YES,
                Models.Api.Shared.YesNoIrrelevantOption.NO,
                Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT
            ].map(optionType => {
                return <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{
                    id: optionType as number,
                    text: optionType === Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED
                        ? ViewModel.Generic.select2BlankOptionTextValue
                        : YesNoIrrelevantOptions.getText(optionType),
                    optionalObjectContext: optionType
                };
            });
        }
    }
}
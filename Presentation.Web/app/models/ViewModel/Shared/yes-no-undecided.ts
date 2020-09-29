module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YesNoUndecidedOptions {

        private static getValueToTextMap() {
            return Object
                .keys(Models.Api.Shared.YesNoUndecidedOption)
                .filter(k => isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                    var text = "";

                    switch (parseInt(next) as Models.Api.Shared.YesNoUndecidedOption) {
                        case Models.Api.Shared.YesNoUndecidedOption.Yes:
                            text = "Ja";
                            break;
                        case Models.Api.Shared.YesNoUndecidedOption.No:
                            text = "Nej";
                            break;
                    }

                    //Set by numeric and text value
                    acc[next] = text;
                    acc[Models.Api.Shared.YesNoUndecidedOption[next]] = text;
                    return acc;
                },
                    {}
                );
        }

        //Cache texts for quick lookup texts
        private static readonly valueToTextMap = YesNoUndecidedOptions.getValueToTextMap();

        static getText(option: Models.Api.Shared.YesNoUndecidedOption) {
            return YesNoUndecidedOptions.valueToTextMap[option];
        }

        getById(id?: number): Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption> {
            if (id === null) {
                return null;
            }
            return this.options.filter(x => x.id === id)[0];
        }

        options: Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>[];
        constructor() {
            this.options = [
                Models.Api.Shared.YesNoUndecidedOption.Undecided,
                Models.Api.Shared.YesNoUndecidedOption.Yes,
                Models.Api.Shared.YesNoUndecidedOption.No
            ].map(optionType => {
                return <Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>>{
                    id: optionType as number,
                    text: optionType === Models.Api.Shared.YesNoUndecidedOption.Undecided
                        ? ViewModel.Generic.select2BlankOptionTextValue
                        : YesNoUndecidedOptions.getText(optionType),
                    optionalObjectContext: optionType
                }
            });
        }
    }
}
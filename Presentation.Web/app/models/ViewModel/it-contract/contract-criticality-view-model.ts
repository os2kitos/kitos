module Kitos.Models.ViewModel.ItContract {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export interface IOptionValues {
        text: string,
        unavailableText: string,
        available: boolean,
    }

    export class CriticalityOptions {

        private getValueToTextMap() {
            var result = {};
            this.options.forEach(value => {
                result[value.id] = { text: value.text, unavailableText: `${value.text} (udgået)`, available: value.optionalObjectContext.expired }
            });
            return result;
        }

        //Cache texts for quick lookup texts
        private readonly valueToTextMap: { [key: number]: IOptionValues };

        getOptionText(id: number, takeUnavailableText: boolean): string {
            const value = this.valueToTextMap[id];
            if (!value) {
                return "";
            }
            if (takeUnavailableText) {
                return value.available
                    ? value.text
                    : value.unavailableText;
            }

            return value.text;
        }

        options: Select2OptionViewModel<any>[];
        constructor(dataSource: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[]) {
            this.options = [];
            dataSource.forEach(option => {
                const name = option.name;
                const item = { id: option.id as number, text: name, optionalObjectContext: option };
                this.options.push(item);
            });

            this.valueToTextMap = this.getValueToTextMap();
        }
    }
}
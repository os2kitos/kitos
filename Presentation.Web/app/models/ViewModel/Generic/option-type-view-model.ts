module Kitos.Models.ViewModel.Generic {
    export interface IOptionValues {
        text: string
    }

    export class OptionTypeViewModel {

        private getValueToTextMap() {
            var result = {};
            this.options.forEach(value => {
                result[value.id] = { text: value.text + (value.optionalObjectContext.expired ? " (udgået)" : "") }
            });
            return result;
        }

        //Cache texts for quick lookup texts
        private readonly valueToTextMap: { [key: number]: IOptionValues };

        getOptionText(id: number): string {
            const value = this.valueToTextMap[id];
            if (!value) {
                return "";
            }

            return value.text;
        }

        options: Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>[];
        enabledOptions: Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>[];
        constructor(dataSource: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[]) {
            this.options = [];
            dataSource.forEach(option => {
                const name = option.name;
                const item = { id: option.id as number, text: name, optionalObjectContext: option };
                this.options.push(item);
            });

            this.valueToTextMap = this.getValueToTextMap();
            this.enabledOptions = this.options.filter(option => !option.optionalObjectContext.expired);
        }
    }
}
module Kitos.Models.ViewModel.ItContract {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class CriticalityOptions {

        private getValueToTextMap() {
            var result = {};
            this.options.forEach(value => result[value.id] = value.text);
            return result;
        }

        //Cache texts for quick lookup texts
        private readonly valueToTextMap;

        getOptionText(id?: number) {
            return this.valueToTextMap[id] ?? "";
        }

        options: Select2OptionViewModel<any>[];
        constructor(dataSource: IOptionEntity[]) {
            this.options = [];
            dataSource.forEach(option => {
                const item = { id: option.Id as number, text: option.Name, optionalObjectContext: option };
                this.options.push(item);
            });

            this.valueToTextMap = this.getValueToTextMap();
        }
    }
}
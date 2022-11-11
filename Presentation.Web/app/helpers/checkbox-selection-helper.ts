module Kitos.Helpers {

    export class CheckboxSelectionHelper {
        static setSelectGroupToValue(objects: Models.ViewModel.Organization.IHasSelection[], targetValue: boolean) {
            objects.forEach(vm => {
                vm.selected = targetValue;
            });
        }
    }
}
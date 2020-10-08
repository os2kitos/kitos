module Kitos.Services.Generic {

    export interface IBindingService {
        bindMultiSelectConfiguration<T>(
            setField: ((finalVm: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<T>) => void),
            getInitialElements: () => T[],
            removeFunc: ((element: T) => void),
            newFunc: Models.ViewModel.Generic.ElementSelectedFunc<Models.ViewModel.Generic.Select2OptionViewModel<T>>,
            allowAddition: boolean,
            allowRemoval: boolean,
            searchFunc?: (query: string) => angular.IPromise<Models.ViewModel.Generic.Select2OptionViewModel<T>[]>,
            fixedValueRange?: () => Models.ViewModel.Generic.Select2OptionViewModel<T>[])
            : void;
    }

    export class BindingService implements IBindingService {

        static $inject = ["select2LoadingService"];
        constructor(private readonly select2LoadingService: Services.ISelect2LoadingService,) {

        }

        bindMultiSelectConfiguration<TElement>(
            setField: ((finalVm: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<TElement>) => void),
            getInitialElements: () => TElement[],
            removeFunc: ((element: TElement) => void),
            newFunc: Models.ViewModel.Generic.ElementSelectedFunc<Models.ViewModel.Generic.Select2OptionViewModel<TElement>>,
            allowAddition: boolean,
            allowRemoval: boolean,
            searchFunc?: (query: string) => angular.IPromise<Models.ViewModel.Generic.Select2OptionViewModel<TElement>[]>,
            fixedValueRange?: () => Models.ViewModel.Generic.Select2OptionViewModel<TElement>[]) {

            let select2Config;
            if (!!searchFunc) {
                select2Config = this.select2LoadingService.loadSelect2WithDataSource(searchFunc, false);
            } else if (!!fixedValueRange) {
                select2Config = this.select2LoadingService.select2LocalDataNoSearch(() => fixedValueRange(), false);
            } else {
                throw new Error("Either searchFunc or fixedValueRange must be provided");
            }

            const configuration = {
                selectedElements: getInitialElements(),
                removeItemRequested: removeFunc,
                allowAddition: allowAddition,
                allowRemoval: allowRemoval,
                newItemSelectionConfig: {
                    selectedElement: null,
                    select2Config: select2Config,
                    elementSelected: newFunc
                }
            };
            setField(configuration);
        }
    }

    app.service("bindingService", BindingService);
}
module Kitos.DataProcessing.Registration.Edit.Oversight {
    "use strict";

    export class EditOversightDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory",
            "select2LoadingService",
            "dataProcessingRegistrationOptions"
        ];

        private readonly dataProcessingRegistrationId: number;
        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions) {

            this.dataProcessingRegistrationId = this.dataProcessingRegistration.id;
            this.bindOversightInterval();
            this.bindOversightIntervalRemark();
            this.bindOversigthOptions();
            this.bindOversigthOptionsRemark();
        }

        headerName = this.dataProcessingRegistration.name;
        oversightInterval: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>;
        oversightIntervalRemark: Models.ViewModel.Generic.IEditTextViewModel;
        oversigthOptions: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        oversightOptionsRemark: Models.ViewModel.Generic.IEditTextViewModel;

        private bindOversigthOptions() {
            this.bindMultiSelectConfiguration<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>(
                config => this.oversigthOptions = config,
                () => this.dataProcessingRegistration.oversightOptions.value,
                element => this.removeOversightOption(element.id),
                newElement => this.addOversightOption(newElement),
                null,
                () => {
                    const selectedOversightOptions = this
                        .dataProcessingRegistration
                        .oversightOptions
                        .value
                        .reduce((acc, next, _) => {
                            acc[next.id] = next;
                            return acc;
                        },
                            {});
                    return this.dataProcessingRegistrationOptions.oversightOptions.filter(x => !selectedOversightOptions[x.id]).map(x => {
                        return {
                            text: x.name,
                            id: x.id,
                            optionalObjectContext: {
                                id: x.id,
                                name: x.name,
                                description: x.description,
                                expired: false //We only allow selection of non-expired and this object is based on the available objects
                            }
                        };
                    });
                }
            );
        }

        private bindOversigthOptionsRemark() {
            this.oversightOptionsRemark = new Models.ViewModel.Generic.EditTextViewModel(
                this.dataProcessingRegistration.oversightOptions.remark,
                (newText) => this.changeOversightOptionRemark(newText));
        }

        private bindOversightInterval() {
            this.oversightInterval = {
                selectedElement: this.getYearMonthIntervalOptionFromId(this.dataProcessingRegistration.oversightInterval.value),
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => new Models.ViewModel.Shared.YearMonthUndecidedIntervalOption().options, false),
                elementSelected: (newElement) => this.changeOversightInterval(newElement)
            }
        }

        private bindOversightIntervalRemark() {
            this.oversightIntervalRemark = new Models.ViewModel.Generic.EditTextViewModel(
                this.dataProcessingRegistration.oversightInterval.remark,
                (newText) => this.changeOversightIntervalRemark(newText));
        }

        private getYearMonthIntervalOptionFromId(id?: number): Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption> {
            return new Models.ViewModel.Shared.YearMonthUndecidedIntervalOption().getById(id);
        } 

        private changeOversightInterval(oversightInterval: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>) {
            this.apiUseCaseFactory
                .createUpdate("Tilsynsinterval", () => this.dataProcessingRegistrationService.updateOversightInterval(this.dataProcessingRegistration.id, oversightInterval.optionalObjectContext))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightInterval.value = oversightInterval.optionalObjectContext;
                    this.bindOversightInterval();
                    return success;
                });
        }

        private changeOversightIntervalRemark(oversightIntervalRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkning", () => this.dataProcessingRegistrationService.updateOversightIntervalRemark(this.dataProcessingRegistration.id, oversightIntervalRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightInterval.remark = oversightIntervalRemark;
                    this.bindOversightIntervalRemark();
                    return success;
                });
        }

        private changeOversightOptionRemark(oversightOptionRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkning", () => this.dataProcessingRegistrationService.updateOversightOptionRemark(this.dataProcessingRegistration.id, oversightOptionRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightOptions.remark = oversightOptionRemark;
                    this.bindOversigthOptionsRemark();
                    return success;
                });
        }

        private bindMultiSelectConfiguration<TElement>(
            setField: ((finalVm: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<TElement>) => void),
            getInitialElements: () => TElement[],
            removeFunc: ((element: TElement) => void),
            newFunc: Models.ViewModel.Generic.ElementSelectedFunc<Models.ViewModel.Generic.Select2OptionViewModel<TElement>>,
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
                allowAddition: this.hasWriteAccess,
                allowRemoval: this.hasWriteAccess,
                newItemSelectionConfig: {
                    selectedElement: null,
                    select2Config: select2Config,
                    elementSelected: newFunc
                }
            };
            setField(configuration);
        }

        private removeOversightOption(id: number) {
            this.apiUseCaseFactory
                .createAssignmentRemoval(() => this.dataProcessingRegistrationService.removeOversightOption(this.dataProcessingRegistrationId, id))
                .executeAsync(success => {

                    //Update the source collection
                    this.dataProcessingRegistration.oversightOptions.value = this.dataProcessingRegistration.oversightOptions.value.filter(x => x.id !== id);

                    //Propagate changes to UI binding
                    this.bindOversigthOptions();
                    return success;
                });
        }

        private addOversightOption(newElement: Models.ViewModel.Generic.Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>) {
            if (!!newElement && !!newElement.optionalObjectContext) {
                const oversightOption = newElement.optionalObjectContext as Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO;
                this.apiUseCaseFactory
                    .createAssignmentCreation(() => this.dataProcessingRegistrationService.assignOversightOption(this.dataProcessingRegistrationId, oversightOption.id))
                    .executeAsync(success => {
                        //Update the source collection 
                        this.dataProcessingRegistration.oversightOptions.value.push(oversightOption);
                        

                        //Trigger UI update
                        this.bindOversigthOptions();
                        return success;
                    });
            }
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.oversight", {
                url: "/oversight",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-oversight.view.html",
                controller: EditOversightDataProcessingRegistrationController,
                controllerAs: "vm"
            });
        }]);
}

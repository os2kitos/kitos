module Kitos.DataProcessing.Registration.Edit.Oversight {
    "use strict";

    export class EditOversightDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory",
            "select2LoadingService",
            "dataProcessingRegistrationOptions",
            "bindingService",
            "notify"
        ];

        private readonly dataProcessingRegistrationId: number;
        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions,
            private readonly bindingService: Kitos.Services.Generic.IBindingService,
            private readonly notify) {

            this.dataProcessingRegistrationId = this.dataProcessingRegistration.id;
            this.bindOversightInterval();
            this.bindOversightIntervalRemark();
            this.bindOversigthOptions();
            this.bindOversigthOptionsRemark();
            this.bindOversightCompleted();
            this.bindLatestOversightCompletedDate();
            this.bindOversightCompletedRemark();
        }

        headerName = this.dataProcessingRegistration.name;
        oversightInterval: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>;
        oversightIntervalRemark: Models.ViewModel.Generic.IEditTextViewModel;
        oversigthOptions: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        oversightOptionsRemark: Models.ViewModel.Generic.IEditTextViewModel;
        isOversightCompleted: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;
        latestOversightCompletedDate: Models.ViewModel.Generic.IDateSelectionViewModel;
        oversightCompletedRemark: Models.ViewModel.Generic.IEditTextViewModel;
        shouldShowLatestOversightCompletedDate: boolean;

        private bindOversigthOptions() {
            this.bindingService.bindMultiSelectConfiguration<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>(
                config => this.oversigthOptions = config,
                () => this.dataProcessingRegistration.oversightOptions.value,
                element => this.removeOversightOption(element.id),
                newElement => this.addOversightOption(newElement),
                this.hasWriteAccess,
                this.hasWriteAccess,
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

        private bindOversightCompleted() {
            this.isOversightCompleted = {
                selectedElement: this.getYesNoUndecidedOversightCompletedOptionFromId(this.dataProcessingRegistration.oversightCompleted.value),
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => new Models.ViewModel.Shared.YesNoUndecidedOptions().options, false),
                elementSelected: (newElement) => this.changeIsOversightCompleted(newElement)
            }

            this.shouldShowLatestOversightCompletedDate =
                this.isOversightCompleted.selectedElement !== null &&
                this.isOversightCompleted.selectedElement.optionalObjectContext === Models.Api.Shared.YesNoUndecidedOption.Yes;
        }

        private bindLatestOversightCompletedDate() {
            this.latestOversightCompletedDate = new Models.ViewModel.Generic.DateSelectionViewModel(
                this.dataProcessingRegistration.oversightCompleted.optionalDateValue,
                (newDate) => this.changeLatestOversightCompletedDate (newDate));
        }

        private bindOversightCompletedRemark() {
            this.oversightCompletedRemark = new Models.ViewModel.Generic.EditTextViewModel(
                this.dataProcessingRegistration.oversightCompleted.remark,
                (newText) => this.changeOversightCompletedRemark(newText));
        }

        private getYearMonthIntervalOptionFromId(id?: number): Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption> {
            return new Models.ViewModel.Shared.YearMonthUndecidedIntervalOption().getById(id);
        }

        private getYesNoUndecidedOversightCompletedOptionFromId(id?: number): Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption> {
            return new Models.ViewModel.Shared.YesNoUndecidedOptions().getById(id);
        }

        private changeOversightInterval(oversightInterval: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>) {
            this.apiUseCaseFactory
                .createUpdate("Tilsynsinterval", () => this.dataProcessingRegistrationService.updateOversightInterval(this.dataProcessingRegistrationId, oversightInterval.optionalObjectContext))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightInterval.value = oversightInterval.optionalObjectContext;
                    this.bindOversightInterval();
                    return success;
                });
        }

        private changeOversightIntervalRemark(oversightIntervalRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkninger", () => this.dataProcessingRegistrationService.updateOversightIntervalRemark(this.dataProcessingRegistrationId, oversightIntervalRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightInterval.remark = oversightIntervalRemark;
                    this.bindOversightIntervalRemark();
                    return success;
                });
        }

        private changeOversightOptionRemark(oversightOptionRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkninger", () => this.dataProcessingRegistrationService.updateOversightOptionRemark(this.dataProcessingRegistrationId, oversightOptionRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightOptions.remark = oversightOptionRemark;
                    this.bindOversigthOptionsRemark();
                    return success;
                });
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

        private addOversightOption(newElement: Models.ViewModel.Generic.
            Select2OptionViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>) {
            if (!!newElement && !!newElement.optionalObjectContext) {
                const oversightOption =
                    newElement.optionalObjectContext as Models.Generic.NamedEntity.
                        NamedEntityWithDescriptionAndExpirationStatusDTO;
                this.apiUseCaseFactory
                    .createAssignmentCreation(
                        () => this.dataProcessingRegistrationService.assignOversightOption(
                            this.dataProcessingRegistrationId,
                            oversightOption.id))
                    .executeAsync(success => {
                        //Update the source collection 
                        this.dataProcessingRegistration.oversightOptions.value.push(oversightOption);


                        //Trigger UI update
                        this.bindOversigthOptions();
                        return success;
                    });
            }
        }

        private changeIsOversightCompleted(isOversightCompleted: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>) {
            this.apiUseCaseFactory
                .createUpdate("Gennemført tilsyn", () => this.dataProcessingRegistrationService.updateOversightCompleted(this.dataProcessingRegistrationId, isOversightCompleted.optionalObjectContext))
                .executeAsync(success => {
                    if (success.optionalServerDataPush) {
                        this.dataProcessingRegistration.oversightCompleted = success.optionalServerDataPush.oversightCompleted;
                    }

                    this.dataProcessingRegistration.oversightCompleted.value = isOversightCompleted.optionalObjectContext;
                    this.bindOversightCompleted();
                    this.bindLatestOversightCompletedDate();
                    return success;
                });

        }

        private changeLatestOversightCompletedDate(latestOversightCompletedDate: string) {
            if (!!latestOversightCompletedDate) {
                var formattedDate = Helpers.DateStringFormat.fromDDMMYYYYToYYYYMMDD(latestOversightCompletedDate);
                if (!!formattedDate.convertedValue) {
                    return this.apiUseCaseFactory
                        .createUpdate("Dato for seneste tilsyn",
                            () => this.dataProcessingRegistrationService.updateLatestOversightCompletedDate(
                                this.dataProcessingRegistrationId,
                                formattedDate.convertedValue))
                        .executeAsync(success => {
                            this.dataProcessingRegistration.oversightCompleted.optionalDateValue = latestOversightCompletedDate;
                            this.bindLatestOversightCompletedDate();
                            return success;
                        });
                }
                return this.notify.addErrorMessage(formattedDate.errorMessage);
            }
        }

        private changeOversightCompletedRemark(oversightCompletedRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkninger", () => this.dataProcessingRegistrationService.updateOversightCompletedRemark(this.dataProcessingRegistrationId, oversightCompletedRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightInterval.remark = oversightCompletedRemark;
                    this.bindOversightIntervalRemark();
                    return success;
                });
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

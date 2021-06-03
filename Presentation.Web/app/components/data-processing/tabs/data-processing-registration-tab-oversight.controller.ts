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
            "$uibModal"
        ];

        private readonly dataProcessingRegistrationId: number;
        private readonly modal;
        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly dataProcessingRegistrationOptions: Models.DataProcessing.IDataProcessingRegistrationOptions,
            private readonly bindingService: Kitos.Services.Generic.IBindingService,
            private readonly $modal) {

            this.dataProcessingRegistrationId = this.dataProcessingRegistration.id;
            this.bindOversightInterval();
            this.bindOversightIntervalRemark();
            this.bindOversigthOptions();
            this.bindOversigthOptionsRemark();
            this.bindOversightCompleted();
            this.bindOversightCompletedRemark();

            this.bindOversightDates();
            this.modal = $modal;
        }

        headerName = this.dataProcessingRegistration.name;
        oversightInterval: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>;
        oversightIntervalRemark: Models.ViewModel.Generic.IEditTextViewModel;
        oversigthOptions: Models.ViewModel.Generic.IMultipleSelectionWithSelect2ConfigViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        oversightOptionsRemark: Models.ViewModel.Generic.IEditTextViewModel;
        isOversightCompleted: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;
        oldIsOversightCompletedValue: Models.Api.Shared.YesNoUndecidedOption;
        yesIsOversightCompletedValue = new Models.ViewModel.Shared.YesNoUndecidedOptions().options[1];
        oversightCompletedRemark: Models.ViewModel.Generic.IEditTextViewModel;
        shouldShowLatestOversightCompletedDate: boolean;

        oversightDates: Models.ViewModel.GDPR.IOversightDateViewModel[];

        createOversightDate() {
            this.modal.open({
                windowClass: "modal fade in",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-oversight-modal.html",
                controller: Kitos.DataProcessing.Registration.Edit.Oversight.Modal.OversightModalController,
                controllerAs: "modalvm",
                resolve: {
                    hasWriteAccess: [() => this.hasWriteAccess],
                    datepickerOptions: [() => this.datepickerOptions],
                    submitFunction: [() => this.assignOversightDate],
                    mainController: [() => this],
                    oversightId: [() => null], //As we don't know the id when creating this field is null
                    oversightDate: [() => null], //As this field is required to have a value entered by the user we initialize it with null
                    oversightRemark: [() => ""],
                    modalType: [() => DataProcessing.Registration.Edit.Oversight.Modal.ModalType.create]
                }
            });
        };

        updateOversightDate(oversightId: number, oversightDate: string, oversightRemark: string) {
            this.modal.open({
                windowClass: "modal fade in",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-oversight-modal.html",
                controller: Kitos.DataProcessing.Registration.Edit.Oversight.Modal.OversightModalController,
                controllerAs: "modalvm",
                resolve: {
                    hasWriteAccess: [() => this.hasWriteAccess],
                    datepickerOptions: [() => this.datepickerOptions],
                    submitFunction: [() => this.modifyOversightDate],
                    mainController: [() => this],
                    oversightId: [() => oversightId],
                    oversightDate: [() => oversightDate],
                    oversightRemark: [() => oversightRemark],
                    modalType: [() => DataProcessing.Registration.Edit.Oversight.Modal.ModalType.modify]
                }
            });
        };

        deleteOversightDate(oversightId: number) {
            this.removeOversightDate(oversightId);
        };

        datepickerOptions = {
            format: "dd-MM-yyyy"
        };


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

            this.oldIsOversightCompletedValue = this.isOversightCompleted.selectedElement.id;

            this.shouldShowLatestOversightCompletedDate =
                this.isOversightCompleted.selectedElement !== null &&
                this.isOversightCompleted.selectedElement.optionalObjectContext === Models.Api.Shared.YesNoUndecidedOption.Yes;
        }

        private bindOversightCompletedRemark() {
            this.oversightCompletedRemark = new Models.ViewModel.Generic.EditTextViewModel(
                this.dataProcessingRegistration.oversightCompleted.remark,
                (newText) => this.changeOversightCompletedRemark(newText));
        }

        private bindOversightDates() {
            this.oversightDates = _.map(this.dataProcessingRegistration.oversightDates, (oversightDate) => new Models.ViewModel.GDPR.OversightDateViewModel(
                oversightDate.id,
                moment(oversightDate.oversightDate).format("DD-MM-YYYY"),
                oversightDate.oversightRemark
            ));
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
            if (oversightIntervalRemark == null) {
                return;
            }
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
            if (this.oldIsOversightCompletedValue === Models.Api.Shared.YesNoUndecidedOption.Yes && isOversightCompleted.id !== Models.Api.Shared.YesNoUndecidedOption.Yes) {
                if (confirm("Er du sikker på du vil skifte væk fra 'Ja' og derved slette alle oprettede tilsyn?")) {
                    this.performIsOversightCompletedChange(isOversightCompleted);
                }
                else {
                    this.isOversightCompleted.selectedElement = this.yesIsOversightCompletedValue;
                }
            }
            else {
                this.performIsOversightCompletedChange(isOversightCompleted);
            }
        }

        private performIsOversightCompletedChange(isOversightCompleted: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>) {
            this.apiUseCaseFactory
                .createUpdate("Gennemført tilsyn", () => this.dataProcessingRegistrationService.updateOversightCompleted(this.dataProcessingRegistrationId, isOversightCompleted.optionalObjectContext))
                .executeAsync(success => {
                    if (success.optionalServerDataPush) {
                        this.dataProcessingRegistration.oversightCompleted = success.optionalServerDataPush.oversightCompleted;
                    }

                    this.dataProcessingRegistration.oversightCompleted.value = isOversightCompleted.optionalObjectContext;
                    if (this.dataProcessingRegistration.oversightCompleted.value !== Models.Api.Shared.YesNoUndecidedOption.Yes) {
                        this.dataProcessingRegistration.oversightDates = []; //Empty local array as it has been emptied in the database when the value is not "Yes"
                    }

                    this.bindOversightCompleted();
                    this.bindOversightDates();
                    return success;
                });

        }

        private changeOversightCompletedRemark(oversightCompletedRemark: string) {

            if (oversightCompletedRemark == null) {
                return;
            }
            this.apiUseCaseFactory
                .createUpdate("Bemærkninger", () => this.dataProcessingRegistrationService.updateOversightCompletedRemark(this.dataProcessingRegistrationId, oversightCompletedRemark))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightCompleted.remark = oversightCompletedRemark;
                    this.bindOversightCompletedRemark();
                    return success;
                });
        }

        private assignOversightDate(self: EditOversightDataProcessingRegistrationController, oversightDate: string, oversightRemark: string) {
            if (oversightDate == null || oversightRemark == null) {
                return null;
            }
            var formattedDate = Helpers.DateStringFormat.fromDDMMYYYYToYYYYMMDD(oversightDate);
            if (!!formattedDate.convertedValue) {
                return self.apiUseCaseFactory
                    .createAssignmentCreation(
                        () => self.dataProcessingRegistrationService.assignOversightDate(self.dataProcessingRegistrationId, formattedDate.convertedValue, oversightRemark))
                    .executeAsync(success => {
                        self.dataProcessingRegistration.oversightDates.push(success);
                        self.bindOversightDates();
                        return success;
                    });
            }
            return null;
        }

        private modifyOversightDate(self: EditOversightDataProcessingRegistrationController, oversightId: number, oversightDate: string, oversightRemark: string) {
            if (oversightId == null || oversightDate == null || oversightRemark == null) {
                return null;
            }
            var formattedDate = Helpers.DateStringFormat.fromDDMMYYYYToYYYYMMDD(oversightDate);
            if (!!formattedDate.convertedValue) {
                return self.apiUseCaseFactory
                    .createAssignmentCreation(
                        () => self.dataProcessingRegistrationService.updateOversightDate(self.dataProcessingRegistrationId, oversightId, formattedDate.convertedValue, oversightRemark))
                    .executeAsync(success => {
                        var updatedOversightDates = _.map(self.dataProcessingRegistration.oversightDates, (dateRemark) => dateRemark.id === success.id ? success : dateRemark); //Update modified entry
                        self.dataProcessingRegistration.oversightDates = updatedOversightDates;
                        self.bindOversightDates();
                        return success;
                    });
            }
            return null;
        }

        private removeOversightDate(oversightId: number) {
            if (oversightId == null) {
                return;
            }
            this.apiUseCaseFactory
                .createAssignmentRemoval(
                    () => this.dataProcessingRegistrationService.removeOversightDate(this.dataProcessingRegistrationId, oversightId))
                .executeAsync(success => {
                    _.remove(this.dataProcessingRegistration.oversightDates, x => x.id === success.id);
                    this.bindOversightDates();
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

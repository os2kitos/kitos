module Kitos.DataProcessing.Registration.Edit.Oversight {
    "use strict";

    export class EditOversightDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory",
            "select2LoadingService",
            "notify"
        ];

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly notify) {

            this.bindOversightInterval();
            this.bindOversightIntervalRemark();
            this.bindOversightCompleted();
            this.bindOversightCompletedLatestDate();
            this.bindOversightCompletedRemark();
        }

        headerName = this.dataProcessingRegistration.name;
        oversightInterval: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>;
        oversightIntervalRemark: Models.ViewModel.Generic.IEditTextViewModel;
        isOversightCompleted: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;
        oversightCompletedLatestDate: Models.ViewModel.Generic.IDateSelectionViewModel;
        oversightCompletedRemark: Models.ViewModel.Generic.IEditTextViewModel;
        shouldShowOversightCompletedLatestDate: boolean;


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

            this.shouldShowOversightCompletedLatestDate =
                this.isOversightCompleted.selectedElement !== null &&
                this.isOversightCompleted.selectedElement.optionalObjectContext === Models.Api.Shared.YesNoUndecidedOption.Yes;
        }

        private bindOversightCompletedLatestDate() {
            this.oversightCompletedLatestDate = new Models.ViewModel.Generic.DateSelectionViewModel(
                this.dataProcessingRegistration.oversightCompleted.optionalDateValue,
                (newDate) => this.changeOversightCompletedLatestDate(newDate));
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

        private changeIsOversightCompleted(isOversightCompleted: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>) {
            this.apiUseCaseFactory
                .createUpdate("Gennemført tilsyn", () => this.dataProcessingRegistrationService.updateOversightCompleted(this.dataProcessingRegistration.id, isOversightCompleted.optionalObjectContext))
                .executeAsync(success => {
                    if (success.optionalServerDataPush) {
                        this.dataProcessingRegistration.oversightCompleted = success.optionalServerDataPush.oversightCompleted;
                    }

                    this.dataProcessingRegistration.oversightCompleted.value = isOversightCompleted.optionalObjectContext;
                    this.bindOversightCompleted();
                    this.bindOversightCompletedLatestDate();
                    return success;
                });

        }

        private changeOversightCompletedLatestDate(oversightCompletedLatestDate: string) {
            if (!!oversightCompletedLatestDate) {
                var formattedDate = Helpers.DateStringFormat.fromDDMMYYYYToYYYYMMDD(oversightCompletedLatestDate);
                if (!!formattedDate.convertedValue) {
                    return this.apiUseCaseFactory
                        .createUpdate("Dato for seneste tilsyn",
                            () => this.dataProcessingRegistrationService.updateOversightLatestDate(
                                this.dataProcessingRegistration.id,
                                formattedDate.convertedValue))
                        .executeAsync(success => {
                            this.dataProcessingRegistration.oversightCompleted.optionalDateValue =
                                oversightCompletedLatestDate;
                            this.bindOversightCompletedLatestDate();
                            return success;
                        });
                }
                return this.notify.addErrorMessage(formattedDate.errorMessage);
            }
        }

        private changeOversightCompletedRemark(oversightCompletedRemark: string) {
            this.apiUseCaseFactory
                .createUpdate("Bemærkning", () => this.dataProcessingRegistrationService.updateOversightCompletedRemark(this.dataProcessingRegistration.id, oversightCompletedRemark))
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

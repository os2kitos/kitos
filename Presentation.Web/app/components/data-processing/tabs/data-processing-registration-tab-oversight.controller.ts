module Kitos.DataProcessing.Registration.Edit.Oversight {
    "use strict";

    export class EditOversightDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "hasWriteAccess",
            "dataProcessingRegistration",
            "apiUseCaseFactory",
            "select2LoadingService"
        ];

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService) {

            this.bindOversightInterval();
            this.bindOversightIntervalRemark();
        }

        headerName = this.dataProcessingRegistration.name;
        oversightInterval: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>;
        oversightIntervalRemark: Models.ViewModel.Generic.IEditTextViewModel;
        

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

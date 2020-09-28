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

        private readonly dataProcessingRegistrationId: number;

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            public hasWriteAccess,
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly notify) {

            this.dataProcessingRegistrationId = this.dataProcessingRegistration.id;

            this.bindOversightInterval();
            this.bindOversightIntervalNote();

        }

        headerName = this.dataProcessingRegistration.name;

        oversightInterval: Models.ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>;
        oversightIntervalNote: string;


        private bindOversightInterval() {
            this.oversightInterval = {
                selectedElement: this.getYearMonthIntervalOptionFromId(this.dataProcessingRegistration.oversightInterval.value),
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => new Models.ViewModel.Shared.YearMonthUndecidedIntervalOption().options, false),
                elementSelected: (newElement) => this.changeOversightInterval(newElement)
            }
        }

        private bindOversightIntervalNote() {
            this.oversightIntervalNote = this.dataProcessingRegistration.oversightInterval.note;
        }

        private getYearMonthIntervalOptionFromId(id?: number): Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption> {
            if (id === null) {
                return null;
            }
            return new Models.ViewModel.Shared.YearMonthUndecidedIntervalOption().options.filter(option => option.id === id)[0];
        } 

        private changeOversightInterval(oversightInterval: Models.ViewModel.Generic.Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>) {
            this.apiUseCaseFactory
                .createUpdate("Tilsyn Interval sat", () => this.dataProcessingRegistrationService.updateOversightInterval(this.dataProcessingRegistration.id, oversightInterval.optionalObjectContext))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightInterval.value = oversightInterval.optionalObjectContext;
                    this.bindOversightInterval();
                    return success;
                });
        }

        private changeOversightIntervalNote(oversightIntervalNote: string) {
            this.apiUseCaseFactory
                .createUpdate("Tilsyn Interval note sat", () => this.dataProcessingRegistrationService.updateOversightIntervalNote(this.dataProcessingRegistration.id, oversightIntervalNote))
                .executeAsync(success => {
                    this.dataProcessingRegistration.oversightInterval.note = oversightIntervalNote;
                    this.bindOversightIntervalNote();
                    return success;
                });
            

        }

    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.oversight", {
                url: "/main",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-oversight.view.html",
                controller: EditOversightDataProcessingRegistrationController,
                controllerAs: "vm"
            });
        }]);
}

module Kitos.DataProcessing.Registration.Edit.Contracts {
    "use strict";

    export class EditContractsDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistration"
        ];

        constructor(
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO) {
        }

        headerName = this.dataProcessingRegistration.name;
        contracts = this.dataProcessingRegistration.associatedContracts;
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.contracts", {
                url: "/contracts",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-contracts.view.html",
                controller: EditContractsDataProcessingRegistrationController,
                controllerAs: "vm"
            });
        }]);
}

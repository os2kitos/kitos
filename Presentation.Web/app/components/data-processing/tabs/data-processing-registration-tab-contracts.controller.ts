module Kitos.DataProcessing.Registration.Edit.Contracts {
    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;
    "use strict";

    export class EditContractsDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistration"
        ];

        constructor(
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO) {
        }

        headerName = this.dataProcessingRegistration.name;
        contracts: NamedEntityDTO[] = [
            { id: 1, name: "One" },
            { id: 2, name: "Two" },
            { id: 3, name: "Three" },
            { id: 4, name: "Four" },
            { id: 5, name: "Five" },
            { id: 6, name: "Six" },
            { id: 7, name: "Seven" },
            { id: 8, name: "Eight" }
        ];

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

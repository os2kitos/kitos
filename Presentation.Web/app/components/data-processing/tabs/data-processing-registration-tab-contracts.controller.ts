module Kitos.DataProcessing.Registration.Edit.Contracts {
    "use strict";

    export class EditContractsDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistration",
            "contracts"
        ];

        constructor(
            private readonly dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private readonly contract: Models.Generic.NamedEntity.NamedEntityDTO[]) {
        }

        headerName = this.dataProcessingRegistration.name;
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.contracts", {
                url: "/contracts",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-contracts.view.html",
                controller: EditContractsDataProcessingRegistrationController,
                controllerAs: "vm",
                resolve: {
                    contracts: [() =>
                        [
                            { id: 1, name: "One" },
                            { id: 2, name: "Two" },
                            { id: 3, name: "Three" },
                            { id: 4, name: "Four" },
                            { id: 5, name: "Five" },
                            { id: 6, name: "Six" },
                            { id: 7, name: "Seven" },
                            { id: 8, name: "Eight" }
                        ]
                    ]
                }
            });
        }]);
}

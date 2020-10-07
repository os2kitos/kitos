module Kitos.DataProcessing.Registration.Edit.Ref {
    "use strict";

    export class EditDataProcessingContractController {
        static $inject: Array<string> = [
            "hasWriteAccess",
            "contract",
            "ItContractsService"
        ];

        private readonly contractId: number;
        constructor(
            public hasWriteAccess,
            private readonly contract: Kitos.Models.ItContract.IItContractDTO,
            private readonly contractService: Kitos.Services.Contract.IItContractsService
        ) {
            this.contractId = this.contract.id;
        }


    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("it-contract.edit.data-processing", {
                url: "/data-processing",
                templateUrl: "app/components/it-contract/tabs/it-contract-tab-data-processing.view.html",
                controller: EditDataProcessingContractController,
                controllerAs: "vm",
                resolve: {
                },
            });
        }]);
}


module Kitos.DataProcessing.Agreement.Edit.Main {
    "use strict";

    export class EditMainDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "notify",
            "hasWriteAccess",
            "dataProcessingAgreement"
        ];

        constructor(
            private dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private notify,
            public hasWriteAccess,
            private dataProcessingAgreement) {
        }

        headerName: string = this.dataProcessingAgreement.name;

        changeName(name) {

            var msg = this.notify.addInfoMessage('Ændre navn på databehandleraftale');

            return this.dataProcessingAgreementService.rename(this.dataProcessingAgreement.id, name).then(
                nameChangeResponse => {
                    msg.toSuccessMessage("Databehandleraftale navn ændret!");
                    this.headerName = nameChangeResponse.valueModifiedTo;
                },
                (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                    switch (errorResponse) {
                    case Models.Api.ApiResponseErrorCategory.BadInput:
                        msg.toErrorMessage("Fejl! Navnet er ugyldigt!");
                        break;
                    case Models.Api.ApiResponseErrorCategory.Conflict:
                        msg.toErrorMessage("Fejl! Navnet er allerede brugt!");
                        break;
                    default:
                        msg.toErrorMessage("Fejl! Kunne ikke ændre navn på databehandleraftale!");
                        break;
                    }
                }
            );
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-agreement.main", {
                url: "/main",
                templateUrl: "app/components/data-processing/tabs/data-processing-agreement-tab-main.view.html",
                controller: EditMainDataProcessingAgreementController,
                controllerAs: "vm"
            });
        }]);
}

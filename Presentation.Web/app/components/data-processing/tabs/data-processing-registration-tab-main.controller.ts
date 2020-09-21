module Kitos.DataProcessing.Agreement.Edit.Main {
    "use strict";

    export class EditMainDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "dataProcessingRegistrationService",
            "notify",
            "hasWriteAccess",
            "dataProcessingRegistration"
        ];

        constructor(
            private readonly dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService,
            private readonly notify,
            public hasWriteAccess,
            private readonly dataProcessingRegistration : Models.DataProcessing.IDataProcessingRegistrationDTO) {
        }

        headerName = this.dataProcessingRegistration.name;

        changeName(name) {

            var msg = this.notify.addInfoMessage("Ændrer navn på databehandleraftale");

            return this.dataProcessingRegistrationService.rename(this.dataProcessingRegistration.id, name).then(
                nameChangeResponse => {
                    msg.toSuccessMessage("Navnet er ændret!");
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
            $stateProvider.state("data-processing.edit-registration.main", {
                url: "/main",
                templateUrl: "app/components/data-processing/tabs/data-processing-registration-tab-main.view.html",
                controller: EditMainDataProcessingRegistrationController,
                controllerAs: "vm"
            });
        }]);
}

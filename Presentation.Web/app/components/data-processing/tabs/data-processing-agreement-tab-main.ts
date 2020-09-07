module Kitos.DataProcessing.Agreement.Edit.Main {
    "use strict";

    export class EditMainDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "user",
            "$scope",
            "notify",
            "$state",
            "hasWriteAccess",
            "dataProcessingAgreement"
        ];

        constructor(
            private dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private user: Services.IUser,
            private $scope,
            private notify,
            private $state: angular.ui.IStateService,
            private hasWriteAccess,
            private dataProcessingAgreement) {
        }

        public headerName: string = this.dataProcessingAgreement.name;

        changeName(name) {

            var msg = this.notify.addInfoMessage('Ændre navn på databehandleraftale');

            return this.dataProcessingAgreementService.rename(this.dataProcessingAgreement.id, name).then(
                nameChangeResponse => {
                    if (nameChangeResponse.modified) {
                        msg.toSuccessMessage("Databehandleraftale navn ændret!");
                        this.headerName = nameChangeResponse.valueModified;
                    } else {
                        msg.toErrorMessage("Fejl! Kunne ikke ændre navn på databehandleraftale!");
                    }
                },
                onError => {
                    msg.toErrorMessage("Fejl! Kunne ikke ændre navn på databehandleraftale!");

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
                controller: EditDataProcessingAgreementController,
                controllerAs: "vm"
            });
        }]);
}

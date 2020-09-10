module Kitos.DataProcessing.Agreement.Edit {
    "use strict";

    export class SubNavDataProcessingAgreementController {
        static $inject: Array<string> = [
            "$scope",
            "$rootScope",
            "$state",
            "$http",
            "notify",
            "dataProcessingAgreementService"
        ];

        constructor(
            private $scope,
            private $rootScope,
            private $state: angular.ui.IStateService,
            private $http,
            private notify,
            private dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService) {

            this.$scope.page.title = "Databehandleraftaler";
 
            $rootScope.page.subnav = [
                { state: "data-processing.overview", text: "Databehandleraftaler"}
            ];
            $rootScope.page.subnav.buttons = [
                { func: remove, text: "Slet Databehandleraftale", style: "btn-danger", showWhen: "data-processing.edit-agreement", dataElementType: 'removeDataProcessingAgreementButton' }
            ];

            $rootScope.subnavPositionCenter = false;

            $scope.$on("$viewContentLoaded", () => {
                $rootScope.positionSubnav();
            });

            function remove() {
                if (!confirm("Er du sikker på du vil slette Databehandleraftale?")) {
                    return;
                }
                var dataProcessingAgreementId = $state.params["id"];
                var msg = notify.addInfoMessage("Sletter Databehandleraftale...", false);

                dataProcessingAgreementService.delete(dataProcessingAgreementId).then(
                    deleteResponse => {
                        msg.toSuccessMessage("Databehandleraftale slettet!");
                            $state.go("data-processing.overview");
                        },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                        case Models.Api.ApiResponseErrorCategory.BadInput:
                        case Models.Api.ApiResponseErrorCategory.Conflict:
                                msg.toErrorMessage("Fejl! Kunne ikke slatte databehandleraftale, kan allerede være slettet!");
                            break;
                        default:
                            msg.toErrorMessage("Fejl! Kunne ikke slette databehandleraftale!");
                            break;
                        }
                    }
                );
            }
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing", {
                url: "/data-processing",
                abstract: true,
                template: "<ui-view autoscroll=\"false\" />",
                controller: SubNavDataProcessingAgreementController,
                controllerAs: "vm"
            });
        }]);
}
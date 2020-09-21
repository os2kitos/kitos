module Kitos.DataProcessing.Agreement.Edit {
    "use strict";

    export class SubNavDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "$scope",
            "$rootScope",
            "$state",
            "notify",
            "dataProcessingRegistrationService"
        ];

        constructor(
            private readonly $scope,
            $rootScope,
            $state: angular.ui.IStateService,
            notify,
            dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService) {

            this.$scope.page.title = "Databehandling - Overblok";
 
            $rootScope.page.subnav = [
                { state: "data-processing.overview", substate: "data-processing.edit-registration", text: "Overblik" }
            ];
            $rootScope.page.subnav.buttons = [
                { func: remove, text: "Slet Registrering", style: "btn-danger", showWhen: "data-processing.edit-registration", dataElementType: 'removeDataProcessingRegistrationButton' }
            ];

            $rootScope.subnavPositionCenter = false;

            $scope.$on("$viewContentLoaded", () => {
                $rootScope.positionSubnav();
            });

            function remove() {
                if (!confirm("Er du sikker på du vil slette registreringen?")) {
                    return;
                }
                const dataProcessingRegistrationId = $state.params["id"];
                var msg = notify.addInfoMessage("Sletter registreringen...", false);

                dataProcessingRegistrationService.delete(dataProcessingRegistrationId).then(
                    deleteResponse => {
                        msg.toSuccessMessage("Registreringen er slettet!");
                            $state.go("data-processing.overview");
                        },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                        case Models.Api.ApiResponseErrorCategory.NotFound:
                                msg.toErrorMessage("Fejl! Kunne ikke finde og slette registreringen!");
                            break;
                        default:
                                msg.toErrorMessage("Fejl! Kunne ikke slette registreringen!");
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
                controller: SubNavDataProcessingRegistrationController,
                controllerAs: "vm"
            });
        }]);
}
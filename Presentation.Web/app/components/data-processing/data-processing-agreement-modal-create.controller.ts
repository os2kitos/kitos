module Kitos.DataProcessing.Agreement.Create {
    "use strict";

    //TODO: Create a modal controller
    export class CreateDateProcessingAgreementController {
        static $inject: Array<string> = [
            "$scope",
            "dataProcessingAgreementService"
        ];

        constructor(
            private $scope: ng.IScope,
            private dataProcessingAgreementService : Kitos.Services.DataProcessing.IDataProcessingAgreementService) {
        }
    }

    //TODO: Publish it as well like below with OnEnter modal stuff
    //angular
    //    .module("app")
    //    .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
    //        $stateProvider.state("reports.overview.report-edit", {
    //            url: "/{id:int}/edit",
    //            onEnter: [
    //                "$state", "$stateParams", "$uibModal", "$rootScope",
    //                ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $uibModal: ng.ui.bootstrap.IModalService, $rootScope) => {
    //                    $rootScope = $uibModal.open({
    //                        templateUrl: "app/components/reports/report-edit.modal.view.html",
    //                        // fade in instead of slide from top, fixes strange cursor placement in IE
    //                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
    //                        windowClass: "modal fade in",
    //                        resolve: {
    //                            hasWriteAccess: ["authorizationServiceFactory", "$stateParams",
    //                                (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory, $stateParams) => {
    //                                    return $stateParams.id
    //                                        ?
    //                                        // Edit dialog - get edit rights
    //                                        authorizationServiceFactory
    //                                            .createReportAuthorization()
    //                                            .getAuthorizationForItem($stateParams.id)
    //                                            .then(accessRightsDto => accessRightsDto.canEdit)

    //                                        // When used as an "Add" dialog
    //                                        : authorizationServiceFactory
    //                                            .createReportAuthorization()
    //                                            .getOverviewAuthorization()
    //                                            .then(accessRightsDto => accessRightsDto.canCreate);

    //                                }
    //                            ],
    //                            user: ["userService", (userService: Services.IUserService) => userService.getUser()],
    //                        },
    //                        controller: EditReportController,
    //                        controllerAs: "vm",
    //                    }).result.then(() => {
    //                        // OK
    //                        // GOTO parent state and reload
    //                        $state.go("^", null, { reload: true });
    //                    },
    //                        () => {
    //                            // Cancel
    //                            // GOTO parent state
    //                            $state.go("^");
    //                        });
    //                }
    //            ]
    //        });
    //    }]);
}

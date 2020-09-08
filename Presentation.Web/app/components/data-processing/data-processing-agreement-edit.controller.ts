module Kitos.DataProcessing.Agreement.Edit {
    "use strict";

    export class EditDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "user",
            "$scope",
            "$rootScope",
            "notify",
            "$state",
            "hasWriteAccess",
            "userAccessRights",
            "dataProcessingAgreement"
        ];

        constructor(
            private dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            private user: Services.IUser,
            private $scope,
            private $rootScope,
            private notify,
            private $state: angular.ui.IStateService,
            private hasWriteAccess,
            private userAccessRights,
            private dataProcessingAgreement) {

            if (!userAccessRights.canDelete) {
                _.remove(this.$rootScope.page.subnav.buttons, (o: any) => o.text === "Slet Databehandlingsaftale");
            }
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-agreement", {
                url: "/edit/{id:[0-9]+}",
                templateUrl: "app/components/data-processing/data-processing-agreement-edit.view.html",
                controller: EditDataProcessingAgreementController,
                controllerAs: "vm",
                resolve: {
                    user: ["userService", userService => userService.getUser().then(user => user)
                    ],
                    userAccessRights: ["authorizationServiceFactory", "$stateParams",
                        (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                            authorizationServiceFactory.createDataProcessingAgreementAuthorization().getAuthorizationForItem($stateParams.id)
                    ],
                    hasWriteAccess: [
                        "userAccessRights", (userAccessRights: Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                    ],
                    dataProcessingAgreement: ['$http', '$stateParams', ($http, $stateParams) => $http.get("api/v1/data-processing-agreement/" + $stateParams.id)
                        .then(result => result.data.response)],
                }
            });
        }]);
}
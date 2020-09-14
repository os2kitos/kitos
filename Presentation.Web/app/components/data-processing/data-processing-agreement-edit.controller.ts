module Kitos.DataProcessing.Agreement.Edit {
    "use strict";

    export class EditDataProcessingAgreementController {
        static $inject: Array<string> = [
            "$rootScope",
            "userAccessRights"
        ];

        constructor(
            private readonly $rootScope,
            private readonly userAccessRights: Models.Api.Authorization.EntityAccessRightsDTO) {

            if (!this.userAccessRights.canDelete) {
                _.remove(this.$rootScope.page.subnav.buttons, (o: any) => o.dataElementType === "removeDataProcessingAgreementButton");
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
                    dataProcessingAgreement: [
                        "dataProcessingAgreementService", "$stateParams", (dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService, $stateParams) => dataProcessingAgreementService.get($stateParams.id)
                    ],
                }
            });
        }]);
}
module Kitos.DataProcessing.Agreement.Edit {
    "use strict";

    export class EditDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "$rootScope",
            "userAccessRights"
        ];

        constructor(
            private readonly $rootScope,
            private readonly userAccessRights: Models.Api.Authorization.EntityAccessRightsDTO) {

            if (!this.userAccessRights.canDelete) {
                _.remove(this.$rootScope.page.subnav.buttons, (o: any) => o.dataElementType === "removeDataProcessingRegistrationButton");
            }
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration", {
                url: "/edit/{id:[0-9]+}",
                templateUrl: "app/components/data-processing/data-processing-registration-edit.view.html",
                controller: EditDataProcessingRegistrationController,
                controllerAs: "vm",
                resolve: {
                    user: ["userService", userService => userService.getUser().then(user => user)
                    ],
                    userAccessRights: ["authorizationServiceFactory", "$stateParams",
                        (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                            authorizationServiceFactory.createDataProcessingRegistrationAuthorization().getAuthorizationForItem($stateParams.id)
                    ],
                    hasWriteAccess: [
                        "userAccessRights", (userAccessRights: Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                    ],
                    dataProcessingRegistration: [
                        "dataProcessingRegistrationService", "$stateParams", (dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService, $stateParams) => dataProcessingRegistrationService.get($stateParams.id)
                    ],
                }
            });
        }]);
}
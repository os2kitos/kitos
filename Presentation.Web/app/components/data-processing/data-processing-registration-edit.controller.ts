﻿module Kitos.DataProcessing.Registration.Edit {
    "use strict";

    export class EditDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "$rootScope",
            "userAccessRights",
            "uiState",
            "user"
        ];

        isFrontPageEnabled: boolean;
        isItSystemsEnabled: boolean;
        isItContractsEnabled: boolean;
        isOversightEnabled: boolean;
        isRolesEnabled: boolean;
        isNotificationsEnabled: boolean;
        isExternalReferencesEnabled: boolean;

        constructor(
            private readonly $rootScope,
            private readonly userAccessRights: Models.Api.Authorization.EntityAccessRightsDTO,
            uiState: Kitos.Models.UICustomization.ICustomizedModuleUI,
            user) {

            if (!this.userAccessRights.canDelete) {
                _.remove(this.$rootScope.page.subnav.buttons, (o: any) => o.dataElementType === "removeDataProcessingRegistrationButton");
            }

            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.DataProcessingUiCustomizationBluePrint;
            this.isFrontPageEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage);
            this.isItSystemsEnabled = user.currentConfig.showItSystemModule && uiState.isBluePrintNodeAvailable(blueprint.children.itSystems);
            this.isItContractsEnabled = user.currentConfig.showItContractModule && uiState.isBluePrintNodeAvailable(blueprint.children.itContracts);
            this.isOversightEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.oversight);
            this.isRolesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.roles);
            this.isNotificationsEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.notifications);
            this.isExternalReferencesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.references);
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
                    dataProcessingRegistrationOptions: [
                        "dataProcessingRegistrationService", "user", (dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService, user) => dataProcessingRegistrationService.getApplicableDataProcessingRegistrationOptions(user.currentOrganizationId)
                    ],
                    uiState: [
                        "uiCustomizationStateService", (uiCustomizationStateService: Kitos.Services.UICustomization.IUICustomizationStateService) => uiCustomizationStateService.getCurrentState(Kitos.Models.UICustomization.CustomizableKitosModule.DataProcessingRegistrations)
                    ]
                }
            });
        }]);
}
module Kitos.ItProject.Edit {
    "use strict";

    export interface IEditController {
        allowClearOption: IAllowClearOption;
        autosaveUrl: string;
        dropdownData: Array<IDropdownOption>;
        hasWriteAccess: boolean;
        parentSelectOptions: any;
        project: any;
        projectTypes: Array<any>;
        selectedData: Array<IDropdownOption>;
        selectSettings: ISelectSettings;
        selectTranslation: ISelectTranslation;
    }

    class EditController implements IEditController {
        public allowClearOption: IAllowClearOption;
        public autosaveUrl: string;
        public dropdownData: Array<IDropdownOption>;
        public parentSelectOptions;
        public selectedData: Array<IDropdownOption>;
        public selectTranslation: ISelectTranslation;
        public selectSettings: ISelectSettings;

        public static $inject: Array<string> = [
            "$rootScope",
            "_",
            "project",
            "projectTypes",
            "hasWriteAccess",
            "userAccessRights"
        ];

        constructor(
            private $rootScope,
            private _: ILoDashWithMixins,
            public project,
            public projectTypes,
            public hasWriteAccess,
            public userAccessRights: Models.Api.Authorization.EntityAccessRightsDTO) {
            this.autosaveUrl = `api/itproject/${this.project.id}`;
            if (!userAccessRights.canDelete) {
                _.remove($rootScope.page.subnav.buttons, function (o: any) {
                    return o.text === "Slet IT Projekt";
                });
            }
        }
    }

    angular
        .module("app")
        .controller("project.EditCtrl", EditController)
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-project.edit", {
                    url: "/edit/{id:[0-9]+}",
                    templateUrl: "app/components/it-project/it-project-edit.view.html",
                    controller: EditController,
                    controllerAs: "projectEditVm",
                    resolve: {
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        userAccessRights: ["authorizationServiceFactory", "$stateParams",
                            (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                            authorizationServiceFactory
                            .createProjectAuthorization()
                            .getAuthorizationForItem($stateParams.id)
                        ],
                        hasWriteAccess: [
                            "userAccessRights", (userAccessRights: Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                        ],
                        project: [
                            "$http", "$stateParams", ($http: ng.IHttpService, $stateParams) => {
                                return $http.get("api/itproject/" + $stateParams.id)
                                    .then((result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => result.data.response);
                            }
                        ],
                        projectTypes: [
                            "$http", $http => {
                                return $http.get("odata/LocalItProjectTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                                    .then((result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => result.data.value);
                            }
                        ]
                    }
                });
            }
        ]);
}

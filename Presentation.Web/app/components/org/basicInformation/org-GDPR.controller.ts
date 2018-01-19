module Kitos.Organization.GDPR {
    "use strict";

    class OrganizationGDPRController {

        public static $inject: string[] = ["$http", "$timeout", "_", "$", "$state", "$scope", "notify", "user", "hasWriteAccess", "organization", "dataResponsible", "dataProtectionAdvisor"];
        public updateOrgUrl: string;
        public updatedataProtectionAdvisorUrl: string;
        public updatedataResponsibleUrl: string;

        constructor(
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private $state: ng.ui.IStateService,
            private $scope,
            private notify,
            private user,
            private hasWriteAccess,
            private organization,
            private dataResponsible,
            private dataProtectionAdvisor) {
            this.hasWriteAccess = hasWriteAccess;
            this.organization = organization; 

            //init update urls
            this.updateOrgUrl = 'api/Organization/' + this.organization.id;
            this.updatedataProtectionAdvisorUrl = 'api/dataProtectionAdvisor/' + this.dataProtectionAdvisor.id;
            this.updatedataResponsibleUrl = 'api/dataResponsible/' + this.dataResponsible.id;

            
        }

    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider) => {
            $stateProvider.state("organization.gdpr", {
                url: "/basicInfo",
                templateUrl: "app/components/org/basicInformation/org-GDPR.view.html",
                controller: OrganizationGDPRController,
                controllerAs: "ctrl",
                resolve: {
                    user: [
                        "userService", userService => userService.getUser()
                    ],
                hasWriteAccess: [
                        '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                            return $http.get('api/Organization/' + user.currentOrganizationId + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                                .then(function (result) {
                                    return result.data.response;
                                });
                        }
                    ],
                    organization: ['$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        return $http.get('api/Organization/' + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                }],
                    dataResponsible: ['$http', '$stateParams', 'organization', function ($http, $stateParams, organization) {
                        return $http.get('api/dataResponsible/' + organization.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                    }],
                    dataProtectionAdvisor: ['$http', '$stateParams', 'organization', function ($http, $stateParams, organization) {
                        //get by org id
                        return $http.get('api/dataProtectionAdvisor/' + organization.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }]
                }
            });
        }
        ]);
}

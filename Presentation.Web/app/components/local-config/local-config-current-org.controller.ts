module Kitos.LocalAdmin.Organization {
    "use strict";

    export class CurrentOrganizationController {
        public orgAutosaveUrl: string;
        public typeName: string;
        public static $inject: string[] = ["$scope", "organization", "user"];

        constructor(private $scope, public organization, private user) {
            switch (organization.typeId) {
                case 1: this.typeName = "Kommune"; break;
                case 2: this.typeName = "Interessefællesskab"; break;
                case 3: this.typeName = "Virksomhed"; break;
                case 4: this.typeName = "Anden offentlig myndighed"; break;
            }

            this.orgAutosaveUrl = `api/organization/${organization.id}`;
            this.$scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
            this.$scope.currentOrganizationId = user.currentOrganizationId;
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state("local-config.current-org", {
                    url: "/current-org",
                    templateUrl: "app/components/local-config/local-config-current-org.view.html",
                    controller: CurrentOrganizationController,
                    controllerAs: "currentOrgCtrl",
                    resolve: {
                        organization: ["organizationApiService", "userService", (organizationApiService: Services.IOrganizationApiService, userService) => {
                            return userService.getUser().then((user) => {
                                return organizationApiService.getOrganization(user.currentOrganizationId);
                            });
                        }],
                        user: [
                            "userService", userService => userService.getUser()
                        ]
                    }
                });
            }]);
}
module Kitos.LocalAdmin.Organization {
    "use strict";

    export class CreateOrganizationController {
        public title: string;
        public org;

        public static $inject: string[] = ["$rootScope", "$scope", "$http", "notify", "OrganizationService", "userService"];

        constructor(private $rootScope, private $scope, private $http, private notify, private organizationService: Services.OrganizationService, private userService: Services.UserService) {
            $rootScope.page.title = "Ny organisation";
            this.title = "Opret organisation";
            this.org = {};
            this.org.accessModifier = 0;
            this.org.typeId = 1; // set type to interessefællesskab by default
        }

        public dismiss() {
            this.$scope.$dismiss();
        }

        public submit() {

            //this.organizationService.attachUser();

            var payload = this.org;
            this.$http.post("api/organization", payload)
                .success((result) => {
                    this.notify.addSuccessMessage(`Organisationen ${result.response.name} er blevet oprettet!`);
                    this.$scope.$close(true);
                })
                .error((result) => {
                    this.notify.addErrorMessage(`Organisationen ${this.org.name} kunne ikke oprettes!`);
                });
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider) => {
                $stateProvider.state("local-config.org.create", {
                    url: "/create",
                    authRoles: Models.OrganizationRole.LocalAdmin,
                    onEnter: ["$state", "$stateParams", "$uibModal",
                        ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $modal: ng.ui.bootstrap.IModalService) => {
                            $modal.open({
                                size: "lg",
                                templateUrl: "app/components/local-config/local-admin-organization-modal.view.html",
                                // fade in instead of slide from top, fixes strange cursor placement in IE
                                // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                                windowClass: "modal fade in",
                                controller: CreateOrganizationController,
                                controllerAs: "ctrl"
                            }).result.then(() => {
                                // OK
                                // GOTO parent state and reload
                                $state.go("^", null, { reload: true });
                            }, function () {
                                // Cancel
                                // GOTO parent state
                                $state.go("^");
                            });
                        }
                    ]
                });
            }
        ]);
}

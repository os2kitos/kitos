module Kitos.LocalAdmin.Organization {
    "use strict";

    export class CreateOrganizationController {
        public title: string;
        public org;

        public static $inject: string[] = [
            "$rootScope", "$scope", "$http", "notify", "organizationService", "userService"
        ];

        constructor(private $rootScope,
            private $scope,
            private $http,
            private notify,
            private organizationService: Services.OrganizationService,
            private userService: Services.IUserService,
            private currentUser: Services.IUser) {

            $rootScope.page.title = "Ny organisation";
            this.title = "Opret organisation";
            this.org = {};
            this.org.accessModifier = 0;
            this.org.typeId = 2; // set type to interessefællesskab by default

            this.userService.getUser().then((user: Services.IUser) => {
                this.currentUser = user;
            }).catch((error) => {
                this.notify.addErrorMessage("User not found!");
            });

        }

        public dismiss() {
            this.$scope.$dismiss();
        }

        public submit() {

            var payload = this.org;
            this.$http.post("api/organization", payload)
                .success((result) => {

                    if (this.org.typeId === 2) {
                        this.assignRoles(result.response.id);
                        this.notify
                            .addSuccessMessage(`Organisationen ${result.response.name} er blevet oprettet med ${
                            this.currentUser.fullName} som lokal admin og bruger.`);
                    } else {
                        this.notify.addSuccessMessage(`Organisationen ${result.response.name} er blevet oprettet!`);
                    }

                    this.$scope.$close(true);
                })
                .error((result) => {
                    this.notify.addErrorMessage(`Organisationen ${this.org.name} kunne ikke oprettes!`);
                });
        }

        public assignRoles(orgId: number) {
            this.organizationService.addRole(orgId, this.currentUser.id, Models.OrganizationRole.LocalAdmin);
            this.organizationService.addRole(orgId, this.currentUser.id, Models.OrganizationRole.User);
        }

    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider) => {
                $stateProvider.state("local-config.org.create", {
                    url: "/create",
                    authRoles: [Models.OrganizationRole.LocalAdmin],
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

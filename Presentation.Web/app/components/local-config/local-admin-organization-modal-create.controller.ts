module Kitos.LocalAdmin.Organization {
    "use strict";

    export class CreateOrganizationController {
        title: string;

        static $inject: string[] = [
            "$rootScope", "$scope", "$http", "notify", "userService"
        ];

        constructor(
            private $rootScope,
            private $scope,
            private $http,
            private notify,
            private userService: Services.IUserService,
            private currentUser: Services.IUser,
            private org) {

            $rootScope.page.title = "Ny organisation";
            this.title = "Opret organisation";
            this.org = {};
            this.org.TypeId = 2; // set type to interessefællesskab by default
            this.org.Cvr = null;

            this.userService.getUser().then((user: Services.IUser) => {
                this.currentUser = user;
            }).catch((error) => {
                this.notify.addErrorMessage("Brugeren blev ikke fundet!");
            });
        }

        dismiss() {
            this.$scope.$dismiss();
        }

        submit() {
            var self = this;
            const payload = {
                Id: this.currentUser.currentOrganizationId,
                ObjectOwnerId: this.currentUser.id,
                Name: this.org.Name,
                TypeId: this.org.TypeId,
                AccessModifier: "1", //Public
                Cvr: this.org.Cvr
            }
            this.$http.post("odata/Organizations", payload)
                .then(function onSuccess(result) {
                    if (self.org.TypeId === 2) {
                        self.notify.addSuccessMessage(`Organisationen ${result.data.Name} er blevet oprettet med ${self.currentUser.fullName} som lokal admin.`);
                        self.userService.reAuthorize();
                    } else {
                        self.notify.addSuccessMessage(`Organisationen ${result.data.Name} er blevet oprettet!`);
                    }

                    self.$scope.$close(true);
                }, function onError(result) {
                        self.notify.addErrorMessage(`Organisationen ${self.org.Name} kunne ikke oprettes!`);
                });
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

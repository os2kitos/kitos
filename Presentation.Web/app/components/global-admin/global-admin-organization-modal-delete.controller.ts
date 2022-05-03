module Kitos.GlobalAdmin.Organization {
    "use strict";

    export class DeleteOrganizationController {
        static $inject: string[] = ["$scope", "organizationApiService", "notify", "orgToDelete", "conflicts"];

        readonly title: string;
        readonly requireAcceptConsequences: boolean;
        readonly consequencesAccepted: boolean;

        constructor(
            private readonly $scope,
            private readonly organizationApiService: Services.IOrganizationApiService,
            private readonly notify,
            private readonly orgToDelete: Models.Api.Organization.Organization,
            private readonly conflicts: Models.Api.Organization.OrganizationDeleteConflicts) {
            this.title = `Slet "${orgToDelete.name}"`;


            this.requireAcceptConsequences = Models.Api.Organization.detectConflicts(conflicts);
            //TODO: Simple VM with grouped resolutions and their cause (present simple overview)
            //TODO: Clipboard button
            this.consequencesAccepted = !this.requireAcceptConsequences;
        }


        dismiss() {
            this.$scope.$dismiss();
        };

        submit() {
            this.organizationApiService.deleteOrganization(this.orgToDelete.uuid, this.consequencesAccepted)
                .then((success) => {
                    this.notify.addSuccessMessage("Organisationen blev slettet");
                    this.$scope.$close(true);
                },
                    (error) => {
                        console.error("Error deleting org:", this.orgToDelete, "Error:", error);
                        this.notify.addErrorMessage("Fejl ifm. sletning af organisationen!");
                    });
        };
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider) => {
                $stateProvider.state("global-admin.organizations.delete", {
                    url: "/delete/:id",
                    authRoles: ["GlobalAdmin"],
                    onEnter: ["$state", "$stateParams", "$uibModal",
                        ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $modal: ng.ui.bootstrap.IModalService) => {
                            $modal.open({
                                size: "lg",
                                templateUrl: "app/components/global-admin/global-admin-organization-modal-delete.view.html",
                                windowClass: "modal fade in",
                                resolve: {
                                    orgToDelete: [
                                        "organizationApiService", (organizationApiService: Services.IOrganizationApiService) =>
                                            organizationApiService.getOrganization($stateParams["id"])
                                    ],
                                    conflicts: [
                                        "organizationApiService", (organizationApiService: Services.IOrganizationApiService) =>
                                            organizationApiService
                                                .getOrganization($stateParams["id"])
                                                .then(orgToDelete => organizationApiService.getOrganizationDeleteConflicts(orgToDelete.uuid))
                                    ]
                                },
                                controller: DeleteOrganizationController,
                                controllerAs: "ctrl"
                            }).result.then(() => {
                                // OK
                                // GOTO parent state and reload
                                $state.go("^", null, { reload: true });
                            }, () => {
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

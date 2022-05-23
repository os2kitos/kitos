module Kitos.GlobalAdmin.Organization {
    "use strict";

    export class DeleteOrganizationController {
        static $inject: string[] = ["$scope", "organizationApiService", "notify", "orgToDelete", "conflicts"];

        readonly title: string;
        readonly orgName: string;
        readonly conflictsDetected: boolean;
        readonly consequencesAccepted: boolean;
        readonly conflictsModel: Models.ViewModel.Organization.OrganizationDeletionConflictsViewModel | null;
        isCopyingConsequencesToClipBoard: boolean = false; //Used to trigger additional spacing in the DOM to support "paste" into word with better spacing.

        constructor(
            private readonly $scope,
            private readonly organizationApiService: Services.IOrganizationApiService,
            private readonly notify,
            private readonly orgToDelete: Models.Api.Organization.Organization,
            private readonly conflicts: Models.Api.Organization.OrganizationDeleteConflicts) {
            this.title = `Slet "${orgToDelete.name}"`;
            this.orgName = orgToDelete.name;

            const mapEntityFromOrg = (input: Models.Generic.NamedEntity.EntityWithOrganizationRelationshipDTO): Models.ViewModel.Organization.EntityFromOrganization => {
                return {
                    name: input.name,
                    organizationName: input.organization.name
                };
            }

            this.conflictsDetected = Models.Api.Organization.detectConflicts(conflicts);
            if (this.conflictsDetected) {
                this.conflictsModel = {
                    contractsWhereSupplierWillBeRemoved: conflicts.contractsInOtherOrganizationsWhereOrgIsSupplier.map(mapEntityFromOrg),
                    dprWhereOrganizationIsRemovedFromListOfDataProcessors: conflicts.dprInOtherOrganizationsWhereOrgIsDataProcessor.map(mapEntityFromOrg),
                    dprWhereOrganizationIsRemovedFromListOfSubDataProcessors: conflicts.dprInOtherOrganizationsWhereOrgIsSubDataProcessor.map(mapEntityFromOrg),
                    interfacesBeingMovedToDefaultOrg: {
                        exposedOnSystemsInOtherOrganizations: conflicts.interfacesExposedOnSystemsOutsideTheOrganization.map(input => {
                            return {
                                name: input.exposedInterface.name,
                                exposedOnSystem: mapEntityFromOrg(input.exposedBy)
                            };
                        })
                    },
                    systemsBeingMovedToDefaultOrg: {
                        systemsExposingInterfacesOutsideTheOrganization: conflicts.systemsExposingInterfacesDefinedInOtherOrganizations.map(conflict => {
                            return {
                                name: conflict.system.name,
                                exposedInterfaces: conflict.exposedInterfaces.map(mapEntityFromOrg)
                            };
                        }),
                        systemsSetAsParentsToSystemsOutsideTheOrganization: conflicts.systemsSetAsParentSystemToSystemsInOtherOrganizations.map(conflict => {
                            return {
                                name: conflict.system.name,
                                children: conflict.children.map(mapEntityFromOrg)
                            };
                        }),
                        systemsUsedOutsideTheOrganization: conflicts.systemsWithUsagesOutsideTheOrganization.map(conflict => {
                            return {
                                name: conflict.system.name,
                                organizations: conflict.otherOrganizationsWhichUseTheSystem.map(org => org.name)
                            };
                        })
                    },
                    systemsInOtherOrganizationsWhereRightsHolderWillBeRemoved: conflicts.systemsInOtherOrganizationsWhereOrgIsRightsHolder.map(mapEntityFromOrg)
                };
            } else {
                this.conflictsModel = null;
            }

            this.consequencesAccepted = !this.conflictsDetected;
        }

        copyToClipBoard() {
            //Trigger additional spacing between consequece rows
            this.isCopyingConsequencesToClipBoard = true;

            //Allow angular to render
            setTimeout(() => {
                try {
                    Utility.copyPageContentToClipBoard("consequencesOverview");
                } catch (e) {
                    console.log("Failed to copy consequences to clipboard", e);

                }

                // Remove spacing
                this.isCopyingConsequencesToClipBoard = false;
                this.notify.addSuccessMessage("Konsekvenser er kopieret til udklipsholderen");
            },
                1);
        }

        dismiss() {
            this.$scope.$dismiss();
        }

        submit() {
            if (confirm(`Er du sikker på, at du vil slette "${this.orgName}"?`)) {
                this.organizationApiService.deleteOrganization(this.orgToDelete.uuid, this.consequencesAccepted)
                    .then((success) => {
                        this.notify.addSuccessMessage("Organisationen blev slettet");
                        this.$scope.$close(true);
                    },
                        (error) => {
                            console.error("Error deleting org:", this.orgToDelete, "Error:", error);
                            this.notify.addErrorMessage("Fejl ifm. sletning af organisationen!");
                        });
            }
        }
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

module Kitos.LocalAdmin.FkOrganisation.Modals {
    export enum FKOrganisationImportFlow {
        Create = "create",
        Update = "update"
    }

    export interface IFKOrganisationImportDialogFactory {
        open(flow: FKOrganisationImportFlow, organizationUuid: string, synchronizationDepth: number | null): ng.ui.bootstrap.IModalInstanceService
    }

    export class FKOrganisationImportDialogFactory implements IFKOrganisationImportDialogFactory {
        static $inject = ["$uibModal"];
        constructor(private readonly $uibModal: ng.ui.bootstrap.IModalService) { }

        open(flow: FKOrganisationImportFlow, organizationUuid: string, synchronizationDepth: number | null): ng.ui.bootstrap.IModalInstanceService {
            return this.$uibModal.open({
                windowClass: "modal fade in wide-modal",
                templateUrl: "app/components/local-config/import/fk-organization-import-config-import-modal.view.html",
                controller: FKOrganisationImportController,
                controllerAs: "vm",
                resolve: {
                    "flow": [() => flow],
                    "orgUuid": [() => organizationUuid],
                    "synchronizationDepth": [() => synchronizationDepth]
                },
                backdrop: "static", //Make sure accidental click outside the modal does not close it during the import process
            });
        }
    }

    class FKOrganisationImportController {
        static $inject = ["flow", "orgUuid", "synchronizationDepth", "stsOrganizationSyncService", "$uibModalInstance", "notify"];
        isConsequencesCollapsed: boolean = false;
        isHierarchyCollapsed: boolean = false;
        busy: boolean = false;
        updating: boolean = false;
        loadingHierarchy: boolean | null;
        consequencesAwaitingApproval: Array<Models.Api.Organization.ConnectionUpdateOrganizationUnitConsequenceDTO> | null = null;
        fkOrgHierarchy: Kitos.Shared.Components.Organization.IOrganizationTreeComponentOptions | null = null;
        constructor(
            readonly flow: FKOrganisationImportFlow,
            private readonly organizationUuid: string,
            initialImportDepth: number | null,
            private readonly stsOrganizationSyncService: Services.Organization.IStsOrganizationSyncService,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly notify) {
            this.fkOrgHierarchy = {
                availableLevels: initialImportDepth,
                root: null
            }
        }

        $onInit() {
            this.loadingHierarchy = true;
            this.isConsequencesCollapsed = this.isHierarchyCollapsed = false;
            this.consequencesAwaitingApproval = null;
            this.stsOrganizationSyncService
                .getSnapshot(this.organizationUuid)
                .then(root => {
                    this.fkOrgHierarchy.root = this.createNodeVm(root);
                    this.loadingHierarchy = false;
                }, error => {
                    console.error(error);
                    this.notify.addErrorMessage("Fejl i indlæsning af hierarkiet fra FK Organisation. Genindlæs siden og prøv igen.");
                });
        }

        private createNodeVm(stsNode: Kitos.Models.Api.Organization.StsOrganizationOrgUnitDTO): Kitos.Shared.Components.Organization.IOrganizationTreeNode {
            return {
                id: stsNode.uuid,
                name: stsNode.name,
                origin: Kitos.Models.Api.Organization.OrganizationUnitOrigin.STS_Organisation,
                nodes: stsNode.children.map(child => this.createNodeVm(child))
            };
        }

        cancel() {
            if (!this.busy) {
                this.$uibModalInstance.dismiss();
            }
        }

        performImport() {
            if (!this.busy) {
                this.busy = true;
                if (this.flow === FKOrganisationImportFlow.Create) {
                    this.createConnection();
                } else if (this.flow === FKOrganisationImportFlow.Update) {
                    this.updateConnection();
                }
            }
        }

        acceptConsequences() {
            this.performUpdate();
        }

        rejectConsequences() {
            this.updating = false;
            this.consequencesAwaitingApproval = null;
            this.isHierarchyCollapsed = false;
        }

        private performUpdate() {
            
            this.updating = true;
            this.busy = true;

            this.stsOrganizationSyncService.updateConnection(this.organizationUuid, this.fkOrgHierarchy.availableLevels)
                .then(() => {
                    this.closeDialog();
                }, error => {
                    console.error("Error: ", error);
                    this.updating = false;
                    this.busy = false;
                });
        }

        private createConnection() {
            this.stsOrganizationSyncService
                .createConnection(this.organizationUuid, this.fkOrgHierarchy.availableLevels)
                .then(() => {
                    this.closeDialog();
                }, error => {
                    console.error("Error:", error);
                    this.busy = false;
                    this.notify.addErrorMessage("Fejl ifm. oprettelse af forbindelsen. Prøv igen.");
                });
        }

        private closeDialog() {
            this.busy = false;
            this.$uibModalInstance.close();
        }

        private updateConnection() {
            this.isConsequencesCollapsed = false;
            this.stsOrganizationSyncService
                .getConnectionUpdateConsequences(this.organizationUuid, this.fkOrgHierarchy.availableLevels)
                .then(consequences => {
                    if (consequences.consequences.length > 0) {
                        this.consequencesAwaitingApproval = consequences.consequences;
                        this.busy = false;
                    } else {
                        this.performUpdate();
                    }
                }, error => {
                    console.error(error);
                    this.busy = false;
                    this.notify.addErrorMessage("Fejl ifm. oprettelse af opdateringen. Prøv igen.");
                });
        }
    }

    app.service("fkOrganisationImportDialogFactory", FKOrganisationImportDialogFactory)
}
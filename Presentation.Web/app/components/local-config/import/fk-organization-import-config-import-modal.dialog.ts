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
                windowClass: "modal fade in",
                templateUrl: "app/components/local-config/import/fk-organization-import-config-import-modal.view.html",
                controller: FKOrganisationImportController,
                controllerAs: "vm",
                resolve: {
                    "flow": [() => flow],
                    "orgUuid": [() => organizationUuid],
                    "synchronizationDepth": [() => synchronizationDepth]
                },
                backdrop: "static" //Make sure accidental click outside the modal does not close it during the import process
            });
        }
    }

    class FKOrganisationImportController {
        static $inject = ["flow", "orgUuid", "synchronizationDepth", "stsOrganizationSyncService", "$uibModalInstance"];
        busy: boolean = false;
        loadingHierarchy: boolean | null;
        fkOrgHierarchy: Kitos.Shared.Components.Organization.IOrganizationTreeComponentOptions | null = null;
        constructor(
            readonly flow: FKOrganisationImportFlow,
            private readonly organizationUuid: string,
            initialImportDepth: number | null,
            private readonly stsOrganizationSyncService: Services.Organization.IStsOrganizationSyncService,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
            this.fkOrgHierarchy = {
                availableLevels: initialImportDepth,
                root: null
            }
        }

        $onInit() {
            this.loadingHierarchy = true;
            this.stsOrganizationSyncService
                .getSnapshot(this.organizationUuid)
                .then(root => {
                    this.fkOrgHierarchy.root = this.createNodeVm(root);
                    this.loadingHierarchy = false;
                });
        }

        private createNodeVm(stsNode: Kitos.Models.Api.Organization.StsOrganizationOrgUnitDTO): Kitos.Shared.Components.Organization.IOrganizationTreeNode {
            return {
                id: stsNode.uuid,
                name: stsNode.name,
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
                this.stsOrganizationSyncService
                    .createConnection(this.organizationUuid, this.fkOrgHierarchy.availableLevels)
                    .then(() => {
                        this.busy = false;
                        this.$uibModalInstance.close();
                    }, error => {
                        console.log("Error:", error);
                        this.busy = false;
                    });
            }
        }
    }

    app.service("fkOrganisationImportDialogFactory", FKOrganisationImportDialogFactory)
}
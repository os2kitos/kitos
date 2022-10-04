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
                }
            });
        }
    }

    class FKOrganisationImportController {
        static $inject = ["flow", "orgUuid", "synchronizationDepth", "stsOrganizationSyncService"];
        constructor(
            readonly flow: FKOrganisationImportFlow,
            private readonly organizationUuid: string,
            private readonly synchronizationDepth: number | null,
            private readonly stsOrganizationSyncService: Services.Organization.IStsOrganizationSyncService) {
            //TODO: Once this becomes an edit dialog, also add the sy c depth
        }

        $onInit() {
            //TODO: Use this to begin load of the hierarchy from FK Organization so that it is ready for preview!
        }

        performImport() {
            this.stsOrganizationSyncService
                .createConnection(this.organizationUuid, this.synchronizationDepth)
                .then(() => {
                    //TODO: Then---close the modal instance
                });
        }
    }

    app.service("fkOrganisationImportDialogFactory", FKOrganisationImportDialogFactory)
}
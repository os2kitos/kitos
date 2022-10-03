module Kitos.LocalAdmin.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                currentOrganizationUuid: "="
            },
            controller: FkOrganizationImportController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/import/fk-organization-import-config.view.html`
        };
    }

    enum CommandCategory {
        Create = "create",
        Update = "update",
        Delete = "delete"
    }

    interface IFkOrganizationCommand {
        id: string
        text: string
        onClick: () => void
        enabled: boolean
        category: CommandCategory
    }

    interface IFkOrganizationSynchronizationStatus {
        connected: boolean
        synchronizationDepth: number | null
    }

    interface IFkOrganizationImportController extends ng.IComponentController {
        currentOrganizationUuid: string
        accessGranted: boolean | null;
        accessError: string | null
        synchronizationStatus: IFkOrganizationSynchronizationStatus | null
        commands: Array<IFkOrganizationCommand> | null;
    }

    class FkOrganizationImportController implements IFkOrganizationImportController {
        currentOrganizationUuid: string | null = null; //note set by bindings
        accessGranted: boolean | null = null;
        accessError: string | null = null;
        synchronizationStatus: IFkOrganizationSynchronizationStatus | null = null;
        commands: Array<IFkOrganizationCommand> | null = null;

        static $inject: string[] = ["stsOrganizationSyncService"];
        constructor(private readonly stsOrganizationSyncService: Kitos.Services.Organization.IStsOrganizationSyncService) {
        }

        $onInit() {
            if (this.currentOrganizationUuid === null) {
                console.error("missing attribute: 'currentOrganizationUuid'");
            } else {
                this.loadState();
            }
        }

        private loadState() {
            this.stsOrganizationSyncService
                .getConnectionStatus(this.currentOrganizationUuid)
                .then(result => {
                    this.bindAccessProperties(result);
                    this.bindSynchronizationStatus(result);
                    this.bindCommands(result);
                }, error => {
                    console.error(error);
                    this.accessGranted = false;
                    this.accessError = "Der skete en fejl ifm. tjek for forbindelsen til FK Organisation. Genindlæs venligst siden for at prøve igen.";
                });
        }

        private bindCommands(result: Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO) {
            const newCommands: Array<IFkOrganizationCommand> = [];
            if (result.connected) {
                newCommands.push({
                    id: "updateSync",
                    text: "Rediger",
                    category: CommandCategory.Update,
                    enabled: result.canUpdateConnection,
                    onClick: () => {
                        //TODO: https://os2web.atlassian.net/browse/KITOSUDV-3313
                        // NOTE: Remember to rebind
                        console.log("UPDATE");
                    }
                });
                newCommands.push({
                    id: "breakSync",
                    text: "Afbryd",
                    category: CommandCategory.Delete,
                    enabled: result.canDeleteConnection,
                    onClick: () => {
                        //TODO: https://os2web.atlassian.net/browse/KITOSUDV-3320
                        // NOTE: Remember to rebind
                        console.log("DELETE");
                    }
                });
            } else {
                newCommands.push({
                    id: "createSync",
                    text: "Forbind",
                    category: CommandCategory.Create,
                    enabled: result.canCreateConnection,
                    onClick: () => {
                        //TODO - open a dialog!
                        console.log("CREATE");
                        //TODO: Open the dialog in stead
                        this.stsOrganizationSyncService
                            .createConnection(this.currentOrganizationUuid, null) //TODO: Get second arg
                            .then(() => this.loadState());
                    }
                });
            }

            this.commands = newCommands;
        }

        private bindSynchronizationStatus(result: Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO) {
            this.synchronizationStatus = {
                connected: result.connected,
                synchronizationDepth: result.synchronizationDepth
            };
            //TODO: Bind the available commands
            //TODO: Consider
            // TODO: - Dialog or in-page flow?
            // TODO: Buttons... same color or?..
        }

        private bindAccessProperties(result: Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO) {
            if (result.accessStatus.accessGranted) {
                this.accessGranted = true;
            } else {
                this.accessGranted = false;
                switch (result.accessStatus.error) {
                    case Models.Api.Organization.CheckConnectionError.ExistingServiceAgreementIssue:
                        this.accessError = "Der er problemer med den eksisterende serviceaftale, der giver KITOS adgang til data fra din kommune i FK Organisatoin. Kontakt venligst den KITOS ansvarlige i din kommune for hjælp.";
                        break;
                    case Models.Api.Organization.CheckConnectionError.InvalidCvrOnOrganization:
                        this.accessError = "Der enten mangler eller er registreret et ugyldigt CVR nummer på din kommune i KITOS.";
                        break;
                    case Models.Api.Organization.CheckConnectionError.MissingServiceAgreement:
                        this.accessError = "Din organisation mangler en gyldig serviceaftale der giver KITOS adgang til data fra din kommune i FK Organisation. Kontakt venligst den KITOS ansvarlige i din kommune for hjælp.";
                        break;
                    case Models.Api.Organization.CheckConnectionError.Unknown: //intended fallthrough
                    default:
                        this.accessError = "Der skete en ukendt fejl ifm. tjek for forbindelsen til FK Organisation. Genindlæs venligst siden for at prøve igen.";
                        break;
                }
            }
        }
    }
    angular.module("app")
        .component("fkOrgnizationImportConfig", setupComponent());
}
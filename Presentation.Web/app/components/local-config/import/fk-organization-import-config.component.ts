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

    interface IFkOrganizationImportController extends ng.IComponentController {
        currentOrganizationUuid: string
    }

    class FkOrganizationImportController implements IFkOrganizationImportController {
        currentOrganizationUuid: string | null = null; //note set by bindings
        accessGranted: boolean | null = null;
        accessError: string | null = null;

        static $inject: string[] = ["stsOrganizationSyncService"];
        constructor(private readonly stsOrganizationSyncService: Kitos.Services.Organization.IStsOrganizationSyncService) {
        }

        $onInit() {
            if (this.currentOrganizationUuid === null) {
                console.error("missing attribute: 'currentOrganizationUuid'");
            } else {
                this.stsOrganizationSyncService
                    .getConnectionStatus(this.currentOrganizationUuid)
                    .then(result => {
                        if (result.accessStatus.accessGranted) {
                            this.accessGranted = true;
                        } else {
                            this.accessGranted = false;
                            switch (result.accessStatus.error) {
                                case Models.Api.Organization.CheckConnectionError.ExistingServiceAgreementIssue:
                                    this.accessError = "Der er problemer med den eksisterende serviceaftale, der giver KITOS adgang til data fra din kommune i FK Organisatoin. Kontakt venligst den KITOS ansvarlige i din kommune for hjælp."
                                    break;
                                case Models.Api.Organization.CheckConnectionError.InvalidCvrOnOrganization:
                                    this.accessError = "Der enten mangler eller er registreret et ugyldigt CVR nummer på din kommune i KITOS."
                                    break;
                                case Models.Api.Organization.CheckConnectionError.MissingServiceAgreement:
                                    this.accessError = "Din organisation mangler en gyldig serviceaftale der giver KITOS adgang til data fra din kommune i FK Organisation. Kontakt venligst den KITOS ansvarlige i din kommune for hjælp."
                                    break;
                                case Models.Api.Organization.CheckConnectionError.Unknown: //intended fallthrough
                                default:
                                    this.accessError = "Der skete en ukendt fejl ifm. tjek for forbindelsen til FK Organisation. Genindlæs venligst siden for at prøve igen."
                                    break;
                            }
                        }
                    }, error => {
                        console.error(error);
                        this.accessGranted = false;
                        this.accessError = "Der skete en fejl ifm. tjek for forbindelsen til FK Organisation. Genindlæs venligst siden for at prøve igen."
                    });
            }
        }
    }
    angular.module("app")
        .component("fkOrgnizationImportConfig", setupComponent());
}
module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                title: "@",
                options: "<",
                configuration: "<",
                unitUuid: "@",
                organizationUuid: "@"
            },
            controller: OrganizationUnitMigrationTableController,
            controllerAs: "ctrl",
            templateUrl: "app/components/org/structure/org-unit-migration-table.view.html"
        }
    }

    export enum MigrationTableColumnType {
        Text = 0,
        Link = 1
    }

    export interface IMigrationTableColumn{
        title: string;
        property: string;
        type: MigrationTableColumnType;
        cssClass?: string;
    }

    interface IOrganizationUnitMigrationTableController extends ng.IComponentController {
        title: string;
        options: IOrganizationUnitMigrationOptions;
        configuration: IMigrationTableColumn[];
        unitUuid: string;
        organizationUuid: string;
    }

    class OrganizationUnitMigrationTableController implements IOrganizationUnitMigrationTableController {
        title: string | null = null;
        options: IOrganizationUnitMigrationOptions | null = null;
        configuration: IMigrationTableColumn[] | null = null;
        unitUuid: string | null = null;
        organizationUuid: string | null = null;
        root: IOrganizationUnitMigrationRoot;
        columnTypes = MigrationTableColumnType;
        
        static $inject: string[] = ["organizationRegistrationsService", "notify"];
        constructor(private readonly organizationRegistrationsService: Services.Organization.IOrganizationRegistrationsService,
            private readonly notify) {
        }

        $onInit() {
            if (this.title === null) {
                console.error("missing migration table attribute: 'title'");
                return;
            }
            if (this.options === null) {
                console.error(`missing migration table attribute: 'options' for table with title: ${this.title}`);
                return;
            }
            if (this.configuration === null) {
                console.error(`missing migration table attribute: 'configuration' for table with title: ${this.title}`);
                return;
            }
            if (this.unitUuid === null) {
                console.error(`missing migration table attribute: 'unitUuid' for table with title: ${this.title}`);
                return;
            }
            if (this.organizationUuid === null) {
                console.error(`missing migration table attribute: 'organizationId' for table with title: ${this.title}`);
                return;
            }

            this.root = this.options.root;
        }

        getPageRoute(id: number) {
            return `${this.options.dataRelatedPage}({ id: ${id} })`;
        }

        registrationSelected() {
            const callback = this.options.selectedRegistrationChanged;
            if (callback !== undefined) {
                callback();
            }
        }

        registrationGroupSelected(root) {
            const callback = this.options.selectedRegistrationGroupChanged;
            if (callback !== undefined) {
                callback(root);
            }
        }

        delete(registration: Models.ViewModel.Organization.IOrganizationUnitRegistration) {
            if (this.options.checkIsBusy()) {
                    return;
            }
            if (!confirm('Er du sikker på, at du vil slette registreringen?')) {
                return;
            }
            this.options.setIsBusy(true);

            registration.selected = true;
            const request = this.createChangeRequest(registration);
            registration.selected = false;

            this.organizationRegistrationsService.deleteSelectedRegistrations(this.organizationUuid, this.unitUuid, request)
                .then(() => {
                    this.options.refreshData();
                    this.options.setIsBusy(false);
                },
                error => {
                    console.log(error);
                    this.notify.addErrorMessage("Failed to deleted the selected unit");
                    this.options.setIsBusy(false);
                });

            this.options.setIsBusy(false);
        }

        private createChangeRequest(request: Models.ViewModel.Organization.IOrganizationUnitRegistration): Models.Api.Organization.OrganizationUnitRegistrationChangeRequestDto {

            const roles = new Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>();
            const contractRegistration = new Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>();
            const internalPayments = new Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>();
            const externalPayments = new Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>();
            const responsibleSystems = new Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>();
            const relevantSystems = new Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>();

            switch (this.options.type) {
                case Models.ViewModel.Organization.OrganizationRegistrationOption.Roles:
                    roles.push(request);
                    break;
                case Models.ViewModel.Organization.OrganizationRegistrationOption.ContractRegistrations:
                    contractRegistration.push(request);
                    break;
                case Models.ViewModel.Organization.OrganizationRegistrationOption.InternalPayments:
                    internalPayments.push(request);
                    break;
                case Models.ViewModel.Organization.OrganizationRegistrationOption.ExternalPayments:
                    externalPayments.push(request);
                    break;
                case Models.ViewModel.Organization.OrganizationRegistrationOption.RelevantSystems:
                    relevantSystems.push(request);
                    break;
                case Models.ViewModel.Organization.OrganizationRegistrationOption.ResponsibleSystems:
                    responsibleSystems.push(request);
                    break;
                default:
                    throw `Wrong OrganizationRegistrationOption: ${this.options.type}`;
            }

            return Helpers.OrganizationRegistrationHelper.createChangeRequest(contractRegistration, externalPayments, internalPayments, roles, relevantSystems, responsibleSystems);
        }
    }

    angular.module("app")
        .component("orgUnitMigrationTable", setupComponent());
}
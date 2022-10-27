module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                title: "@",
                options: "<",
                configuration: "<",
                unitId: "<"
            },
            controller: OrganizationUnitMigrationTableController,
            controllerAs: "ctrl",
            templateUrl: "app/components/org/structure/org-unit-migration-table.view.html"
        }
    }

    export interface IMigrationTableColumn{
        title: string;
        property: string;
        cssClass?: string;
    }

    interface IOrganizationUnitMigrationTableController extends ng.IComponentController {
        title: string;
        options: IOrganizationUnitMigrationOptions;
        configuration: IMigrationTableColumn[];
        unitId: number;
    }

    class OrganizationUnitMigrationTableController implements IOrganizationUnitMigrationTableController {
        title: string | null = null;
        options: IOrganizationUnitMigrationOptions | null;
        configuration: IMigrationTableColumn[] | null = null;
        unitId: number | null = null;
        root: IOrganizationUnitMigrationRoot;

        static $inject: string[] = ["organizationUnitService"];
        constructor(private readonly organizationUnitService: Services.Organization.IOrganizationUnitService) {
        }

        $onInit() {
            if (this.title === null) {
                console.error("missing migration table attribute: 'title'");
            }
            if (this.options === null) {
                console.error("missing migration table attribute: 'options'");
            }
            if (this.configuration === null) {
                console.error("missing migration table attribute: 'configuration'");
            }
            if (this.unitId === null) {
                console.error("missing migration table attribute: 'unitId'");
            }

            this.root = this.options.root;
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

        delete(registration: Models.Organization.IOrganizationUnitRegistration) {
            if (!confirm('Er du sikker på, at du vil slette registreringen?')) {
                return;
            }

            registration.selected = true;
            const request = this.createChangeRequest(registration);
            registration.selected = false;

            this.organizationUnitService.deleteSelectedRegistrations(this.unitId, request)
                .then(() => this.options.refreshData());
        }

        createChangeRequest(request: Models.Organization.IOrganizationUnitRegistration): Models.Api.Organization.OrganizationRegistrationChangeRequest {

            const roles = new Array<Models.Organization.IOrganizationUnitRegistration>();
            const contractRegistration = new Array<Models.Organization.IOrganizationUnitRegistration>();
            const internalPayments = new Array<Models.Organization.IOrganizationUnitRegistration>();
            const externalPayments = new Array<Models.Organization.IOrganizationUnitRegistration>();
            const responsibleSystems = new Array<Models.Organization.IOrganizationUnitRegistration>();
            const relevantSystems = new Array<Models.Organization.IOrganizationUnitRegistration>();

            switch (this.options.type) {
                case Models.Organization.OrganizationRegistrationOption.Roles:
                    roles.push(request);
                    break;
                case Models.Organization.OrganizationRegistrationOption.ContractRegistrations:
                    contractRegistration.push(request);
                    break;
                case Models.Organization.OrganizationRegistrationOption.InternalPayments:
                    internalPayments.push(request);
                    break;
                case Models.Organization.OrganizationRegistrationOption.ExternalPayments:
                    externalPayments.push(request);
                    break;
                case Models.Organization.OrganizationRegistrationOption.RelevantSystems:
                    relevantSystems.push(request);
                    break;
                case Models.Organization.OrganizationRegistrationOption.ResponsibleSystems:
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
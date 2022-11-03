module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                title: "@",
                options: "<",
                configuration: "<",
                unitId: "<",
                organizationId: "<",
                closeModal: "&"
            },
            controller: OrganizationUnitMigrationTableController,
            controllerAs: "ctrl",
            templateUrl: "app/components/org/structure/org-unit-migration-table.view.html"
        }
    }

    export interface IMigrationTableColumn{
        title: string;
        property: string;
        isLink?: boolean;
        cssClass?: string;
    }

    interface IOrganizationUnitMigrationTableController extends ng.IComponentController {
        title: string;
        options: IOrganizationUnitMigrationOptions;
        configuration: IMigrationTableColumn[];
        unitId: number;
        organizationId: number;
        closeModal: () => void;
    }

    class OrganizationUnitMigrationTableController implements IOrganizationUnitMigrationTableController {
        title: string | null = null;
        options: IOrganizationUnitMigrationOptions | null;
        configuration: IMigrationTableColumn[] | null = null;
        unitId: number | null = null;
        organizationId: number | null = null;
        closeModal: () => void;
        root: IOrganizationUnitMigrationRoot;

        static $inject: string[] = ["organizationUnitService", "notify", "$state"];
        constructor(private readonly organizationUnitService: Services.Organization.IOrganizationUnitService,
            private readonly notify,
            private readonly $state) {
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
            if (this.organizationId === null) {
                console.error("missing migration table attribute: 'organizationId'");
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

            this.organizationUnitService.deleteSelectedRegistrations(this.organizationId, this.unitId, request)
                .then(() => this.options.refreshData()),
                error => {
                    console.log(error);
                    this.notify.addErrorMessage("Failed to delete the selected unit");
                };
        }

        navigateTo(id: number) {
            this.$state.go(this.options.dataRelatedPage, { id: id })
                .then(() => this.closeModal());
        }

        private createChangeRequest(request: Models.ViewModel.Organization.IOrganizationUnitRegistration): Models.Api.Organization.OrganizationRegistrationChangeRequestDto {

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
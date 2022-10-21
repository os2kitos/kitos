module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                title: "@",
                options: "=",
                configuration: "=",
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

        static $inject: string[] = ["organizationRegistrationsService"];
        constructor(private readonly organizationRegistrationsService: Services.Organization.IOrganizationRegistrationsService) {
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

        delete(registration) {
            if (!confirm('Er du sikker på, at du vil slette INSERT REST OF THE TEXT?')) {
                return;
            }

            const request = {
                id: registration.id,
                type: registration.type
            } as Models.Api.Organization.OrganizationRegistrationDetailsDto;

            this.organizationRegistrationsService.deleteSingleRegistration(this.unitId, request)
                .then(() => this.options.refreshData());
        }
    }

    angular.module("app")
        .component("orgUnitMigrationTable", setupComponent());
}
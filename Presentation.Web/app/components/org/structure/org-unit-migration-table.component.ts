module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                title: "@",
                options: "=",
                configuration: "="
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
    }

    class OrganizationUnitMigrationTableController implements IOrganizationUnitMigrationTableController {
        title: string | null = null;
        options: IOrganizationUnitMigrationOptions;
        configuration: IMigrationTableColumn[] | null = null;
        root: IOrganizationUnitMigrationRoot;

        $onInit() {
            /*if (this.configuration === null) {
                console.error("missing migration table attribute: 'configuration'");
            }*/
            if (this.title === null) {
                console.error("missing migration table attribute: 'title'");
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
            //TODO: delete registration
        }
    }

    angular.module("app")
        .component("orgUnitMigrationTable", setupComponent());
}
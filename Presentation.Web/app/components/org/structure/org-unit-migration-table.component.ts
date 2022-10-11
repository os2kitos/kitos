module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                title: "@",
                options: "="
            },
            controller: OrganizationUnitMigrationTableController,
            controllerAs: "ctrl",
            templateUrl: "app/components/org/structure/org-unit-migration-table.view.html"
        }
    }

    interface IOrganizationUnitMigrationTableController extends ng.IComponentController {
        title: string;
        options: IOrganizationUnitMigrationOptions;
    }

    class OrganizationUnitMigrationTableController implements IOrganizationUnitMigrationTableController {
        title: string | null = null;
        options: IOrganizationUnitMigrationOptions | null = null;
        root: IOrganizationUnitMigrationRoot;

        $onInit() {
            this.root = this.options.root;
        }

        unitSelected(unit, event) {
            const callback = this.options.selectedUnitChanged;

        }

        unitGroupSelected(group, event) {
            const callback = this.options.selectedUnitGroupChanged;

        }
    }

    angular.module("app")
        .component("orgUnitMigrationTable", setupComponent());
}
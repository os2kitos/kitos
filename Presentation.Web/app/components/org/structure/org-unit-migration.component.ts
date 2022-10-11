module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return{
            bindings: {
                organizationId: "<"
            },
            controller: OrganizationUnitMigrationController,
            controllerAs: "ctrl",
            templateUrl: "app/components/org/structure/org-unit-migration.view.html"
        }
    }

    export interface IOrganizationUnitMigrationOptions {
        root: IOrganizationUnitMigrationRoot;
        selectedUnitChanged?: (unit: IOrganizationUnitMigration) => void;
        selectedUnitGroupChanged?: (root: IOrganizationUnitMigrationRoot) => void;
    }

    export interface IOrganizationUnitMigrationRoot extends Models.ViewModel.Organization.IHasSelection {
        children: IOrganizationUnitMigration[];
    }

    export interface IOrganizationUnitMigration extends Models.ViewModel.Organization.IHasSelection {
        id: number;
        text: string;
        targetUnitId?: number;
        optionalObjectContext?: any;
    }

    interface IOrganizationUnitMigrationController extends ng.IComponentController {
        organizationId: number;
    }

    class OrganizationUnitMigrationController implements IOrganizationUnitMigrationController {
        organizationId: number | null = null;
        roles: any;
        payments: any;
        internalContractRegistrations: any;
        externalContractRegistrations: any;
        relevantSystemRegistrations: any;
        responsibleSystemRegistrations: any;
        organizations: any;
        selectedOrg: any;

        //TODO: inject data
        static $inject: string[] = [];
        constructor() {

        }

        $onInit() {
            this.roles = this.createOptions();
            this.payments = this.createOptions();
            this.internalContractRegistrations = this.createOptions();
            this.externalContractRegistrations = this.createOptions();
            this.relevantSystemRegistrations = this.createOptions();
            this.responsibleSystemRegistrations = this.createOptions();
            this.organizations = [{ id: 1, name: "Organization1" }, { id: 2, name: "Organization2" }];
            this.selectedOrg = {};
        }

        changeUnitStatus(unit: IOrganizationUnitMigration) {
            unit.selected = !unit.selected;
        }

        changeUnitGroupStatus(root: IOrganizationUnitMigrationRoot) {
            const areAllSelected = root.children.filter(x => !x.selected).length < 1;
            const targetValue = !areAllSelected;
            Helpers.CheckboxSelectionHelper.setSelectGroupToValue(root.children, targetValue);
        }

        private createOptions() {
            return {
                root: {
                    selected: false,
                    children: [{ text: "test" }, { text: "test2" }]
                },
                selectedUnitChanged: (unit: IOrganizationUnitMigration) => this.changeUnitStatus(unit),
                selectedUnitGroupChanged: (root: IOrganizationUnitMigrationRoot) => this.changeUnitGroupStatus(root)
            };
        }
    }

    angular.module("app")
        .component("orgUnitMigration", setupComponent());
}
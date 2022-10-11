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
        selectedRegistrationChanged?: () => void;
        selectedRegistrationGroupChanged?: (root: IOrganizationUnitMigrationRoot) => void;
    }

    export interface IOrganizationUnitMigrationRoot extends Models.ViewModel.Organization.IHasSelection {
        children: IOrganizationUnitRegistration[];
    }

    export interface IOrganizationUnitRegistration extends Models.ViewModel.Organization.IHasSelection {
        id: number;
        text: string;
        targetUnit?: Models.Generic.NamedEntity.NamedEntityDTO;
        optionalObjectContext?: any;
    }

    interface IOrganizationUnitMigrationController extends ng.IComponentController {
        organizationId: number;
    }

    class OrganizationUnitMigrationController implements IOrganizationUnitMigrationController {
        organizationId: number | null = null;
        anySelections = false;
        allSelections = false;

        roles: IOrganizationUnitMigrationOptions;
        payments: IOrganizationUnitMigrationOptions;
        internalContractRegistrations: IOrganizationUnitMigrationOptions;
        externalContractRegistrations: IOrganizationUnitMigrationOptions;
        relevantSystemRegistrations: IOrganizationUnitMigrationOptions;
        responsibleSystemRegistrations: IOrganizationUnitMigrationOptions;
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

        deleteSelected() {
            //TODO: delete selected registrations
        }

        deleteUnit() {
            //TODO: delete unit
        }

        transfer() {
            //TODO: transfer registrations
        }

        setSelectedOrg() {
            const selectedRegistrations = this.collectSelectedRegistrations();
            selectedRegistrations.forEach(registration => registration.targetUnit = this.selectedOrg);
        }

        selectAll() {
            this.changeAllSelections(true);
            this.updateAnySelections();
        }

        deselectAll() {
            this.changeAllSelections(false);
            this.updateAnySelections();
        }

        updateAnySelections() {
            let anySelectionsFound = false;
            let allSelectionsFound = false;
            
            const roots = this.getAllRoots();
            var totalRegistrations = 0;
            roots.forEach(root => totalRegistrations += root.children.length);

            const numberOfSelectedRegistrations = this.collectSelectedRegistrations().length;

            if (numberOfSelectedRegistrations > 0) {
                anySelectionsFound = true;
            }
            if (numberOfSelectedRegistrations === totalRegistrations) {
                allSelectionsFound = true;
            }

            this.initiateGroupSelectionCheck();
            this.anySelections = anySelectionsFound;
            this.allSelections = allSelectionsFound;
        }

        changeRegistrationGroupStatus(root: IOrganizationUnitMigrationRoot) {
            const areAllSelected = root.children.filter(x => !x.selected).length < 1;
            const targetValue = !areAllSelected;
            root.selected = targetValue;
            Helpers.CheckboxSelectionHelper.setSelectGroupToValue(root.children, targetValue);
            this.updateAnySelections();
        }

        private changeAllSelections(targetValue: boolean) {
            const roots = this.getAllRoots();
            roots.filter(x => x.selected !== targetValue).forEach(root => this.changeRegistrationGroupStatus(root));
        }

        private initiateGroupSelectionCheck() {
            this.getAllRoots().forEach(group => this.checkAndChangeGroupSelectionStatus(group));
        }

        private checkAndChangeGroupSelectionStatus(groupRoot: IOrganizationUnitMigrationRoot) {
            const registrations = this.collectSelectedRegistrationsFromSource(groupRoot.children);

            if (registrations.length < groupRoot.children.length) {
                groupRoot.selected = false;
                return;
            }

            groupRoot.selected = true;
        }

        private collectSelectedRegistrations(): Array<IOrganizationUnitRegistration> {

            var result = [] as IOrganizationUnitRegistration[];

            const roots = this.getAllRoots();
            roots.forEach(root => {
                const res = this.collectSelectedRegistrationsFromSource(root.children);
                result = result.concat(res);
                console.log("");
            });

            return result;
        }

        private collectSelectedRegistrationsFromSource(sourceCollection: Array<IOrganizationUnitRegistration>): Array<IOrganizationUnitRegistration> {
            return sourceCollection.filter(x => x.selected);
        }

        private getAllRoots(): Array<IOrganizationUnitMigrationRoot> {
            const array = [] as IOrganizationUnitMigrationRoot[];
            return array
                .concat(this.roles.root)
                .concat(this.payments.root)
                .concat(this.internalContractRegistrations.root)
                .concat(this.externalContractRegistrations.root)
                .concat(this.relevantSystemRegistrations.root)
                .concat(this.responsibleSystemRegistrations.root);
        }

        private createOptions(): IOrganizationUnitMigrationOptions {
            return {
                root: {
                    selected: false,
                    children: [{ text: "test" }, { text: "test2" }]
                },
                selectedRegistrationChanged: () => this.updateAnySelections(),
                selectedRegistrationGroupChanged: (root: IOrganizationUnitMigrationRoot) => this.changeRegistrationGroupStatus(root)
            } as IOrganizationUnitMigrationOptions;
        }
    }

    angular.module("app")
        .component("orgUnitMigration", setupComponent());
}
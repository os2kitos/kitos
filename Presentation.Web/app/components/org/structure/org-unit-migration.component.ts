module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return{
            bindings: {
                organizationId: "<",
                unitId: "<"
            },
            controller: OrganizationUnitMigrationController,
            controllerAs: "ctrl",
            templateUrl: "app/components/org/structure/org-unit-migration.view.html",
        }
    }

    export interface IDropdownUnit {
        id: number;
        name: string;
        text: string;
        level: number;
    }

    export interface IOrganizationUnitMigrationOptions {
        root: IOrganizationUnitMigrationRoot;
        hasExtraDataColumn?: boolean;
        hasExtraIndexColumn?: boolean;
        selectedRegistrationChanged?: () => void;
        selectedRegistrationGroupChanged?: (root: IOrganizationUnitMigrationRoot) => void;
    }

    export interface IOrganizationUnitMigrationRoot extends Models.ViewModel.Organization.IHasSelection {
        children: IOrganizationUnitRegistration[];
    }

    export interface IOrganizationUnitRegistration extends Models.ViewModel.Organization.IHasSelection {
        id: number;
        text: string;
        objectId?: number;
        objectName?: string;
        index?: number;

        targetUnitId?: number;
        targetUnitName?: string;
        optionalObjectContext?: any;
    }

    interface IOrganizationUnitMigrationController extends ng.IComponentController {
        organizationId: number;
        unitId: number;
    }

    class OrganizationUnitMigrationController implements IOrganizationUnitMigrationController {
        organizationId: number | null = null;
        unitId: number | null = null;
        anySelections = false;
        allSelections = false;

        roles: IOrganizationUnitMigrationOptions;
        internalPayments: IOrganizationUnitMigrationOptions;
        externalPayments: IOrganizationUnitMigrationOptions;
        contractRegistrations: IOrganizationUnitMigrationOptions;
        relevantSystemRegistrations: IOrganizationUnitMigrationOptions;
        responsibleSystemRegistrations: IOrganizationUnitMigrationOptions;
        organizations: IDropdownUnit[];
        selectedOrg: any;

        rolesTableConfig: IMigrationTableColumn[];
        internalPaymentTableConfig: IMigrationTableColumn[];
        externalPaymentTableConfig: IMigrationTableColumn[];
        contractTableConfig: IMigrationTableColumn[];
        relevantSystemTableConfig: IMigrationTableColumn[];
        responsibleSystemTableConfig: IMigrationTableColumn[];
        
        static $inject: string[] = ["organizationRegistrationsService", "organizationUnitOdataService", "_"];
        constructor(private readonly organizationRegistrationsService: Services.Organization.IOrganizationRegistrationsService,
            private readonly organizationUnitOdataService: Services.Organization.IOrganizationUnitOdataService,
            private readonly _) {
        }

        $onInit() {
            if (this.organizationId === null) {
                console.error("missing attribute: 'organizationId'");
            }
            if (this.unitId === null) {
                console.error("missing attribute: 'unitId'");
            }

            this.createTableConfigurations();
            this.createNewOptions();
            this.getData();

            //TODO: get orgs from the api
            this.organizations = [];
            this.selectedOrg = {};
            var self = this;

            this.organizationUnitOdataService
                .getOrganizationUnits()
                .then(result => {
                    self._.addHierarchyLevelOnFlatAndSort(result, "Id", "ParentId");
                    var resultWithLevels = result as any;
                    self.organizations = self.organizations.concat(
                        resultWithLevels.map(
                            item => {
                                 return {
                                     id: item.Id,
                                     name: item.Name,
                                     level: item.$level,
                                     text: "&nbsp;&nbsp;&nbsp;&nbsp;".repeat(item.$level) + item.Name
                                 }
                            }));
                    console.log(self.organizations);
                });
        }

        deleteSelected() {
            if (!confirm('Er du sikker på, at du vil slette INSERT REST OF THE TEXT?')) {
                return;
            }

            const request = this.createChangeRequest();
            this.organizationRegistrationsService.deleteSelectedRegistrations(this.unitId, request)
                .then(() => this.getData());
        }

        transfer() {
            if (!confirm('Er du sikker på, at du vil INSERT REST OF THE TEXT?')) {
                return;
            }

            const request = this.createChangeRequest();
            this.organizationRegistrationsService.transferSelectedRegistrations(this.unitId, this.selectedOrg.id, request)
                .then(() => this.getData())
                .then(() => this.updateAnySelections());
        }

        setSelectedOrg() {
            const selectedRegistrations = this.collectSelectedRegistrations();
            selectedRegistrations.forEach(registration => {
                registration.targetUnitId = this.selectedOrg.id;
                registration.targetUnitName = this.selectedOrg.name;
            });
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
                .concat(this.internalPayments.root)
                .concat(this.externalPayments.root)
                .concat(this.contractRegistrations.root)
                .concat(this.relevantSystemRegistrations.root)
                .concat(this.responsibleSystemRegistrations.root);
        }
        
        private createOptions(hasExtraDataColumn: boolean = false, hasExtraIndexColumn: boolean = false): IOrganizationUnitMigrationOptions {
            return {
                root: {
                    selected: false,
                    children: []
                },
                hasExtraDataColumn: hasExtraDataColumn,
                hasExtraIndexColumn: hasExtraIndexColumn,
                selectedRegistrationChanged: () => this.updateAnySelections(),
                selectedRegistrationGroupChanged: (root: IOrganizationUnitMigrationRoot) => this.changeRegistrationGroupStatus(root)
            } as IOrganizationUnitMigrationOptions;
        }

        private mapOrganizationRegistrationsToOptions(registrations: Models.Api.Organization.OrganizationRegistrationDetailsDto[]): IOrganizationUnitRegistration[]{
            return registrations.map(res => {
                return {
                    id: res.id,
                    text: res.text,
                } as IOrganizationUnitRegistration;
            });
        }

        private mapOrganizationRegistrationsPaymentsToOptions(registrations: Models.Api.Organization.OrganizationRegistrationContractPaymentDto[]): IOrganizationUnitRegistration[]{
            return registrations.map(res => {
                return {
                    id: res.id,
                    text: res.text,
                    objectId: res.objectId,
                    objectName: res.objectName,
                    index: res.paymentIndex
                } as IOrganizationUnitRegistration;
            });
        }

        private createTableConfigurations() {
            this.rolesTableConfig = this.createStandardTableConfig("Roles");
            this.internalPaymentTableConfig = this.createPaymentTableConfig("Internal payments");
            this.externalPaymentTableConfig = this.createPaymentTableConfig("External payments");
            this.contractTableConfig = this.createStandardTableConfig("Contract registrations");
            this.relevantSystemTableConfig = this.createStandardTableConfig("Relevant systems");
            this.responsibleSystemTableConfig = this.createStandardTableConfig("Responsible systems");
        }

        private createNewOptions() {
            this.roles = this.createOptions();
            this.internalPayments = this.createOptions(true, true);
            this.externalPayments = this.createOptions(true, true);
            this.contractRegistrations = this.createOptions();
            this.relevantSystemRegistrations = this.createOptions(true);
            this.responsibleSystemRegistrations = this.createOptions();
        }

        getData() {
            this.organizationRegistrationsService.getRegistrations(this.unitId).then(response => {
                this.roles.root.children = this.mapOrganizationRegistrationsToOptions(response.roles);
                this.internalPayments.root.children = this.mapOrganizationRegistrationsPaymentsToOptions(response.internalPayments);
                this.externalPayments.root.children = this.mapOrganizationRegistrationsPaymentsToOptions(response.externalPayments);
                this.contractRegistrations.root.children = this.mapOrganizationRegistrationsToOptions(response.contractRegistrations);
                this.relevantSystemRegistrations.root.children = this.mapOrganizationRegistrationsToOptions(response.relevantSystemRegistrations);
                this.responsibleSystemRegistrations.root.children = this.mapOrganizationRegistrationsToOptions(response.responsibleSystemRegistrations);
            }, error => {
                console.error(error);
            });
        }

        private createStandardTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: title, property: "text" },
            ] as IMigrationTableColumn[];
        }

        private createPaymentTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: "Index", property: "index" },
                { title: title, property: "text" },
                { title: "Contract name", property: "objectName" }
            ] as IMigrationTableColumn[];
        }

        private createChangeRequest(): Models.Api.Organization.OrganizationRegistrationChangeRequest {
            const request = new Models.Api.Organization.OrganizationRegistrationChangeRequest();

            request.roles = this.roles.root.children.filter(x => x.selected).map(result => { return result.id });
            request.internalPayments = this.internalPayments.root.children.filter(x => x.selected).map(result => { return result.id });
            request.externalPayments = this.externalPayments.root.children.filter(x => x.selected).map(result => { return result.id });
            request.contractRegistrations = this.contractRegistrations.root.children.filter(x => x.selected).map(result => { return result.id });
            request.responsibleSystems = this.responsibleSystemRegistrations.root.children.filter(x => x.selected).map(result => { return result.id });
            request.relevantSystems = this.relevantSystemRegistrations.root.children.filter(x => x.selected).map(result => { return result.id });

            return request;
        }
    }

    angular.module("app")
        .component("orgUnitMigration", setupComponent());
}
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
        refreshData: () => void;
    }

    export interface IOrganizationUnitMigrationRoot extends Models.ViewModel.Organization.IHasSelection {
        children: IOrganizationUnitRegistration[];
        type: Models.Api.Organization.OrganizationRegistrationOption;
    }

    export interface IOrganizationUnitRegistration extends Models.ViewModel.Organization.IHasSelection {
        id: number;
        text: string;
        type: Models.Api.Organization.OrganizationRegistrationOption;
        objectId?: number;
        objectName?: string;
        index?: number;

        targetUnitId?: number;
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
        targetUnitSelected = false;
        shouldTransferBtnBeEnabled = false;

        roles: IOrganizationUnitMigrationOptions;
        internalPayments: IOrganizationUnitMigrationOptions;
        externalPayments: IOrganizationUnitMigrationOptions;
        contractRegistrations: IOrganizationUnitMigrationOptions;
        relevantSystemRegistrations: IOrganizationUnitMigrationOptions;
        responsibleSystemRegistrations: IOrganizationUnitMigrationOptions;
        organizations: IDropdownUnit[];
        orgUnits: Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[];
        selectedOrg: any;

        rolesTableConfig: IMigrationTableColumn[];
        internalPaymentTableConfig: IMigrationTableColumn[];
        externalPaymentTableConfig: IMigrationTableColumn[];
        contractTableConfig: IMigrationTableColumn[];
        relevantSystemTableConfig: IMigrationTableColumn[];
        responsibleSystemTableConfig: IMigrationTableColumn[];
        
        static $inject: string[] = ["organizationRegistrationsService", "organizationApiService"];
        constructor(private readonly organizationRegistrationsService: Services.Organization.IOrganizationRegistrationsService,
            private readonly organizationApiService: Services.IOrganizationApiService) {
        }

        $onInit() {
            if (this.organizationId === null) {
                console.error("missing attribute: 'organizationId'");
            }
            if (this.unitId === null) {
                console.error("missing attribute: 'unitId'");
            }

            this.createTableConfigurations();
            this.setupOptions();
            this.getData();

            this.organizations = [];
            this.orgUnits = [];

            this.organizationApiService.getOrganizationUnit(this.organizationId).then(result => {
                this.orgUnits = this.orgUnits.concat(Helpers.Select2OptionsFormatHelper.addIndentationToUnitChildren(result, 0));
            });
        }

        deleteSelected() {
            if (!confirm('Er du sikker på, at du vil slette de valgte registreringer?')) {
                return;
            }

            const request = this.createChangeRequest();
            this.organizationRegistrationsService.deleteSelectedRegistrations(this.unitId, request)
                .then(() => this.getData());
        }

        transfer() {
            if (!this.selectedOrg)
                return;
            if (!confirm(`Er du sikker på, at du vil overføre de valgte registreringer til "${this.selectedOrg.text}"?`)) {
                return;
            }

            const request = this.createChangeRequest();
            this.organizationRegistrationsService.transferSelectedRegistrations(this.unitId, this.selectedOrg.id, request)
                .then(() => this.getData())
                .then(() => this.updateAnySelections());
        }

        setSelectedOrg() {
            if (!this.selectedOrg.id)
                return;

            const selectedRegistrations = this.collectSelectedRegistrations();
            selectedRegistrations.forEach(registration => {
                registration.targetUnitId = this.selectedOrg.id;
            });

            this.checkShouldTransferBtnBeEnabled();
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
            this.checkShouldTransferBtnBeEnabled();
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

        private checkShouldTransferBtnBeEnabled() {
            this.checkIsUnitSelected();
            if (this.anySelections && this.targetUnitSelected) {
                this.shouldTransferBtnBeEnabled = true;
                return;
            }

            this.shouldTransferBtnBeEnabled = false;
        }

        private checkIsUnitSelected() {
            if (!this.selectedOrg?.id) {
                this.targetUnitSelected = false;
                return;
            }

            this.targetUnitSelected = true;
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

        private createTableConfigurations() {
            this.rolesTableConfig = this.createStandardTableConfig("Roles");
            this.internalPaymentTableConfig = this.createPaymentTableConfig("Internal payments");
            this.externalPaymentTableConfig = this.createPaymentTableConfig("External payments");
            this.contractTableConfig = this.createStandardTableConfig("Contract registrations");
            this.relevantSystemTableConfig = this.createStandardTableConfig("Relevant systems");
            this.responsibleSystemTableConfig = this.createStandardTableConfig("Responsible systems");
        }

        private setupOptions() {
            this.roles = this.createSingleOptions(Models.Api.Organization.OrganizationRegistrationOption.Roles);
            this.internalPayments = this.createSingleOptions(Models.Api.Organization.OrganizationRegistrationOption.InternalPayments, true, true);
            this.externalPayments = this.createSingleOptions(Models.Api.Organization.OrganizationRegistrationOption.ExternalPayments, true, true);
            this.contractRegistrations = this.createSingleOptions(Models.Api.Organization.OrganizationRegistrationOption.ContractRegistrations);
            this.relevantSystemRegistrations = this.createSingleOptions(Models.Api.Organization.OrganizationRegistrationOption.RelevantSystems, true);
            this.responsibleSystemRegistrations = this.createSingleOptions(Models.Api.Organization.OrganizationRegistrationOption.ResponsibleSystems);
        }

        private createSingleOptions(type: Models.Api.Organization.OrganizationRegistrationOption, hasExtraDataColumn: boolean = false, hasExtraIndexColumn: boolean = false): IOrganizationUnitMigrationOptions {
            return {
                root: {
                    selected: false,
                    type: type,
                    children: []
                },
                hasExtraDataColumn: hasExtraDataColumn,
                hasExtraIndexColumn: hasExtraIndexColumn,
                selectedRegistrationChanged: () => this.updateAnySelections(),
                selectedRegistrationGroupChanged: (root: IOrganizationUnitMigrationRoot) => this.changeRegistrationGroupStatus(root),
                refreshData: () => this.getData()
            } as IOrganizationUnitMigrationOptions;
        }

        getData() {
            this.organizationRegistrationsService.getRegistrations(this.unitId).then(response => {
                this.roles.root.children = this.mapOrganizationRegistrationsToOptions(Models.Api.Organization.OrganizationRegistrationOption.Roles, response);
                this.internalPayments.root.children = this.mapOrganizationRegistrationsPaymentsToOptions(Models.Api.Organization.OrganizationRegistrationOption.InternalPayments, response);
                this.externalPayments.root.children = this.mapOrganizationRegistrationsPaymentsToOptions(Models.Api.Organization.OrganizationRegistrationOption.ExternalPayments, response);
                this.contractRegistrations.root.children = this.mapOrganizationRegistrationsToOptions(Models.Api.Organization.OrganizationRegistrationOption.ContractRegistrations, response);
                this.relevantSystemRegistrations.root.children = this.mapOrganizationRegistrationsToOptions(Models.Api.Organization.OrganizationRegistrationOption.RelevantSystems, response);
                this.responsibleSystemRegistrations.root.children = this.mapOrganizationRegistrationsToOptions(Models.Api.Organization.OrganizationRegistrationOption.ResponsibleSystems, response);
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

        private createChangeRequest(): Array<Models.Api.Organization.OrganizationRegistrationChangeRequest> {
            return this.roles.root.children.filter(x => x.selected)
                .map(item => {
                    return{
                        id: item.id,
                        type: item.type
                    } as Models.Api.Organization.OrganizationRegistrationChangeRequest;
                });
        }

        private mapOrganizationRegistrationsToOptions(type: Models.Api.Organization.OrganizationRegistrationOption, registrations: Models.Api.Organization.OrganizationRegistrationDetailsDto[]): IOrganizationUnitRegistration[] {
            return registrations.filter(x => x.type === type).map(res => {
                return {
                    id: res.id,
                    text: res.text,
                } as IOrganizationUnitRegistration;
            });
        }

        private mapOrganizationRegistrationsPaymentsToOptions(type: Models.Api.Organization.OrganizationRegistrationOption, registrations: Models.Api.Organization.OrganizationRegistrationDetailsDto[]): IOrganizationUnitRegistration[] {
            return registrations.filter(x => x.type === type).map(res => {
                return {
                    id: res.id,
                    text: res.text,
                    objectId: res.objectId,
                    objectName: res.objectName,
                    index: res.paymentIndex
                } as IOrganizationUnitRegistration;
            });
        }
    }

    angular.module("app")
        .component("orgUnitMigration", setupComponent());
}
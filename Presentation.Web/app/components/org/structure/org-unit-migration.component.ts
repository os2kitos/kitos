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

    export interface IOrganizationUnitMigrationOptions {
        root: IOrganizationUnitMigrationRoot;
        selectedRegistrationChanged?: () => void;
        selectedRegistrationGroupChanged?: (root: IOrganizationUnitMigrationRoot) => void;
        refreshData: () => void;
        type: Models.Organization.OrganizationRegistrationOption;
    }

    export interface IOrganizationUnitMigrationRoot extends Models.ViewModel.Organization.IHasSelection {
        children: Models.Organization.IOrganizationUnitRegistration[];
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
                .then(() => this.selectedOrg = null)
                .then(() => this.getData())
                .then(() => this.updateAnySelections());
        }

        setSelectedOrg() {
            if (!this.selectedOrg?.id)
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

        private collectSelectedRegistrations(): Array<Models.Organization.IOrganizationUnitRegistration> {

            var result = [] as Models.Organization.IOrganizationUnitRegistration[];

            const roots = this.getAllRoots();
            roots.forEach(root => {
                const res = this.collectSelectedRegistrationsFromSource(root.children);
                result = result.concat(res);
            });

            return result;
        }

        private collectSelectedRegistrationsFromSource(sourceCollection: Array<Models.Organization.IOrganizationUnitRegistration>): Array<Models.Organization.IOrganizationUnitRegistration> {
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
            this.relevantSystemTableConfig = this.createSystemTableConfig("Relevant systems");
            this.responsibleSystemTableConfig = this.createSystemTableConfig("Responsible systems");
        }

        private setupOptions() {
            this.roles = this.createBaseOptions(Models.Organization.OrganizationRegistrationOption.Roles);
            this.internalPayments = this.createBaseOptions(Models.Organization.OrganizationRegistrationOption.InternalPayments);
            this.externalPayments = this.createBaseOptions(Models.Organization.OrganizationRegistrationOption.ExternalPayments);
            this.contractRegistrations = this.createBaseOptions(Models.Organization.OrganizationRegistrationOption.ContractRegistrations);
            this.relevantSystemRegistrations = this.createBaseOptions(Models.Organization.OrganizationRegistrationOption.RelevantSystems);
            this.responsibleSystemRegistrations = this.createBaseOptions(Models.Organization.OrganizationRegistrationOption.ResponsibleSystems);
        }

        private createChangeRequest(): Models.Api.Organization.OrganizationRegistrationChangeRequest {
            return Helpers.OrganizationRegistrationHelper.createChangeRequest(
                this.contractRegistrations.root.children,
                this.externalPayments.root.children,
                this.internalPayments.root.children,
                this.roles.root.children,
                this.relevantSystemRegistrations.root.children,
                this.responsibleSystemRegistrations.root.children);
        }

        private createBaseOptions(type: Models.Organization.OrganizationRegistrationOption): IOrganizationUnitMigrationOptions {
            return {
                root: {
                    selected: false,
                    children: []
                },
                type: type,
                selectedRegistrationChanged: () => this.updateAnySelections(),
                selectedRegistrationGroupChanged: (root: IOrganizationUnitMigrationRoot) => this.changeRegistrationGroupStatus(root),
                refreshData: () => this.getData()
            } as IOrganizationUnitMigrationOptions;
        }

        private getData() {
            this.organizationRegistrationsService.getRegistrations(this.unitId).then(response => {
                this.roles.root.children = this.mapOrganizationDtoToOptions(response.organizationUnitRights);
                this.getPaymentOptions(response.payments);
                this.contractRegistrations.root.children = this.mapOrganizationDtoToOptions(response.itContractRegistrations);
                this.relevantSystemRegistrations.root.children = this.mapOrganizationDtoWithEnabledToOptions(response.relevantSystems);
                this.responsibleSystemRegistrations.root.children = this.mapOrganizationDtoWithEnabledToOptions(response.responsibleSystems);
            }, error => {
                console.error(error);
            });
        }

        private createStandardTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: title, property: "text" },
            ] as IMigrationTableColumn[];
        }

        private createSystemTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: title, property: "text" },
                { title: "Status", property: "objectText" },
            ] as IMigrationTableColumn[];
        }

        private createPaymentTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: "Index", property: "index" },
                { title: title, property: "text" },
                { title: "Contract name", property: "objectText" }
            ] as IMigrationTableColumn[];
        }

        private mapOrganizationDtoToOptions(registrations: Models.Generic.NamedEntity.NamedEntityDTO[]): Models.Organization.IOrganizationUnitRegistration[] {
            return registrations.map(res => {
                return {
                    id: res.id,
                    text: res.name,
                } as Models.Organization.IOrganizationUnitRegistration;
            });
        }

        private mapOrganizationDtoWithEnabledToOptions(registrations: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[]): Models.Organization.IOrganizationUnitRegistration[] {
            return registrations.map(res => {
                return {
                    id: res.id,
                    text: res.name,
                    objectText: res.disabled ? "Ikke aktivt" : "Aktivt" 
            } as Models.Organization.IOrganizationUnitRegistration;
            });
        }

        private getPaymentOptions(payments: Models.Api.Organization.PaymentRegistrationDetailsDto[]) {
            this.internalPayments.root.children = [];
            this.externalPayments.root.children = [];
            payments.forEach(payment => {
                this.internalPayments.root.children = this.internalPayments.root.children.concat(this.mapPaymentsToOptions(payment.itContract, payment.internalPayments));
                this.externalPayments.root.children = this.externalPayments.root.children.concat(this.mapPaymentsToOptions(payment.itContract, payment.externalPayments));
            });
        }

        private mapPaymentsToOptions(contract: Models.Generic.NamedEntity.NamedEntityDTO, payments: Models.Generic.NamedEntity.NamedEntityDTO[]): Models.Organization.IOrganizationUnitRegistration[] {
            return payments.map((element, index) => {
                return {
                    id: element.id,
                    text: element.name,
                    objectText: contract.name,
                    index: index + 1,
                    optionalObjectContext: contract
                } as Models.Organization.IOrganizationUnitRegistration;
            });
        }
    }

    angular.module("app")
        .component("orgUnitMigration", setupComponent());
}
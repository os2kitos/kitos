module Kitos.Organization.Structure.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return{
            bindings: {
                organizationId: "<",
                unitId: "<",
                unitName: "@",
                closeModal: "&",
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
        checkIsBusy: () => boolean;
        setIsBusy: (value: boolean) => void;
        type: Models.ViewModel.Organization.OrganizationRegistrationOption;
        dataRelatedPage: string;
    }

    export interface IOrganizationUnitMigrationRoot extends Models.ViewModel.Organization.IHasSelection {
        children: Models.ViewModel.Organization.IOrganizationUnitRegistration[];
    }

    interface IOrganizationUnitMigrationController extends ng.IComponentController {
        organizationId: number;
        unitId: number;
        unitName: string;
        closeModal: () => void;
    }

    class OrganizationUnitMigrationController implements IOrganizationUnitMigrationController {
        organizationId: number | null = null;
        unitId: number | null = null;
        unitName: string | null = null;
        closeModal: () => void;
        anySelections = false;
        allSelections = false;
        targetUnitSelected = false;
        shouldTransferBtnBeEnabled = false;
        isAnyDataPresent = false;

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

        private isBusy = false;

        static $inject: string[] = ["organizationRegistrationsService", "organizationApiService", "notify"];
        constructor(private readonly organizationRegistrationsService: Services.Organization.IOrganizationRegistrationsService,
            private readonly organizationApiService: Services.IOrganizationApiService,
            private readonly notify) {
        }

        $onInit() {
            if (this.organizationId === null) {
                console.error("missing attribute: 'organizationId'");
            }
            if (this.unitId === null) {
                console.error("missing attribute: 'unitId'");
            }
            if (this.unitName === null) {
                console.error("missing attribute: 'unitName'");
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
            if (this.isBusy) {
                return;
            }
            if (!confirm('Er du sikker på, at du vil slette de valgte registreringer?')) {
                return;
            }
            this.isBusy = true;

            const request = this.createChangeRequest();
            this.organizationRegistrationsService.deleteSelectedRegistrations(this.organizationId, this.unitId, request)
                .then(() => {
                    this.refreshData(); 
                },
                error => {
                    console.error(error);
                    this.notify.addErrorMessage("Delete selected failed");
                });

            this.isBusy = false;
        }

        transfer() {
            if (this.isBusy) {
                return;
            }
            if (!this.selectedOrg?.id)
                return;
            if (!confirm(`Er du sikker på, at du vil overføre de valgte registreringer til "${this.selectedOrg.text}"?`)) {
                return;
            }
            this.isBusy = true;

            const request = this.createChangeRequest();
            this.organizationRegistrationsService.transferSelectedRegistrations(this.organizationId, this.unitId, this.selectedOrg.id, request)
                .then(() => {
                    this.selectedOrg = null;
                    this.refreshData();
                }, error => {
                    console.log(error);
                    this.notify.addErrorMessage("Transfer failed");
                });

            this.isBusy = false;
        }

        setSelectedOrg() {
            if (!this.selectedOrg?.id)
                return;
            if (parseInt(this.selectedOrg.id) === this.unitId) {
                this.selectedOrg = null;
                this.notify.addErrorMessage("You cannot choose the current unit");
                return;
            }
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

        private collectSelectedRegistrations(): Array<Models.ViewModel.Organization.IOrganizationUnitRegistration> {

            var result = [] as Models.ViewModel.Organization.IOrganizationUnitRegistration[];

            const roots = this.getAllRoots();
            roots.forEach(root => {
                const res = this.collectSelectedRegistrationsFromSource(root.children);
                result = result.concat(res);
            });

            return result;
        }

        private collectSelectedRegistrationsFromSource(sourceCollection: Array<Models.ViewModel.Organization.IOrganizationUnitRegistration>): Array<Models.ViewModel.Organization.IOrganizationUnitRegistration> {
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
            this.rolesTableConfig = this.createUserFullNameTableConfig("Rolle");
            this.internalPaymentTableConfig = this.createPaymentTableConfig("Betaling (Anskaffelse,Drift,Andet)");
            this.externalPaymentTableConfig = this.createPaymentTableConfig("Betaling (Anskaffelse,Drift,Andet)");
            this.contractTableConfig = this.createStandardTableConfig("Kontraktnavn");
            this.relevantSystemTableConfig = this.createStandardTableConfig("Systemnavn");
            this.responsibleSystemTableConfig = this.createStandardTableConfig("Systemnavn");
        }

        private setupOptions() {
            this.roles = this.createBaseOptions(Models.ViewModel.Organization.OrganizationRegistrationOption.Roles);
            this.internalPayments = this.createBaseOptions(Models.ViewModel.Organization.OrganizationRegistrationOption.InternalPayments, "it-contract.edit.economy");
            this.externalPayments = this.createBaseOptions(Models.ViewModel.Organization.OrganizationRegistrationOption.ExternalPayments, "it-contract.edit.economy");
            this.contractRegistrations = this.createBaseOptions(Models.ViewModel.Organization.OrganizationRegistrationOption.ContractRegistrations, "it-contract.edit.main");
            this.relevantSystemRegistrations = this.createBaseOptions(Models.ViewModel.Organization.OrganizationRegistrationOption.RelevantSystems, "it-system.usage.org");
            this.responsibleSystemRegistrations = this.createBaseOptions(Models.ViewModel.Organization.OrganizationRegistrationOption.ResponsibleSystems, "it-system.usage.org");
        }

        private createChangeRequest(): Models.Api.Organization.OrganizationRegistrationChangeRequestDto {
            return Helpers.OrganizationRegistrationHelper.createChangeRequest(
                this.contractRegistrations.root.children,
                this.externalPayments.root.children,
                this.internalPayments.root.children,
                this.roles.root.children,
                this.relevantSystemRegistrations.root.children,
                this.responsibleSystemRegistrations.root.children);
        }

        private getData(): ng.IPromise<void> {
            return this.organizationRegistrationsService.getRegistrations(this.organizationId, this.unitId).then(response => {
                this.roles.root.children = this.mapDtoWithUserFullNameToOptions(response.organizationUnitRights);
                this.getPaymentOptions(response.payments);
                this.contractRegistrations.root.children = this.mapOrganizationDtoToOptions(response.itContractRegistrations);
                this.relevantSystemRegistrations.root.children = this.mapOrganizationDtoWithEnabledToOptions(response.relevantSystems);
                this.responsibleSystemRegistrations.root.children = this.mapOrganizationDtoWithEnabledToOptions(response.responsibleSystems);
                this.checkIsAnyDataPresent();
            }, error => {
                console.error(error);
            });
        }

        private refreshData() {
            this.getData()
                .then(() => this.updateAnySelections());
        }

        private checkIsAnyDataPresent(): void {
            if (this.roles.root.children.length === 0 &&
                this.externalPayments.root.children.length === 0 &&
                this.internalPayments.root.children.length === 0 &&
                this.contractRegistrations.root.children.length === 0 &&
                this.relevantSystemRegistrations.root.children.length === 0 &&
                this.responsibleSystemRegistrations.root.children.length === 0) {

                this.isAnyDataPresent = false;
                return;
            }

            this.isAnyDataPresent = true;
        }

        private checkIsBusy(): boolean {
            return this.isBusy;
        }

        private setIsBusy(value: boolean): void{
            this.isBusy = value;
        }

        private createBaseOptions(type: Models.ViewModel.Organization.OrganizationRegistrationOption, dataRelatedPage?: string): IOrganizationUnitMigrationOptions {
            return {
                root: {
                    selected: false,
                    children: []
                },
                type: type,
                dataRelatedPage: dataRelatedPage,
                selectedRegistrationChanged: () => this.updateAnySelections(),
                selectedRegistrationGroupChanged: (root: IOrganizationUnitMigrationRoot) => this.changeRegistrationGroupStatus(root),
                refreshData: () => this.refreshData(),
                checkIsBusy: () => this.checkIsBusy(),
                setIsBusy: (value: boolean) => this.setIsBusy(value)
            } as IOrganizationUnitMigrationOptions;
        }

        private createStandardTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: title, property: "text", isLink: true },
            ] as IMigrationTableColumn[];
        }

        private createUserFullNameTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: title, property: "text" },
                { title: "Bruger", property: "objectText" }
            ] as IMigrationTableColumn[];
        }

        private createPaymentTableConfig(title: string): IMigrationTableColumn[] {
            return [
                { title: "Index", property: "index" },
                { title: "Kontraktnavn", property: "objectText", isLink: true },
                { title: title, property: "text" }
            ] as IMigrationTableColumn[];
        }

        private mapOrganizationDtoToOptions(registrations: Models.Generic.NamedEntity.NamedEntityDTO[]): Models.ViewModel.Organization.IOrganizationUnitRegistration[] {
            return registrations.map(res => {
                return {
                    id: res.id,
                    text: res.name,
                    targetPageObjectId: res.id,
                } as Models.ViewModel.Organization.IOrganizationUnitRegistration;
            });
        }

        private mapDtoWithUserFullNameToOptions(registrations: Models.Generic.NamedEntity.NamedEntityWithUserFullNameDTO[]): Models.ViewModel.Organization.IOrganizationUnitRegistration[] {
            return registrations.map(res => {
                return {
                    id: res.id,
                    text: res.name,
                    objectText: res.userFullName
                } as Models.ViewModel.Organization.IOrganizationUnitRegistration;
            });
        }

        private mapOrganizationDtoWithEnabledToOptions(registrations: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[]): Models.ViewModel.Organization.IOrganizationUnitRegistration[] {
            return registrations.map(res => {
                return {
                    id: res.id,
                    text: Helpers.SystemNameFormat.apply(res.name, res.disabled),
                    targetPageObjectId: res.id,
                } as Models.ViewModel.Organization.IOrganizationUnitRegistration;
            });
        }

        private getPaymentOptions(payments: Models.Api.Organization.PaymentRegistrationDetailsDto[]) {
            var internalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[] = [];
            var externalPayments: Models.ViewModel.Organization.IOrganizationUnitRegistration[] = [];

            payments.forEach(payment => {
                internalPayments = internalPayments.concat(this.mapPaymentsToOptions(payment.itContract, payment.internalPayments));
                externalPayments = externalPayments.concat(this.mapPaymentsToOptions(payment.itContract, payment.externalPayments));
            });

            this.internalPayments.root.children = internalPayments;
            this.externalPayments.root.children = externalPayments;
        }

        private mapPaymentsToOptions(contract: Models.Generic.NamedEntity.NamedEntityDTO, payments: Models.Generic.NamedEntity.NamedEntityDTO[]): Models.ViewModel.Organization.IOrganizationUnitRegistration[] {
            return payments.map((element, index) => {
                return {
                    id: element.id,
                    text: element.name,
                    targetPageObjectId: contract.id,
                    objectText: contract.name,
                    index: index + 1,
                    optionalObjectContext: contract
                } as Models.ViewModel.Organization.IOrganizationUnitRegistration;
            });
        }
    }

    angular.module("app")
        .component("orgUnitMigration", setupComponent());
}
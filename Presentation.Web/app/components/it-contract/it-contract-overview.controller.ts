module Kitos.ItContract.Overview {
    export interface IOverviewController extends Utility.KendoGrid.IGridViewAccess<Models.ViewModel.ItContract.IItContractOverviewViewModel> {
    }

    export class OverviewController implements IOverviewController {
        mainGrid: IKendoGrid<Models.ViewModel.ItContract.IItContractOverviewViewModel>;
        mainGridOptions: IKendoGridOptions<Models.ViewModel.ItContract.IItContractOverviewViewModel>;
        canCreate: boolean;

        private readonly criticalityPropertyName = "Criticality";
        private readonly contractTypePropertyName = "ContractType";
        private readonly contractTemplatePropertyName = "ContractTemplate";
        private readonly purchaseFormPropertyName = "PurchaseForm";
        private readonly procurementStrategyPropertyName = "ProcurementStrategy";
        private readonly paymentModelPropertyName = "PaymentModel";
        private readonly paymentFrequencyPropertyName = "PaymentFreqency";
        private readonly optionExtendPropertyName = "OptionExtend";
        private readonly terminationDeadlinePropertyName = "TerminationDeadline";
        private readonly procurementPlanYearPropertyName = "ProcurementPlanYear";
        private readonly orgUnitStorageKey = "it-contract-overview-orgunit";

        private readonly criticalityOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly contractTypeOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly contractTemplateOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly purchaseFormOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly procurementStrategyOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly paymentModelOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly paymentFrequencyOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly optionExtendOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly terminationDeadlineOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly yesNoUndecided: Models.ViewModel.Shared.YesNoUndecidedOptions;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$window",
            "$state",
            "_",
            "user",
            "itContractRoles",
            "orgUnits",
            "needsWidthFixService",
            "userAccessRights",
            "uiState",
            "itContractOptions",
            "kendoGridLauncherFactory",
            "procurements"
        ];

        constructor(
            $rootScope: IRootScope,
            $scope: ng.IScope,
            $window: ng.IWindowService,
            $state: ng.ui.IStateService,
            _: ILoDashWithMixins,
            user,
            itContractRoles,
            orgUnits,
            needsWidthFixService,
            userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO,
            uiState: Models.UICustomization.ICustomizedModuleUI,
            itContractOptions: Models.ItContract.IItContractOptions,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            procurements: Models.ItContract.IContractProcurementPlanDTO[]) {
            $rootScope.page.title = "IT Kontrakt";

            $scope.procurements = procurements.map(value => {
                return {
                    textValue: `${value.procurementPlanYear} | Q${value.procurementPlanQuarter}`,
                    remoteValue: `${value.procurementPlanYear} | Q${value.procurementPlanQuarter}`
                }
            });
            $scope.procurements.push({
                textValue: " ",
                remoteValue: -1
            });

            const uiBluePrint = Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            const getRoleKey = (roleId: number | string) => `role${roleId}`;

            this.criticalityOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.criticalityOptions);
            this.contractTypeOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.contractTypeOptions);
            this.contractTemplateOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.contractTemplateOptions);
            this.purchaseFormOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.purchaseFormOptions);
            this.procurementStrategyOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.procurementStrategyOptions);
            this.paymentModelOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.paymentModelOptions);
            this.paymentFrequencyOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.paymentFrequencyOptions);
            this.optionExtendOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.optionExtendOptions);
            this.terminationDeadlineOptionViewModel =
                new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.terminationDeadlineOptions);

            this.yesNoUndecided = new Models.ViewModel.Shared.YesNoUndecidedOptions();
            
            // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(concat(concat(c/User/Name, ' '), c/User/LastName),'foo') and c/RoleId eq {roleId})"
            const replaceRoleFilter = (filterUrl: string, roleName: string, roleId: number) => {
                const pattern = new RegExp(`(\\w+\\()${roleName}(.*?\\))`, "i");
                return filterUrl.replace(pattern,
                    `Rights/any(c: $1concat(concat(c/User/Name, ' '), c/User/LastName)$2 and c/RoleId eq ${roleId})`);
            }

            const replaceSystemFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
                return filterUrl.replace(pattern, "AssociatedSystemUsages/any(c: $1c/ItSystemUsage/ItSystem/Name$2)");
            }

            const replaceSystemUuidFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
                return filterUrl.replace(pattern, "AssociatedSystemUsages/any(c: contains(cast(c/ItSystemUsage/Uuid, Edm.String)$2)");
            }

            const replaceOptionTypeFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`(${column}( eq )\'([0-9]+)\')`, "i");
                const matchingFilterParts = pattern.exec(filterUrl);
                if (matchingFilterParts?.length !== 4)
                    return filterUrl;

                var searchedValue = matchingFilterParts[3];
                if (searchedValue.indexOf("0") !== -1) {
                    searchedValue = searchedValue.replace("0", "null");
                }
                return filterUrl.replace(pattern, `${column}/Id$2${searchedValue}`);
            }

            const replaceDprFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`(${column}( eq )\'([0-9]+)\')`, "i");
                const matchingFilterParts = pattern.exec(filterUrl);
                if (matchingFilterParts?.length !== 4)
                    return filterUrl;

                var searchedValue = matchingFilterParts[3];
                const yesValue = `${Models.Api.Shared.YesNoIrrelevantOption.YES.valueOf()}`;
                if (searchedValue.indexOf(yesValue) !== -1) {
                    return filterUrl.replace(pattern, `${column}/Any (c:c/IsAgreementConcluded eq '${yesValue}')`);
                }
                
                return filterUrl.replace(pattern, `${column}/All (c:c/IsAgreementConcluded ne '${yesValue}')`);
            }

            const replaceProcurementFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`${column} eq \'([a-zA-Z0-9 |-]+)\'`, "i");
                const matchingFilterPart = pattern.exec(filterUrl);
                if (matchingFilterPart?.length !== 2) {
                    return filterUrl;
                }
                const userFilterQueryElements = matchingFilterPart[1].replace(",'", "").replace(",", "").replace(/\)$/, "").replace(/'$/, "").replace(" |", "").replace("Q", "").split(" ");

                var result = "(";
                userFilterQueryElements.forEach((filterValue, i) => {
                    var value = filterValue;
                    if (value === "-1")
                        value = "null";

                    result += `(ProcurementPlanYear eq ${value} or ProcurementPlanQuarter eq ${value})`;
                    if (i < userFilterQueryElements.length - 1) {
                        result += " and ";
                    } else {
                        result += ")";
                    }
                });

                filterUrl = filterUrl.replace(pattern, result);
                return filterUrl;
            }

            var launcher =
                kendoGridLauncherFactory
                    .create<Models.ViewModel.ItContract.IItContractOverviewViewModel>()
                    .withScope($scope)
                    .withGridBinding(this)
                    .withUser(user)
                    .withEntityTypeName("IT Kontrakt")
                    .withExcelOutputName("IT Kontrakt")
                    .withStorageKey("it-contract-overview-options")
                    .withUrlFactory(() => {
                        var urlParameters =
                            "?$expand=" +
                                "Reference($select=URL,Title,ExternalReferenceId)," +
                                "Parent($select=Id,Name)," +
                                "ResponsibleOrganizationUnit($select=Name)," +
                                "PaymentModel($select=Name)," +
                                "PaymentFreqency($select=Name)," +
                                "Rights($select=Id,RoleId,UserId;$expand=User($select=Id,Name,LastName),Role($select=Name,Id))," +
                                "Supplier($select=Name)," +
                                "AssociatedSystemUsages($expand=ItSystemUsage($select=Id;$expand=ItSystem($select=Name,Disabled)))," +
                                "DataProcessingRegistrations($select=IsAgreementConcluded,Name,Id)," +
                                "LastChangedByUser($select=Name,LastName)," +
                                "ExternEconomyStreams($select=Acquisition,Operation,Other,AuditStatus,AuditDate)," +
                                `${this.criticalityPropertyName}($select=Id),` +
                                `${this.contractTypePropertyName}($select=Id),` +
                                `${this.contractTemplatePropertyName}($select=Id),` +
                                `${this.purchaseFormPropertyName}($select=Id),` +
                                `${this.procurementStrategyPropertyName}($select=Id),` +
                                `${this.paymentModelPropertyName}($select=Id),` +
                                `${this.paymentFrequencyPropertyName}($select=Id),` +
                                `${this.optionExtendPropertyName}($select=Id),` +
                                `${this.terminationDeadlinePropertyName}($select=Id),` +
                                "AssociatedSystemUsages($select=ItSystemUsage;$expand=ItSystemUsage($select=Uuid))," +
                                "AssociatedSystemRelations($select=Id)";

                        var orgUnitId = $window.sessionStorage.getItem(this.orgUnitStorageKey);
                        var query = `/odata/Organizations(${user.currentOrganizationId})/`;
                        // if orgunit is set then the org unit filter is active
                        if (orgUnitId === null) {
                            return query + `ItContracts` + urlParameters;
                        } else {
                            return query + `OrganizationUnits(${orgUnitId})/ItContracts` + urlParameters;
                        }
                    })
                    .withStandardSorting("Name")
                    .withParameterMapping((options, type) => {
                        var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                        if (parameterMap.$orderby) {
                            
                            //Option types orderBy fixes
                            if (parameterMap.$orderby.includes(this.criticalityPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.criticalityPropertyName,
                                    `${this.criticalityPropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.contractTypePropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.contractTypePropertyName,
                                    `${this.contractTypePropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.contractTemplatePropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.contractTemplatePropertyName,
                                    `${this.contractTemplatePropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.purchaseFormPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.purchaseFormPropertyName,
                                    `${this.purchaseFormPropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.procurementStrategyPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(
                                    this.procurementStrategyPropertyName,
                                    `${this.procurementStrategyPropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.paymentModelPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.paymentModelPropertyName,
                                    `${this.paymentModelPropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.paymentFrequencyPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.paymentFrequencyPropertyName,
                                    `${this.paymentFrequencyPropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.optionExtendPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.optionExtendPropertyName,
                                    `${this.optionExtendPropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.terminationDeadlinePropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(
                                    this.terminationDeadlinePropertyName,
                                    `${this.terminationDeadlinePropertyName}/Name`);
                            }
                        }

                        if (parameterMap.$filter) {
                            _.forEach(itContractRoles,
                                role => parameterMap.$filter =
                                replaceRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id));

                            parameterMap.$filter =
                                replaceSystemFilter(parameterMap.$filter, "AssociatedSystemUsages");
                            parameterMap.$filter =
                                replaceSystemUuidFilter(parameterMap.$filter, "ExhibitedBy.ItSystem.Uuid");

                            const lastChangedByUserSearchedProperties = ["Name", "LastName"];
                            parameterMap.$filter = Helpers.OdataQueryHelper.replaceQueryByMultiplePropertyContains(parameterMap.$filter,
                                "LastChangedByUser/Name",
                                "LastChangedByUser",
                                lastChangedByUserSearchedProperties);
                            
                            parameterMap.$filter = replaceProcurementFilter(parameterMap.$filter,
                                "ProcurementPlanYear");

                            //Option types filter fixes
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.criticalityPropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.contractTypePropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.contractTemplatePropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.purchaseFormPropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.procurementStrategyPropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.paymentModelPropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.paymentFrequencyPropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.optionExtendPropertyName);
                            parameterMap.$filter =
                                replaceOptionTypeFilter(parameterMap.$filter, this.terminationDeadlinePropertyName);

                            parameterMap.$filter =
                                replaceDprFilter(parameterMap.$filter, "DataProcessingRegistrations");
                        }

                        return parameterMap;
                    })
                    .withResponseParser(response => {
                        $scope.procurements = [];

                        response.forEach(contract => {
                            if (contract.ProcurementPlanQuarter && contract.ProcurementPlanYear) {
                                const procurement =
                                    `${contract.ProcurementPlanYear} | Q${contract.ProcurementPlanQuarter}`;
                                if ($scope.procurements.filter(value => value === procurement).length === 0) {
                                    $scope.procurements.push(procurement);
                                }
                            }

                            var ecoData = contract.ExternEconomyStreams ?? [];

                            contract.Acquisition = _.sumBy(ecoData, "Acquisition");
                            contract.Operation = _.sumBy(ecoData, "Operation");
                            contract.Other = _.sumBy(ecoData, "Other");

                            const streamsSortedByAuditDate = _.sortBy(ecoData, ["AuditDate"]);
                            var streamWithEarliestAuditDate = _.last(streamsSortedByAuditDate);
                            if (streamWithEarliestAuditDate && streamWithEarliestAuditDate.AuditDate) {
                                contract.AuditDate = streamWithEarliestAuditDate.AuditDate;
                            }

                            var totalWhiteStatuses = _.filter(ecoData, { AuditStatus: "White" }).length;
                            var totalRedStatuses = _.filter(ecoData, { AuditStatus: "Red" }).length;
                            var totalYellowStatuses = _.filter(ecoData, { AuditStatus: "Yellow" }).length;
                            var totalGreenStatuses = _.filter(ecoData, { AuditStatus: "Green" }).length;

                            contract.status = {
                                max: totalWhiteStatuses +
                                    totalRedStatuses +
                                    totalYellowStatuses +
                                    totalGreenStatuses,
                                white: totalWhiteStatuses,
                                red: totalRedStatuses,
                                yellow: totalYellowStatuses,
                                green: totalGreenStatuses
                            };

                            contract.roles = [];
                            // iterrate each right
                            _.forEach(contract.Rights,
                                right => {
                                    // init an role array to hold users assigned to this role
                                    if (!contract.roles[right.RoleId])
                                        contract.roles[right.RoleId] = [];

                                    // push username to the role array
                                    contract.roles[right.RoleId]
                                        .push([right.User.Name, right.User.LastName].join(" "));
                                });


                            if (!contract.Parent) { contract.Parent = { Name: "" } as any; }
                            if (!contract.ResponsibleOrganizationUnit) { contract.ResponsibleOrganizationUnit = { Name: "" } as any; }
                            if (!contract.Supplier) { contract.Supplier = { Name: "" } as any; }
                            if (!contract.Reference) { contract.Reference = { Title: "", ExternalReferenceId: "" } as any; }
                            if (!contract.PaymentModel) { contract.PaymentModel = { Name: "" } as any; }
                            if (!contract.Reference) { contract.Reference = { Title: "", ExternalReferenceId: "" } as any; }
                            if (!contract.LastChangedByUser) { contract.LastChangedByUser = { Name: "", LastName: "" } as any; }
                        });

                        return response;
                    })
                    .withToolbarEntry({
                        id: "createContract",
                        title: "Opret IT Kontrakt",
                        color: Utility.KendoGrid.KendoToolbarButtonColor.Green,
                        position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                        margins: [Utility.KendoGrid.KendoToolbarMargin.Left],
                        implementation: Utility.KendoGrid.KendoToolbarImplementation.Button,
                        enabled: () => userAccessRights.canCreate,
                        onClick: () => $state.go("it-contract.overview.create")
                    } as Utility.KendoGrid.IKendoToolbarEntry);

            if (uiState.isBluePrintNodeAvailable(uiBluePrint.children.contractRoles)) {
                launcher = launcher.withToolbarEntry({
                    id: "roleSelector",
                    title: "Vælg kontraktrolle...",
                    color: Utility.KendoGrid.KendoToolbarButtonColor.Grey,
                    position: Utility.KendoGrid.KendoToolbarButtonPosition.Left,
                    margins: [Utility.KendoGrid.KendoToolbarMargin.Left],
                    implementation: Utility.KendoGrid.KendoToolbarImplementation.DropDownList,
                    enabled: () => true,
                    dropDownConfiguration: {
                        selectedOptionChanged: newItem => {
                            // hide all roles column
                            itContractRoles.forEach(role => {
                                this.mainGrid.hideColumn(getRoleKey(role.Id));
                            });

                            //Only show the selected role
                            var gridFieldName = getRoleKey(newItem.id);
                            this.mainGrid.showColumn(gridFieldName);
                            needsWidthFixService.fixWidth();
                        },
                        availableOptions: itContractRoles.map(role => {
                            return {
                                id: `${role.Id}`,
                                text: role.Name,
                                originalObject: role
                            };
                        })
                    }
                } as Utility.KendoGrid.IKendoToolbarEntry);
            }

            launcher = launcher
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("IsActive")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Boolean)
                        .withTitle("Gyldig/Ikke gyldig")
                        .withId("isActive")
                        .withFixedValueRange([
                                {
                                    textValue: "Gyldig",
                                    remoteValue: true
                                },
                                {
                                    textValue: "Ikke gyldig",
                                    remoteValue: false
                                }
                            ],
                            false)
                        .withRendering(dataItem => dataItem.IsActive
                            ? '<span class="fa fa-file text-success" aria-hidden="true"></span>'
                            : '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>')
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withExcelOutput(dataItem => dataItem.IsActive ? "Gyldig" : "Ikke gyldig")
                        .withoutSorting()
                        .withInclusionCriterion(
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.isActive)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("ItContractId")
                        .withTitle("Kontrakt ID")
                        .withId("contractId")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withInclusionCriterion(
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.contractId)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("Parent.Name")
                        .withTitle("Overordnet kontrakt")
                        .withId("parentName")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(
                            "kendo-parent-rendering",
                            "it-contract.edit.main",
                            dataItem.Parent.Id,
                            dataItem.Parent.Name))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dataItem.Parent.Name)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("Name")
                        .withTitle("It Kontrakt")
                        .withId("contractName")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(
                            "contractNameObject",
                            "it-contract.edit.main",
                            dataItem.Id,
                            dataItem.Name))
                        .withExcelOutput(dataItem => dataItem.Parent?.Name))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName(this.criticalityPropertyName)
                        .withTitle("Kritikalitet")
                        .withId("criticality")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.criticalityOptionViewModel.options,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                            this.criticalityOptionViewModel.getOptionText(dataItem.Criticality?.Id)))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                            this.criticalityOptionViewModel.getOptionText(dataItem.Criticality?.Id))))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("ResponsibleOrganizationUnit.Name")
                        .withTitle("Ansvarlig org. enhed")
                        .withId("responsibleOrganizationUnitName")
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(orgUnits.map((unit) => {
                                return {
                                    textValue: unit.Name,
                                    remoteValue: unit.Id,
                                    optionalContext: unit
                                };
                            }),
                            false,
                            dataItem => "&nbsp;&nbsp;&nbsp;&nbsp;".repeat(dataItem.optionalContext.$level) +
                            dataItem.optionalContext.Name)
                        .withRendering(
                            dataItem => Helpers.RenderFieldsHelper.renderString(dataItem
                                .ResponsibleOrganizationUnit?.Name))
                        .withExcelOutput(
                            dataItem => Helpers.ExcelExportHelper.renderString(dataItem
                                .ResponsibleOrganizationUnit?.Name)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("Supplier.Name")
                        .withTitle("Leverandør")
                        .withId("supplierName")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.Supplier?.Name))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dataItem.Supplier?.Name)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("ContractSigner")
                        .withTitle("Kontraktunderskriver")
                        .withId("contractSigner")
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withInclusionCriterion(
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.intSigner)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName(this.contractTypePropertyName)
                        .withTitle("Kontrakttype")
                        .withId("contractType")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.contractTypeOptionViewModel.options,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                            this.contractTypeOptionViewModel.getOptionText(dataItem.ContractType?.Id)))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                            this.contractTypeOptionViewModel.getOptionText(dataItem.ContractType?.Id)))
                        .withInclusionCriterion(
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.contractType)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName(this.contractTemplatePropertyName)
                        .withTitle("Kontraktskabelon")
                        .withId("contractTemplate")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.contractTemplateOptionViewModel.options,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                            this.contractTemplateOptionViewModel.getOptionText(dataItem.ContractTemplate?.Id)))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                            this.contractTemplateOptionViewModel.getOptionText(dataItem.ContractTemplate?.Id))))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName(this.purchaseFormPropertyName)
                        .withTitle("Indkøbsform")
                        .withId("purchaseForm")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.purchaseFormOptionViewModel.options,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                            this.purchaseFormOptionViewModel.getOptionText(dataItem.PurchaseForm?.Id)))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                            this.purchaseFormOptionViewModel.getOptionText(dataItem.PurchaseForm?.Id)))
                        .withInclusionCriterion(
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.purchaseForm)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName(this.procurementStrategyPropertyName)
                        .withTitle("Genanskaffelsesstrategi")
                        .withId("procurementStrategy")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.procurementStrategyOptionViewModel.options,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                            this.procurementStrategyOptionViewModel.getOptionText(dataItem
                                .ProcurementStrategy?.Id)))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                            this.procurementStrategyOptionViewModel.getOptionText(dataItem
                                .ProcurementStrategy?.Id))))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName(this.procurementPlanYearPropertyName)
                        .withTitle("Genanskaffelsesplan")
                        .withId("procurementPlanYear")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange($scope.procurements, false)
                        .withRendering(dataItem => dataItem.ProcurementPlanQuarter && dataItem.ProcurementPlanYear
                            ? `${dataItem.ProcurementPlanYear} | Q${dataItem.ProcurementPlanQuarter}`
                            : "")
                        .withExcelOutput(
                            dataItem => dataItem.ProcurementPlanQuarter && dataItem.ProcurementPlanYear
                            ? `${dataItem.ProcurementPlanYear} | Q${dataItem.ProcurementPlanQuarter}`
                                : "")
                        .withInclusionCriterion(
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.procurementPlan)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("ProcurementInitiated")
                        .withTitle("Genanskaffelse igangsat")
                        .withId("procurementInitiated")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(this.yesNoUndecided.options, false),
                            false)
                        .withRendering(dataItem => dataItem.ProcurementInitiated
                            ? Models.ViewModel.Shared.YesNoUndecidedOptions.getText(dataItem.ProcurementInitiated)
                            : "")
                        .withExcelOutput(dataItem => dataItem.ProcurementInitiated
                            ? Models.ViewModel.Shared.YesNoUndecidedOptions.getText(dataItem.ProcurementInitiated)
                            : "")
                        .withInclusionCriterion(
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.procurementStrategy)));

            itContractRoles.forEach(role => {
                const roleColumnId = `itContract${role.Id}`;
                const roleKey = getRoleKey(role.Id);
                const numberOfRolesToConcat = 5;
                launcher = launcher
                    .withColumn(builder =>
                        builder
                        .withDataSourceName(roleKey)
                        .withTitle(role.Name)
                        .withId(roleColumnId)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withoutSorting()
                        .withContentOverflow()
                        .withInitialVisibility(false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(
                            `kendo-contract-${roleKey}-rendering`,
                            "it-contract.edit.roles",
                            dataItem.Id,
                            Helpers.ArrayHelper.concatFirstNumberOfItemsAndAddElipsis(dataItem.roles[role.Id],
                                numberOfRolesToConcat)))
                        .withExcelOutput(
                            dataItem => Helpers.ArrayHelper.concatFirstNumberOfItemsAndAddElipsis(
                                dataItem.roles[role.Id],
                                numberOfRolesToConcat)));
            });

            launcher = launcher
                .withColumn(builder =>
                    builder
                    .withDataSourceName("DataProcessingRegistrations")
                    .withTitle("Databehandleraftale")
                    .withId("dataProcessingRegistrations")
                    .withoutSorting()
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                    .withFixedValueRange(
                        [
                            Models.Api.Shared.YesNoIrrelevantOption.YES,
                            Models.Api.Shared.YesNoIrrelevantOption.NO
                        ].map(value => {
                            return {
                                textValue: Models.ViewModel.Shared.YesNoIrrelevantOptions.getText(value),
                                remoteValue: value
                            };
                        }),
                        false)
                    .withContentOverflow()
                    .withRendering(dataItem => {
                        var activeDprs = [];
                        dataItem.DataProcessingRegistrations.forEach(dpr => {
                            //TODO: Comparing with an enum value always returns false
                            if (dpr.IsAgreementConcluded && dpr.IsAgreementConcluded.toString() === "YES") {
                                activeDprs.push(Helpers.RenderFieldsHelper.renderInternalReference(
                                    `kendo-contract-dpr-${dpr.Id}`,
                                    "data-processing.edit-registration.main",
                                    dpr.Id,
                                    dpr.Name));
                            }
                        });
                        return activeDprs.toString();
                    })
                    .withExcelOutput(dataItem => {
                        var activeDprs = [];
                        dataItem.DataProcessingRegistrations.forEach(dpr => {
                            //TODO: Comparing with an enum value always returns false
                            if (dpr.IsAgreementConcluded.toString() === "YES") {
                                activeDprs.push(dpr.Name);
                            }
                        });
                        return activeDprs.toString();
                    }))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("AssociatedSystemUsages")
                    .withTitle("IT Systemer")
                    .withId("associatedSystemUsages")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    .withoutSorting()
                    .withContentOverflow()
                    .withRendering(dataItem => {
                        var activeSystemUsages = [];
                        dataItem.AssociatedSystemUsages.forEach(system => {
                            activeSystemUsages.push(Helpers.RenderFieldsHelper.renderInternalReference(
                                `kendo-contract-system-usages-${system.ItSystemUsageId}`,
                                "data-processing.edit-registration.main",
                                system.ItSystemUsageId,
                                system.ItSystemUsage.ItSystem.Name));

                        });
                        return activeSystemUsages.toString();
                    })
                    .withExcelOutput(dataItem => {
                        var activeSystemUsages = [];
                        dataItem.AssociatedSystemUsages.forEach(system => {
                            activeSystemUsages.push(system.ItSystemUsage.ItSystem.Name);
                        });
                        return activeSystemUsages.toString();
                    }))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("AssociatedSystemRelations")
                    .withTitle("Antal Relationer")
                    .withId("relationCount")
                    .withoutSorting()
                    .withRendering(dataItem => {
                        if (dataItem.AssociatedSystemUsages === undefined)
                            return "";

                        return dataItem.AssociatedSystemRelations.length.toString();
                    })
                    .withExcelOutput(dataItem => {
                        if (dataItem.AssociatedSystemUsages === undefined)
                            return "";

                        return dataItem.AssociatedSystemRelations.length.toString();
                    }))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("Reference.Title")
                    .withTitle("Reference")
                    .withId("referenceTitle")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReferenceUrl(dataItem.Reference))
                    .withExcelOutput(dataItem => {
                        if (dataItem.Reference === undefined) {
                            return "";
                        }
                        return dataItem.Reference.Title;
                    }))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("Reference.ExternalReferenceId")
                    .withTitle("Dokument ID/Sagsnr.")
                    .withId("referenceExternalReferenceId")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderExternalReferenceId(dataItem.Reference))
                    .withExcelOutput(
                        dataItem => Helpers.ExcelExportHelper.renderExternalReferenceId(dataItem.Reference)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("ExternEconomyStreams.Acquisition")
                    .withTitle("Anskaffelse")
                    .withId("acquisition")
                    .withoutSorting()
                    .withRendering(dataItem => {
                        return dataItem.Acquisition ? dataItem.Acquisition.toString() : "";
                    })
                    .withExcelOutput(dataItem => {
                        return dataItem.Acquisition ? dataItem.Acquisition.toString() : "";
                    }))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("ExternEconomyStreams.Operation")
                    .withTitle("Drift/år")
                    .withId("operation")
                    .withoutSorting()
                    .withRendering(dataItem => {
                        return dataItem.Operation ? dataItem.Operation.toString() : "";
                    })
                    .withExcelOutput(dataItem => {
                        return dataItem.Operation ? dataItem.Operation.toString() : "";
                    }))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("ExternEconomyStreams.Other")
                    .withTitle("Andet")
                    .withId("other")
                    .withoutSorting()
                    .withRendering(dataItem => {
                        return dataItem.Other ? dataItem.Other.toString() : "";
                    })
                    .withExcelOutput(dataItem => {
                        return dataItem.Other ? dataItem.Other.toString() : "";
                    }))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("OperationRemunerationBegun")
                    .withTitle("Driftsvederlag begyndt")
                    .withId("operaitonRemunerationBegun")
                    .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                    .withRendering(
                        dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.OperationRemunerationBegun))
                    .withExcelOutput(
                        dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.OperationRemunerationBegun)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName(this.paymentModelPropertyName)
                    .withTitle("Betalingsmodel")
                    .withId("paymentModel")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                    .withFixedValueRange(
                        Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                            this.paymentModelOptionViewModel.options,
                            true),
                        false)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                        this.paymentModelOptionViewModel.getOptionText(dataItem.PaymentModel?.Id)))
                    .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                        this.paymentModelOptionViewModel.getOptionText(dataItem.PaymentModel?.Id)))
                    .withInclusionCriterion(
                        () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.paymentModel)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName(this.paymentFrequencyPropertyName)
                    .withTitle("Betalingsfrekvens")
                    .withId("paymentFrequency")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                    .withFixedValueRange(
                        Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                            this.paymentFrequencyOptionViewModel.options,
                            true),
                        false)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                        this.paymentFrequencyOptionViewModel.getOptionText(dataItem.PaymentFreqency?.Id)))
                    .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                        this.paymentFrequencyOptionViewModel.getOptionText(dataItem.PaymentFreqency?.Id)))
                    .withInclusionCriterion(
                        () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.paymentModel)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("ExternEconomyStreams.AuditDate")
                    .withTitle("Audit dato")
                    .withId("auditDate")
                    .withoutSorting()
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.AuditDate))
                    .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.AuditDate)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("ExternEconomyStreams.AuditStatus")
                    .withTitle("Audit status")
                    .withId("auditStatus")
                    .withoutSorting()
                    .withRendering(dataItem => {
                        if (dataItem.status.max > 0) {
                            var str = JSON.stringify(dataItem.status);
                            return `<div data-show-status='${str}'></div>`;
                        }
                        return "";
                    })
                    .withExcelOutput(dataItem => dataItem &&
                        dataItem.status &&
                        `Hvid: ${dataItem.status.white}, Rød: ${dataItem.status.red}, Gul: ${dataItem.status.yellow
                        }, Grøn: ${dataItem.status.green}, Max: ${dataItem.status.max}` ||
                        ""))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("Duration")
                    .withTitle("Verighed") //TODO: check if title is correct (possible typo): in the task documentation the title is: "Verighed", but the old field has a title "Varighed"
                    .withId("duration")
                    .withoutSorting()
                    .withRendering(dataItem => {
                        if (dataItem.DurationOngoing) {
                            return "Løbende";
                        }

                        const years = dataItem.DurationYears || 0;
                        const months = dataItem.DurationMonths || 0;

                        if (years === 0 && months === 0) {
                            return "Ikke angivet";
                        }

                        if (years > 0 && months > 0 && months < 2)
                            return `${years} år og ${months} måned`;

                        if (years > 0 && months > 0)
                            return `${years} år og ${months} måneder`;

                        if (years < 1 && months > 0 && months > 1)
                            return `${months} måneder`;

                        if (years < 1 && months < 2) {
                            return `${months} måned`;
                        }

                        if (years > 0 && months < 1)
                            return `${years} år`;

                        return "Ikke angivet";
                    })
                    .withInclusionCriterion(
                        () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.agreementDeadlines)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName(this.optionExtendPropertyName)
                    .withTitle("Option")
                    .withId("optionExtend")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                    .withFixedValueRange(
                        Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                            this.optionExtendOptionViewModel.options,
                            true),
                        false)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                        this.optionExtendOptionViewModel.getOptionText(dataItem.OptionExtend?.Id)))
                    .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                        this.optionExtendOptionViewModel.getOptionText(dataItem.OptionExtend?.Id))))
                .withColumn(builder =>
                    builder
                    .withDataSourceName(this.terminationDeadlinePropertyName)
                    .withTitle("Opsigelse (måneder)")
                    .withId("terminationDeadline")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                    .withFixedValueRange(
                        Kitos.Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                            this.terminationDeadlineOptionViewModel.options,
                            true),
                        false)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(
                        this.terminationDeadlineOptionViewModel.getOptionText(dataItem.TerminationDeadline?.Id)))
                    .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(
                        this.terminationDeadlineOptionViewModel.getOptionText(dataItem.TerminationDeadline?.Id))))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("IrrevocableTo")
                    .withTitle("Uopsigelig til")
                    .withId("irrevocableTo")
                    .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.IrrevocableTo))
                    .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.IrrevocableTo))
                    .withInclusionCriterion(
                        () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.agreementDeadlines)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("Terminated")
                    .withTitle("Opsagt")
                    .withId("terminated")
                    .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.Terminated))
                    .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.Terminated))
                    .withInclusionCriterion(
                        () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.termination)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("LastChangedByUser.Name")
                    .withTitle("Sidst redigeret: Bruger")
                    .withId("lastChangedByUser")
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    .withRendering(
                        dataItem => `${dataItem.LastChangedByUser.Name} ${dataItem.LastChangedByUser.LastName}`)
                    .withExcelOutput(
                        dataItem => `${dataItem.LastChangedByUser.Name} ${dataItem.LastChangedByUser.LastName}`))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("LastChangedBy")
                    .withTitle("Sidste redigeret: Dato")
                    .withId("lastChangedDate")
                    .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                    .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.LastChanged))
                    .withExcelOutput(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.LastChanged)))
                .withColumn(builder =>
                    builder
                    .withDataSourceName("ExhibitedBy.ItSystem.Uuid")
                    .withTitle("Udstillersystem (UUID)")
                    .withId("isSystemUuid")
                    .withoutSorting()
                    .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    .withRendering(dataItem => {
                        if (dataItem.AssociatedSystemUsages?.length > 0) {
                            var uuids = [];
                            dataItem.AssociatedSystemUsages.forEach(value => {
                                uuids.push(value.ItSystemUsage.Uuid);
                            });
                            return uuids.toString();
                        } else
                            return "";
                    }));

            launcher.launch();
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider) => {
            $stateProvider.state("it-contract.overview",
                {
                    url: "/overview",
                    templateUrl: "app/components/it-contract/it-contract-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "contractOverviewVm",
                    resolve: {
                        itContractRoles: [
                            "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                                localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItContractRoles).getAll()
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        userAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory) =>
                            authorizationServiceFactory
                                .createContractAuthorization()
                                .getOverviewAuthorization()
                        ],
                        orgUnits: [
                            "$http", "user", "_",
                            ($http, user, _) => $http
                                .get(`/odata/Organizations(${user.currentOrganizationId})/OrganizationUnits`)
                                .then(result => _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId"))
                        ],
                        uiState: [
                            "uiCustomizationStateService", (uiCustomizationStateService: Kitos.Services.UICustomization.IUICustomizationStateService) => uiCustomizationStateService.getCurrentState(Kitos.Models.UICustomization.CustomizableKitosModule.ItContract)
                        ],
                        itContractOptions: [
                            "ItContractsService", "user",
                            (ItContractsService: Kitos.Services.Contract.IItContractsService, user) =>
                                ItContractsService.getApplicableItContractOptions(user.currentOrganizationId)
                        ],
                        procurements: [
                            "ItContractsService", "user",
                            (ItContractsService: Kitos.Services.Contract.IItContractsService, user) =>
                                ItContractsService.getAvailableProcurementPlans(user.currentOrganizationId)
                        ]
                    }
                });
        }
        ]);
}
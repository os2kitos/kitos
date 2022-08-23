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
        private readonly associatedSystemUsagesPropertyName = "AssociatedSystemUsages";
        private readonly lastChangedByUserPropertyName = "LastChangedByUser";
        private readonly dataProcessingRegistrationsPropertyName = "DataProcessingRegistrations";

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

        private renderProcurementPlan(year: number, quarter: number): string {
            return `Q${quarter} | ${year}`;
        }

        constructor(
            $rootScope: IRootScope,
            $scope: ng.IScope,
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

            const procurementOptions = [{
                textValue: " ",
                remoteValue: Helpers.KendoOverviewHelper.emptyOptionId.toString()
            }];

            procurements.map(value => {
                return {
                    textValue: this.renderProcurementPlan(value.procurementPlanYear, value.procurementPlanQuarter),
                    remoteValue: value.procurementPlanYear + "_" + value.procurementPlanQuarter
                };
            }).forEach(option => {
                procurementOptions.push(option);
            });

            $scope.procurements = procurementOptions;

            const uiBluePrint = Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            const getRoleKey = (roleId: number | string) => `role${roleId}`;

            this.criticalityOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.criticalityOptions);
            this.contractTypeOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.contractTypeOptions);
            this.contractTemplateOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.contractTemplateOptions);
            this.purchaseFormOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.purchaseFormOptions);
            this.procurementStrategyOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.procurementStrategyOptions);
            this.paymentModelOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.paymentModelOptions);
            this.paymentFrequencyOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.paymentFrequencyOptions);
            this.optionExtendOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.optionExtendOptions);
            this.terminationDeadlineOptionViewModel = new Models.ViewModel.Generic.OptionTypeViewModel(itContractOptions.terminationDeadlineOptions);

            this.yesNoUndecided = new Models.ViewModel.Shared.YesNoUndecidedOptions();

            const replaceRoleFilter = (filterUrl: string, roleName: string, roleId: number) => {
                const pattern = new RegExp(`(\\w+\\()${roleName}(.*?\\))`, "i");
                return filterUrl.replace(pattern, `Rights/any(c: $1concat(concat(c/User/Name, ' '), c/User/LastName)$2 and c/RoleId eq ${roleId})`);
            }

            const replaceSystemFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
                return filterUrl.replace(pattern, "AssociatedSystemUsages/any(c: $1c/ItSystemUsage/ItSystem/Name$2)");
            }

            const replaceSystemUuidFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
                return filterUrl.replace(pattern, "AssociatedSystemUsages/any(c: contains(cast(c/ItSystemUsage/ItSystem/Uuid, Edm.String)$2)");
            }

            const replaceOptionTypeFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`(${column}( eq )\'([0-9a-zA-Z]+)\')`, "i");
                const matchingFilterParts = pattern.exec(filterUrl);
                if (matchingFilterParts?.length !== 4)
                    return filterUrl;

                var searchedValue = matchingFilterParts[3];
                const emptyOptionId = Helpers.KendoOverviewHelper.emptyOptionId.toString();
                if (searchedValue.indexOf(emptyOptionId) !== -1) {
                    searchedValue = searchedValue.replace(emptyOptionId, "null");
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
                const pattern = new RegExp(`${column} eq \'([0-9]+)_([0-9]+)\'`, "i");
                const emptyOptionPattern = new RegExp(`${column} eq \'(${Helpers.KendoOverviewHelper.emptyOptionId})\'`, "i");
                const matchingFilterPart = pattern.exec(filterUrl);

                if (matchingFilterPart?.length !== 3) {
                    const emptyOptionMatch = emptyOptionPattern.exec(filterUrl);

                    if (emptyOptionMatch?.length === 2) {
                        filterUrl = filterUrl.replace(emptyOptionPattern, `(ProcurementPlanYear eq null and ProcurementPlanQuarter eq null)`);

                    }
                } else {
                    const year = matchingFilterPart[1];
                    const quarter = matchingFilterPart[2];

                    filterUrl = filterUrl.replace(pattern, `(ProcurementPlanYear eq ${year} and ProcurementPlanQuarter eq ${quarter})`);
                }

                return filterUrl;
            }

            const matchDprWithConcludedAgreement = (dpr: { IsAgreementConcluded: string | null }): boolean => {
                return dpr.IsAgreementConcluded && Models.Api.Shared.YesNoIrrelevantOption[dpr.IsAgreementConcluded] === Models.Api.Shared.YesNoIrrelevantOption.YES;
            }

            var launcher =
                kendoGridLauncherFactory
                    .create<Models.ViewModel.ItContract.IItContractOverviewViewModel>()
                    .withOverviewType(Models.Generic.OverviewType.ItContract)
                    .withScope($scope)
                    .withGridBinding(this)
                    .withUser(user)
                    .withEntityTypeName("IT Kontrakt")
                    .withExcelOutputName("IT Kontrakt Overblik")
                    .withStorageKey("it-contract-full-overview-options")
                    .withUrlFactory(options => {
                        var urlParameters =
                            "?$expand=" +
                            "Reference($select=URL,Title,ExternalReferenceId)," +
                            "Parent($select=Id,Name)," +
                            "ResponsibleOrganizationUnit($select=Name)," +
                            "PaymentModel($select=Name)," +
                            "PaymentFreqency($select=Name)," +
                            "Rights($select=Id,RoleId,UserId;$expand=User($select=Id,Name,LastName),Role($select=Name,Id))," +
                            "Supplier($select=Name)," +
                            "AssociatedSystemUsages($expand=ItSystemUsage($select=Id;$expand=ItSystem($select=Name,Disabled,Uuid)))," +
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
                            "AssociatedSystemRelations($select=Id)";

                        const selectedOrgId: number | null = options.currentOrgUnit;
                        var query = `/odata/Organizations(${user.currentOrganizationId})/`;

                        // if orgunit is set then the org unit filter is active
                        if (selectedOrgId === null) {
                            return `${query}ItContracts${urlParameters}`;
                        } else {
                            return `${query}OrganizationUnits(${selectedOrgId})/ItContracts${urlParameters}`;
                        }
                    })
                    .withStandardSorting("Name")
                    .withParameterMapping((options, type) => {
                        var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);
                        var activeOrgUnit: number | null = null;

                        if (parameterMap.$orderby) {

                            //Option types orderBy fixes
                            const optionTypeProperties: Array<string> = [
                                this.criticalityPropertyName,
                                this.contractTypePropertyName,
                                this.contractTemplatePropertyName,
                                this.purchaseFormPropertyName,
                                this.procurementStrategyPropertyName,
                                this.paymentModelPropertyName,
                                this.paymentFrequencyPropertyName,
                                this.optionExtendPropertyName,
                                this.terminationDeadlinePropertyName
                            ];

                            for (let optionTypePropertyName of optionTypeProperties) {
                                if (parameterMap.$orderby.includes(optionTypePropertyName)) {
                                    parameterMap.$orderby = parameterMap.$orderby.replace(optionTypePropertyName,
                                        `${optionTypePropertyName}/Name`);
                                }
                            }

                            //Fix Ordering based on last changed by user name
                            parameterMap.$orderby = Helpers.OdataQueryHelper.expandOrderingToMultipleProperties(
                                parameterMap.$orderby,
                                `${this.lastChangedByUserPropertyName}/Name`,
                                [`${this.lastChangedByUserPropertyName}/Name`, `${this.lastChangedByUserPropertyName}/LastName`]
                            );

                            //Fix procurement plan ordering to be by year and then by quarter
                            parameterMap.$orderby = Helpers.OdataQueryHelper.expandOrderingToMultipleProperties(
                                parameterMap.$orderby,
                                this.procurementPlanYearPropertyName,
                                [this.procurementPlanYearPropertyName, "ProcurementPlanQuarter"]
                            );
                        }

                        if (parameterMap.$filter) {
                            // Org unit is stripped from the odata query and passed on to the url factory!
                            const captureOrgUnit = new RegExp(`ResponsibleOrganizationUnit/Name eq '(\\d+)'`, "i");
                            if (captureOrgUnit.test(parameterMap.$filter)) {
                                activeOrgUnit = parseInt(captureOrgUnit.exec(parameterMap.$filter)[1]);
                            }
                            parameterMap.$filter = parameterMap.$filter.replace(captureOrgUnit, "");

                            //Fix role filters
                            _.forEach(itContractRoles,
                                (role: any) => parameterMap.$filter =
                                    replaceRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id));

                            //Fix system usage collection search
                            parameterMap.$filter = replaceSystemFilter(parameterMap.$filter, this.associatedSystemUsagesPropertyName);
                            parameterMap.$filter = replaceSystemUuidFilter(parameterMap.$filter, `${this.associatedSystemUsagesPropertyName}Uuids`);

                            //Fix search on user to cover both name and last name
                            const lastChangedByUserSearchedProperties = ["Name", "LastName"];
                            parameterMap.$filter = Helpers.OdataQueryHelper.replaceQueryByMultiplePropertyContains(parameterMap.$filter,
                                `${this.lastChangedByUserPropertyName}/Name`,
                                this.lastChangedByUserPropertyName,
                                lastChangedByUserSearchedProperties);

                            //Fix procurement plan filtering
                            parameterMap.$filter = replaceProcurementFilter(parameterMap.$filter, this.procurementPlanYearPropertyName);

                            //Option types filter fixes
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.criticalityPropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.contractTypePropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.contractTemplatePropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.purchaseFormPropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.procurementStrategyPropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.paymentModelPropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.paymentFrequencyPropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.optionExtendPropertyName);
                            parameterMap.$filter = replaceOptionTypeFilter(parameterMap.$filter, this.terminationDeadlinePropertyName);

                            //DPR filter fix
                            parameterMap.$filter = replaceDprFilter(parameterMap.$filter, this.dataProcessingRegistrationsPropertyName);

                            //Cleanup filter if invalid ODATA Filter (can happen if we strip params)
                            if (parameterMap.$filter === "") {
                                delete parameterMap.$filter;
                            }
                        }

                        //Making sure orgunit is set
                        (options as any).currentOrgUnit = activeOrgUnit;

                        return parameterMap;
                    })
                    .withResponseParser(response => {

                        response.forEach(contract => {
                            var ecoData = contract.ExternEconomyStreams ?? [];

                            //Only compute payment related stuff if needed
                            if (uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment)) {
                                contract.Acquisition = _.sumBy(ecoData, "Acquisition");
                                contract.Operation = _.sumBy(ecoData, "Operation");
                                contract.Other = _.sumBy(ecoData, "Other");

                                const streamsSortedByAuditDate = _.sortBy(ecoData, ["AuditDate"]);
                                const streamWithEarliestAuditDate = _.last(streamsSortedByAuditDate);
                                if (streamWithEarliestAuditDate && streamWithEarliestAuditDate.AuditDate) {
                                    contract.AuditDate = streamWithEarliestAuditDate.AuditDate;
                                }

                                const totalWhiteStatuses = _.filter(ecoData, { AuditStatus: "White" }).length;
                                const totalRedStatuses = _.filter(ecoData, { AuditStatus: "Red" }).length;
                                const totalYellowStatuses = _.filter(ecoData, { AuditStatus: "Yellow" }).length;
                                const totalGreenStatuses = _.filter(ecoData, { AuditStatus: "Green" }).length;

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
                            }

                            contract.roles = [];

                            //Only compute roles related stuff if needed
                            if (uiState.isBluePrintNodeAvailable(uiBluePrint.children.contractRoles)) {
                                // Create columns lookups for all assigned rights
                                _.forEach(contract.Rights,
                                    right => {
                                        // init an role array to hold users assigned to this role
                                        if (!contract.roles[right.RoleId])
                                            contract.roles[right.RoleId] = [];

                                        // push username to the role array
                                        contract.roles[right.RoleId].push(`${right.User.Name} ${right.User.LastName}`);
                                    });
                            }

                            //Ensure that object, where the data source is nested, are provided. Otherwise pre-render prep will fail in kendo grid's excel export function (even if we override the export)
                            contract.Parent = contract.Parent ?? {} as any;
                            contract.ResponsibleOrganizationUnit = contract.ResponsibleOrganizationUnit ?? {} as any;
                            contract.Supplier = contract.Supplier ?? {} as any;
                            contract.Reference = contract.Reference ?? {} as any;
                            contract.LastChangedByUser = contract.LastChangedByUser ?? { Name: "", LastName: "" } as any;
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
                        .withTitle("Gyldig/Ikke Gyldig")
                        .withId("isActive")
                        .withRendering(dataItem => dataItem.IsActive ? "Gyldig" : "Ikke Gyldig")
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withoutSorting())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItContractId")
                        .withTitle("Kontrakt ID")
                        .withId("contractId")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.contractId))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Parent.Name")
                        .withTitle("Overordnet kontrakt")
                        .withId("parentName")
                        .withStandardWidth(190)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => dataItem.Parent.Id !== undefined ? Helpers.RenderFieldsHelper.renderInternalReference(
                            "kendo-parent-rendering",
                            "it-contract.edit.main",
                            dataItem.Parent.Id,
                            dataItem.Parent.Name) : "")
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dataItem.Parent.Name)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Name")
                        .withTitle("IT Kontrakt")
                        .withId("contractName")
                        .withStandardWidth(190)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withContentOverflow()
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(
                            "contractNameObject",
                            "it-contract.edit.main",
                            dataItem.Id,
                            dataItem.Name))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.criticalityPropertyName)
                        .withTitle("Kritikalitet")
                        .withId("criticality")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withContentOverflow()
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.criticalityOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => dataItem.Criticality ? Helpers.RenderFieldsHelper.renderString(this.criticalityOptionViewModel.getOptionText(dataItem.Criticality.Id)) : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ResponsibleOrganizationUnit.Name")
                        .withTitle("Ansvarlig org. enhed")
                        .withId("responsibleOrganizationUnitName")
                        .withStandardWidth(190)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withContentOverflow()
                        .withFixedValueRange(orgUnits.map((unit) => {
                            return {
                                textValue: unit.Name,
                                remoteValue: unit.Id,
                                optionalContext: unit
                            };
                        }),
                            false,
                            dataItem => '&nbsp;&nbsp;&nbsp;&nbsp;'.repeat(dataItem.optionalContext.$level) + dataItem.optionalContext.Name)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.ResponsibleOrganizationUnit?.Name)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Supplier.Name")
                        .withTitle("Leverandør")
                        .withId("supplierName")
                        .withStandardWidth(190)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.Supplier?.Name)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ContractSigner")
                        .withTitle("Kontraktunderskriver")
                        .withId("contractSigner")
                        .withStandardWidth(190)
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.internalSigner))
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.contractTypePropertyName)
                        .withTitle("Kontrakttype")
                        .withId("contractType")
                        .withContentOverflow()
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.contractType))
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(Helpers.KendoOverviewHelper.mapDataForKendoDropdown(this.contractTypeOptionViewModel.enabledOptions, true), false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.contractTypeOptionViewModel.getOptionText(dataItem.ContractType?.Id))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.contractTemplatePropertyName)
                        .withTitle("Kontraktskabelon")
                        .withId("contractTemplate")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.contractTemplateOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.contractTemplateOptionViewModel.getOptionText(dataItem.ContractTemplate?.Id))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.purchaseFormPropertyName)
                        .withTitle("Indkøbsform")
                        .withId("purchaseForm")
                        .withContentOverflow()
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.purchaseForm))
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.purchaseFormOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.purchaseFormOptionViewModel.getOptionText(dataItem.PurchaseForm?.Id))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.procurementStrategyPropertyName)
                        .withTitle("Genanskaffelsesstrategi")
                        .withId("procurementStrategy")
                        .withStandardWidth(180)
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.procurementStrategy))
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.procurementStrategyOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.procurementStrategyOptionViewModel.getOptionText(dataItem.ProcurementStrategy?.Id))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.procurementPlanYearPropertyName)
                        .withTitle("Genanskaffelsesplan")
                        .withId("procurementPlanYear")
                        .withStandardWidth(165)
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.procurementPlan))
                        .withContentOverflow()
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange($scope.procurements, false)
                        .withRendering(dataItem => dataItem.ProcurementPlanQuarter && dataItem.ProcurementPlanYear
                            ? this.renderProcurementPlan(dataItem.ProcurementPlanYear, dataItem.ProcurementPlanQuarter)
                            : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ProcurementInitiated")
                        .withTitle("Genanskaffelse igangsat")
                        .withId("procurementInitiated")
                        .withStandardWidth(185)
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.procurementInitiated))
                        .withContentOverflow()
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(this.yesNoUndecided.options, false),
                            false)
                        .withRendering(dataItem => dataItem.ProcurementInitiated
                            ? Models.ViewModel.Shared.YesNoUndecidedOptions.getText(dataItem.ProcurementInitiated)
                            : ""));

            if (uiState.isBluePrintNodeAvailable(uiBluePrint.children.contractRoles)) {
                itContractRoles.forEach(role => {
                    const roleColumnId = `itContract${role.Id}`;
                    const roleKey = getRoleKey(role.Id);
                    launcher = launcher
                        .withColumn(builder =>
                            builder
                            .withDataSourceName(roleKey)
                            .withTitle(role.Name)
                            .withId(roleColumnId)
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withoutSorting()
                            .withContentOverflow()
                            .withRendering(dataItem => Helpers.RenderFieldsHelper.renderInternalReference(
                                `kendo-contract-${roleKey}-rendering`,
                                "it-contract.edit.roles",
                                dataItem.Id,
                                dataItem.roles[role.Id]?.toString() ?? ""))
                            .withExcelOutput(
                                dataItem => dataItem.roles[role.Id]?.toString() ?? ""));
                });
            }

            launcher = launcher
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.dataProcessingRegistrationsPropertyName)
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
                                if (matchDprWithConcludedAgreement(dpr)) {
                                    activeDprs.push(Helpers.RenderFieldsHelper.renderInternalReference(
                                        `kendo-contract-dpr-${dpr.Id}`,
                                        "data-processing.edit-registration.main",
                                        dpr.Id,
                                        dpr.Name));
                                }
                            });
                            return activeDprs.join(", ");
                        })
                        .withExcelOutput(dataItem => {
                            var activeDprs = [];
                            dataItem.DataProcessingRegistrations.forEach(dpr => {
                                if (matchDprWithConcludedAgreement(dpr)) {
                                    activeDprs.push(dpr.Name);
                                }
                            });
                            return activeDprs.join(", ");
                        }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.associatedSystemUsagesPropertyName)
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
                                    "it-system.usage.main",
                                    system.ItSystemUsageId,
                                    Helpers.SystemNameFormat.apply(system.ItSystemUsage.ItSystem.Name, system.ItSystemUsage.ItSystem.Disabled)));

                            });
                            return activeSystemUsages.join(", ");
                        })
                        .withExcelOutput(dataItem => {
                            var systemUsages = [];
                            dataItem.AssociatedSystemUsages.forEach(system => {
                                systemUsages.push(Helpers.SystemNameFormat.apply(system.ItSystemUsage.ItSystem.Name, system.ItSystemUsage.ItSystem.Disabled));
                            });
                            return systemUsages.join(", ");
                        }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(`${this.associatedSystemUsagesPropertyName}Uuids`)
                        .withTitle("IT Systemer (UUID)")
                        .withId("itSystemUuid")
                        .withContentOverflow()
                        .withoutSorting()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => {
                            var activeSystemUsages = [];
                            dataItem.AssociatedSystemUsages.forEach(system => {
                                activeSystemUsages.push(Helpers.RenderFieldsHelper.renderInternalReference(
                                    `kendo-contract-system-usages-uuid-${system.ItSystemUsageId}`,
                                    "it-system.usage.main",
                                    system.ItSystemUsageId,
                                    Helpers.SystemNameFormat.apply(system.ItSystemUsage.ItSystem.Uuid, system.ItSystemUsage.ItSystem.Disabled)));

                            });
                            return activeSystemUsages.join(", ");
                        })
                        .withExcelOutput(dataItem => {
                            var uuids = [];
                            if (dataItem.AssociatedSystemUsages?.length > 0) {
                                dataItem.AssociatedSystemUsages.forEach(value => {
                                    uuids.push(value.ItSystemUsage.ItSystem.Uuid);
                                });
                            }
                            return uuids.join(", ");
                        })
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("AssociatedSystemRelations")
                        .withTitle("Antal Relationer")
                        .withId("relationCount")
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withoutSorting()
                        .withRendering(dataItem => {
                            if (dataItem.AssociatedSystemUsages === undefined)
                                return "0";

                            return dataItem.AssociatedSystemRelations.length.toString();
                        }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Reference.Title")
                        .withTitle("Reference")
                        .withId("referenceTitle")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReferenceUrl(dataItem.Reference))
                        .withExcelOutput(dataItem => {
                            if (!dataItem.Reference) {
                                return "";
                            }
                            return dataItem.Reference.Title ?? dataItem.Reference.URL;
                        }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Reference.ExternalReferenceId")
                        .withTitle("Dokument ID/Sagsnr.")
                        .withId("referenceExternalReferenceId")
                        .withStandardWidth(170)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderExternalReferenceId(dataItem.Reference)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ExternEconomyStreams.Acquisition")
                        .withTitle("Anskaffelse")
                        .withId("acquisition")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withoutSorting()
                        .withRendering(dataItem => { return dataItem.Acquisition ? dataItem.Acquisition.toString() : ""; }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ExternEconomyStreams.Operation")
                        .withTitle("Drift/år")
                        .withId("operation")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withoutSorting()
                        .withRendering(dataItem => { return dataItem.Operation ? dataItem.Operation.toString() : ""; }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ExternEconomyStreams.Other")
                        .withTitle("Andet")
                        .withId("other")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withoutSorting()
                        .withRendering(dataItem => { return dataItem.Other ? dataItem.Other.toString() : ""; }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("OperationRemunerationBegun")
                        .withTitle("Driftsvederlag begyndt")
                        .withId("operationRemunerationBegun")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.paymentModel))
                        .withStandardWidth(170)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.OperationRemunerationBegun)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.paymentModelPropertyName)
                        .withTitle("Betalingsmodel")
                        .withId("paymentModel")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.paymentModel))
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.paymentModelOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => dataItem.PaymentModel ? Helpers.RenderFieldsHelper.renderString(this.paymentModelOptionViewModel.getOptionText(dataItem.PaymentModel?.Id)) : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.paymentFrequencyPropertyName)
                        .withTitle("Betalingsfrekvens")
                        .withId("paymentFrequency")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.paymentModel))
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.paymentFrequencyOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.paymentFrequencyOptionViewModel.getOptionText(dataItem.PaymentFreqency?.Id))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ExternEconomyStreams.AuditDate")
                        .withTitle("Audit dato")
                        .withId("auditDate")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withoutSorting()
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.AuditDate))
                        .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderDate(dataItem.AuditDate)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ExternEconomyStreams.AuditStatus")
                        .withTitle("Audit status")
                        .withId("auditStatus")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withoutSorting()
                        .withRendering(dataItem => {
                            if (dataItem.status.max > 0) {
                                const str = JSON.stringify(dataItem.status);
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
                        .withTitle("Varighed")
                        .withId("duration")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.agreementDeadlines))
                        .withoutSorting()
                        .withRendering(dataItem => {
                            if (dataItem.DurationOngoing) {
                                return "Løbende";
                            }

                            const years = dataItem.DurationYears || 0;
                            const months = dataItem.DurationMonths || 0;

                            let result = "";
                            if (years > 0) {
                                result += `${years} år`;
                                if (months > 0) {
                                    result += " og ";
                                }
                            }
                            if (months > 0) {
                                result += `${months} måned`;
                                if (months > 1) {
                                    result += "er";
                                }
                            }
                            return result;
                        }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.optionExtendPropertyName)
                        .withTitle("Option")
                        .withId("optionExtend")
                        .withContentOverflow()
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.agreementDeadlines))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.optionExtendOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => dataItem.OptionExtend ? Helpers.RenderFieldsHelper.renderString(this.optionExtendOptionViewModel.getOptionText(dataItem.OptionExtend?.Id)) : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.terminationDeadlinePropertyName)
                        .withTitle("Opsigelse (måneder)")
                        .withId("terminationDeadline")
                        .withStandardWidth(160)
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.termination))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.terminationDeadlineOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => dataItem.TerminationDeadline ? Helpers.RenderFieldsHelper.renderString(this.terminationDeadlineOptionViewModel.getOptionText(dataItem.TerminationDeadline?.Id)) : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("IrrevocableTo")
                        .withTitle("Uopsigelig til")
                        .withId("irrevocableTo")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.agreementDeadlines))
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.IrrevocableTo)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Terminated")
                        .withTitle("Opsagt")
                        .withId("terminated")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.termination))
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.Terminated)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(`${this.lastChangedByUserPropertyName}.Name`)
                        .withTitle("Sidst redigeret: Bruger")
                        .withId("lastChangedByUser")
                        .withStandardWidth(170)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => dataItem.LastChangedByUser ? `${dataItem.LastChangedByUser.Name} ${dataItem.LastChangedByUser.LastName}` : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LastChanged")
                        .withTitle("Sidste redigeret: Dato")
                        .withId("lastChangedDate")
                        .withStandardWidth(170)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.LastChanged)));
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
                            "localOptionServiceFactory", (localOptionServiceFactory: Services.LocalOptions.ILocalOptionServiceFactory) =>
                                localOptionServiceFactory.create(Services.LocalOptions.LocalOptionType.ItContractRoles).getAll()
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        userAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) =>
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
                            "uiCustomizationStateService", (uiCustomizationStateService: Services.UICustomization.IUICustomizationStateService) => uiCustomizationStateService.getCurrentState(Models.UICustomization.CustomizableKitosModule.ItContract)
                        ],
                        itContractOptions: [
                            "ItContractsService", "user",
                            (ItContractsService: Services.Contract.IItContractsService, user) =>
                                ItContractsService.getApplicableItContractOptions(user.currentOrganizationId)
                        ],
                        procurements: [
                            "ItContractsService", "user",
                            (ItContractsService: Services.Contract.IItContractsService, user) =>
                                ItContractsService.getAvailableProcurementPlans(user.currentOrganizationId)
                        ]
                    }
                });
        }
        ]);
}
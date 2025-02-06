module Kitos.ItContract.Overview {
    export interface IOverviewController extends Utility.KendoGrid.IGridViewAccess<Kitos.Models.ViewModel.ItContract.ItContractOverviewViewModel> {
    }

    export class OverviewController implements IOverviewController {
        mainGrid: IKendoGrid<Kitos.Models.ViewModel.ItContract.ItContractOverviewViewModel>;
        mainGridOptions: IKendoGridOptions<Kitos.Models.ViewModel.ItContract.ItContractOverviewViewModel>;
        canCreate: boolean;


        private readonly procurementInitiatedPropertyName = "ProcurementInitiated";
        private readonly criticalityIdPropertyName = "CriticalityId";
        private readonly contractTypeIdPropertyName = "ContractTypeId";
        private readonly contractTemplateIdPropertyName = "ContractTemplateId";
        private readonly purchaseFormIdPropertyName = "PurchaseFormId";
        private readonly procurementStrategyIdPropertyName = "ProcurementStrategyId";
        private readonly paymentModelIdPropertyName = "PaymentModelId";
        private readonly paymentFrequencyIdPropertyName = "PaymentFrequencyId";
        private readonly optionExtendIdPropertyName = "OptionExtendId";
        private readonly terminationDeadlineIdPropertyName = "TerminationDeadlineId";
        private readonly procurementPlanYearProperties = {
            year: "ProcurementPlanYear",
            quarter: "ProcurementPlanQuarter"
        };
        private readonly itSystemUsagesCsvPropertyName = "ItSystemUsagesCsv";
        private readonly dataProcessingAgreementsCsvPropertyName = "DataProcessingAgreementsCsv";

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
                var pattern = new RegExp(`(\\w+\\()${roleName}(,.*?\\))`, "i");
                return filterUrl.replace(pattern, `RoleAssignments/any(c: $1c/UserFullName$2 and c/RoleId eq ${roleId})`);
            }

            const replaceDprFilter = (filterUrl: string) => {
                const pattern = new RegExp(`(${this.dataProcessingAgreementsCsvPropertyName}( eq )\'([0-9]+)\')`, "i");
                const matchingFilterParts = pattern.exec(filterUrl);
                if (matchingFilterParts?.length !== 4)
                    return filterUrl;

                var searchedValue = matchingFilterParts[3];
                const yesValue = `${Models.Api.Shared.YesNoIrrelevantOption.YES.valueOf()}`;
                if (searchedValue.indexOf(yesValue) !== -1) {
                    return filterUrl.replace(pattern, "DataProcessingAgreements/Any()");
                }

                return filterUrl.replace(pattern, "(not DataProcessingAgreements/Any())");
            }

            const replaceProcurementFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`${column} eq \'([0-9]+)_([0-9]+)\'`, "i");
                const emptyOptionPattern = new RegExp(`${column} eq \'(${Helpers.KendoOverviewHelper.emptyOptionId})\'`, "i");
                const matchingFilterPart = pattern.exec(filterUrl);

                if (matchingFilterPart?.length !== 3) {
                    const emptyOptionMatch = emptyOptionPattern.exec(filterUrl);

                    if (emptyOptionMatch?.length === 2) {
                        filterUrl = filterUrl.replace(emptyOptionPattern, `(${this.procurementPlanYearProperties.year} eq null and ${this.procurementPlanYearProperties.quarter} eq null)`);

                    }
                } else {
                    const year = matchingFilterPart[1];
                    const quarter = matchingFilterPart[2];

                    filterUrl = filterUrl.replace(pattern, `(${this.procurementPlanYearProperties.year} eq ${year} and ${this.procurementPlanYearProperties.quarter} eq ${quarter})`);
                }

                return filterUrl;
            }

            const replaceOptionIdFilter = (filterUrl: string, column: string) => {
                const pattern = new RegExp(`${column} eq \'([0-9]+)\'`, "i");
                const emptyOptionPattern = new RegExp(`${column} eq \'(${Helpers.KendoOverviewHelper.emptyOptionId})\'`, "i");
                const matchingFilterPart = pattern.exec(filterUrl);

                if (matchingFilterPart?.length !== 2) {
                    const emptyOptionMatch = emptyOptionPattern.exec(filterUrl);

                    if (emptyOptionMatch?.length === 2) {
                        filterUrl = filterUrl.replace(emptyOptionPattern, `(${column} eq null)`);

                    }
                } else {
                    const value = matchingFilterPart[1];

                    filterUrl = filterUrl.replace(pattern, `(${column} eq ${value})`);
                }

                return filterUrl;
            }

            const createAuditStatusSummary = (dataItem: Kitos.Models.ViewModel.ItContract.ItContractOverviewViewModel) => {
                return {
                    max: dataItem.AuditStatusGreen + dataItem.AuditStatusRed + dataItem.AuditStatusWhite + dataItem.AuditStatusYellow,
                    red: dataItem.AuditStatusRed,
                    green: dataItem.AuditStatusGreen,
                    white: dataItem.AuditStatusWhite,
                    yellow: dataItem.AuditStatusYellow
                };
            }

            var launcher =
                kendoGridLauncherFactory
                    .create<Kitos.Models.ViewModel.ItContract.ItContractOverviewViewModel>()
                    .withOverviewType(Models.Generic.OverviewType.ItContract)
                    .withScope($scope)
                    .withGridBinding(this)
                    .withUser(user)
                    .withEntityTypeName("IT Kontrakt")
                    .withExcelOutputName("IT Kontrakt Overblik")
                    .withStorageKey("it-contract-full-overview-options-v3")
                    .withUrlFactory(options => {

                        var urlParameters =
                            "?$expand=" +
                            "RoleAssignments($select=RoleId,UserId,UserFullName,Email)," +
                            "DataProcessingAgreements($select=DataProcessingRegistrationId,DataProcessingRegistrationName)," +
                            "ItSystemUsages($select=ItSystemUsageId,ItSystemUsageName,ItSystemIsDisabled)";

                        const selectedOrgId: number | null = options.currentOrgUnit;
                        var query = `/odata/Organizations(${user.currentOrganizationId})/ItContractOverviewReadModels`;

                        // if orgunit is set then the org unit filter is active
                        if (selectedOrgId === null) {
                            return `${query}${urlParameters}`;
                        } else {
                            return `${query}${urlParameters}&responsibleOrganizationUnitId=${selectedOrgId}`;
                        }
                    })
                    .withStandardSorting("Name")
                    .withParameterMapping((options, type) => {
                        var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);
                        var activeOrgUnit: number | null = null;

                        //Option types orderBy fixes
                        const optionTypeProperties = [
                            { id: this.criticalityIdPropertyName, name: "CriticalityName" },
                            { id: this.contractTypeIdPropertyName, name: "ContractTypeName" },
                            { id: this.contractTemplateIdPropertyName, name: "ContractTemplateName" },
                            { id: this.purchaseFormIdPropertyName, name: "PurchaseFormName" },
                            { id: this.procurementStrategyIdPropertyName, name: "ProcurementStrategyName" },
                            { id: this.paymentModelIdPropertyName, name: "PaymentModelName" },
                            { id: this.paymentFrequencyIdPropertyName, name: "PaymentFrequencyName" },
                            { id: this.optionExtendIdPropertyName, name: "OptionExtendName" },
                            { id: this.terminationDeadlineIdPropertyName, name: "TerminationDeadlineName" },
                        ];

                        if (parameterMap.$orderby) {
                            for (let idToName of optionTypeProperties) {
                                if (parameterMap.$orderby.includes(idToName.id)) {
                                    parameterMap.$orderby = parameterMap.$orderby.replace(idToName.id,
                                        idToName.name);
                                }
                            }

                            //Fix procurement plan ordering to be by year and then by quarter
                            parameterMap.$orderby = Helpers.OdataQueryHelper.expandOrderingToMultipleProperties(
                                parameterMap.$orderby,
                                this.procurementPlanYearProperties.year,
                                [this.procurementPlanYearProperties.year, this.procurementPlanYearProperties.quarter]
                            );
                        }

                        if (parameterMap.$filter) {

                            //NOTE: all option type ids are of type string to allow the null/nan value. Otherwise filtering will not be populated. Null is also ignored on numeric columns
                            for (let idToName of optionTypeProperties) {
                                parameterMap.$filter = replaceOptionIdFilter(parameterMap.$filter, idToName.id);
                            }

                            // Org unit is stripped from the odata query and passed on to the url factory!
                            const captureOrgUnit = new RegExp(`ResponsibleOrgUnitName eq '(\\d+)'`, "i");
                            if (captureOrgUnit.test(parameterMap.$filter)) {
                                activeOrgUnit = parseInt(captureOrgUnit.exec(parameterMap.$filter)[1]);
                            }
                            parameterMap.$filter = parameterMap.$filter.replace(captureOrgUnit, "");

                            //Fix role filters
                            _.forEach(itContractRoles,
                                (role: Models.IEntity) => parameterMap.$filter =
                                    replaceRoleFilter(parameterMap.$filter, getRoleKey(role.Id), role.Id));

                            //Fix procurement plan filtering
                            parameterMap.$filter = replaceProcurementFilter(parameterMap.$filter, this.procurementPlanYearProperties.year);

                            //Redirect CSV field search towards optimized search targets (which have indexes due to constrained - known - max size)
                            parameterMap.$filter = parameterMap.$filter
                                .replace(/(\w+\()ItSystemUsagesCsv(.*\))/, "ItSystemUsages/any(c: $1c/ItSystemUsageName$2)")
                                .replace(/(\w+\()ItSystemUsagesSystemUuidCsv(.*\))/, "ItSystemUsages/any(c: $1c/ItSystemUsageSystemUuid$2)");

                            //DPR fix - interpret the yes | no to lookups in the backend collections
                            parameterMap.$filter = replaceDprFilter(parameterMap.$filter);

                            //blank/null fix for enum types
                            parameterMap.$filter = Helpers.OdataQueryHelper.replaceOptionQuery(parameterMap.$filter,
                                this.procurementInitiatedPropertyName,
                                Models.Api.Shared.YesNoUndecidedOption.Undecided);

                            //Cleanup broken queries due to stripping
                            Helpers.OdataQueryHelper.cleanupModifiedKendoFilterConfig(parameterMap);
                        }

                        //Making sure orgunit is set
                        (options as any).currentOrgUnit = activeOrgUnit;

                        return parameterMap;
                    })
                    .withResponseParser(response => {

                        response.forEach(contract => {
                            contract.roles = [];

                            //Only compute roles related stuff if needed
                            if (uiState.isBluePrintNodeAvailable(uiBluePrint.children.contractRoles)) {
                                // Create columns lookups for all assigned rights
                                _.forEach(contract.RoleAssignments,
                                    assignment => {
                                        // Role User names
                                        if (!contract.roles[assignment.RoleId])
                                            contract.roles[assignment.RoleId] = [];

                                        contract.roles[assignment.RoleId].push({ name: assignment.UserFullName, email: assignment.Email });
                                    });
                            }
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
                    color: Utility.KendoGrid.KendoToolbarButtonColor.None,
                    position: Utility.KendoGrid.KendoToolbarButtonPosition.Left,
                    margins: [Utility.KendoGrid.KendoToolbarMargin.Left],
                    implementation: Utility.KendoGrid.KendoToolbarImplementation.DropDownList,
                    standardWidth: Utility.KendoGrid.KendoToolbarStandardWidth.Standard,
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
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange([
                            {
                                remoteValue: true,
                                textValue: "Gyldig"
                            },
                            {
                                remoteValue: false,
                                textValue: "Ikke Gyldig"
                            }
                        ], false))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ContractId")
                        .withTitle("Kontrakt ID")
                        .withId("contractId")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.contractId)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ParentContractName")
                        .withTitle("Overordnet kontrakt")
                        .withId("parentName")
                        .withStandardWidth(190)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => dataItem.ParentContractId != null ? Helpers.RenderFieldsHelper.renderInternalReference(
                            "kendo-parent-rendering",
                            "it-contract.edit.main",
                            dataItem.ParentContractId,
                            dataItem.ParentContractName) : "")
                        .withSourceValueEchoExcelOutput())
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
                            dataItem.SourceEntityId,
                            dataItem.Name))
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Concluded")
                        .withTitle("Gyldig fra")
                        .withId("concluded")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.agreementPeriod))
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.Concluded)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ExpirationDate")
                        .withTitle("Gyldig til")
                        .withId("expirationDate")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.agreementPeriod))
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.ExpirationDate)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.criticalityIdPropertyName)
                        .withTitle("Kritikalitet")
                        .withId("criticality")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withContentOverflow()
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.criticalityOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => dataItem.CriticalityId ? Helpers.RenderFieldsHelper.renderString(this.criticalityOptionViewModel.getOptionText(dataItem.CriticalityId)) : "")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.criticality)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ResponsibleOrgUnitName")
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
                            dataItem => '&nbsp;&nbsp;&nbsp;&nbsp;'.repeat(dataItem.optionalContext.$level) + dataItem.optionalContext.Name,
                            true)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(dataItem.ResponsibleOrgUnitName)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("SupplierName")
                        .withTitle("Leverandør")
                        .withId("supplierName")
                        .withStandardWidth(190)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ContractSigner")
                        .withTitle("Kontraktunderskriver")
                        .withId("contractSigner")
                        .withStandardWidth(190)
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.internalSigner))
                        .withContentOverflow()
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.contractTypeIdPropertyName)
                        .withTitle("Kontrakttype")
                        .withId("contractType")
                        .withContentOverflow()
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.contractType))
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(Helpers.KendoOverviewHelper.mapDataForKendoDropdown(this.contractTypeOptionViewModel.enabledOptions, true), false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.contractTypeOptionViewModel.getOptionText(dataItem.ContractTypeId))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.contractTemplateIdPropertyName)
                        .withTitle("Kontraktskabelon")
                        .withId("contractTemplate")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withFixedValueRange(
                            Helpers.KendoOverviewHelper.mapDataForKendoDropdown(
                                this.contractTemplateOptionViewModel.enabledOptions,
                                true),
                            false)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.contractTemplateOptionViewModel.getOptionText(dataItem.ContractTemplateId)))
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage.children.template)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.purchaseFormIdPropertyName)
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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.purchaseFormOptionViewModel.getOptionText(dataItem.PurchaseFormId))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.procurementStrategyIdPropertyName)
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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.procurementStrategyOptionViewModel.getOptionText(dataItem.ProcurementStrategyId))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.procurementPlanYearProperties.year)
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
                        .withDataSourceName(this.procurementInitiatedPropertyName)
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
                    const roleEmailColumnId = `${roleColumnId}_emails`;
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
                                    dataItem.SourceEntityId,
                                    dataItem.roles[role.Id]?.map(r => r.name)?.join(", ") ?? ""))
                                .withExcelOutput(
                                    dataItem => dataItem.roles[role.Id]?.map(r => r.name)?.join(", ") ?? ""))
                        .withExcelOnlyColumn(builder =>
                            builder
                                .withId(roleEmailColumnId)
                                .withDataSourceName(`${roleKey}_emails`)
                                .withTitle(`${role.Name} Email"`)
                                .withParentColumnId(roleColumnId)
                                .withExcelOutput(dataItem => Helpers.ExcelExportHelper.renderString(dataItem.roles[role.Id]?.map(r => r.email)?.join(", ")))
                        );
                });
            }

            launcher = launcher
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.dataProcessingAgreementsCsvPropertyName)
                        .withTitle("Databehandleraftale")
                        .withId("dataProcessingRegistrations")
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
                            dataItem.DataProcessingAgreements.forEach(dpr => {
                                activeDprs.push(Helpers.RenderFieldsHelper.renderInternalReference(
                                    `kendo-contract-dpr-${dpr.DataProcessingRegistrationId}`,
                                    "data-processing.edit-registration.main",
                                    dpr.DataProcessingRegistrationId,
                                    dpr.DataProcessingRegistrationName));
                            });
                            return activeDprs.join(", ");
                        })
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.itSystemUsagesCsvPropertyName)
                        .withTitle("IT Systemer")
                        .withId("associatedSystemUsages")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withContentOverflow()
                        .withRendering(dataItem => {
                            var activeSystemUsages = [];
                            dataItem.ItSystemUsages.forEach(system => {
                                activeSystemUsages.push(Helpers.RenderFieldsHelper.renderInternalReference(
                                    `kendo-contract-system-usages-${system.ItSystemUsageId}`,
                                    "it-system.usage.main",
                                    system.ItSystemUsageId,
                                    Helpers.SystemNameFormat.apply(system.ItSystemUsageName, system.ItSystemIsDisabled)));

                            });
                            return activeSystemUsages.join(", ");
                        })
                        .withSourceValueEchoExcelOutput())
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ItSystemUsagesSystemUuidCsv")
                        .withTitle("IT Systemer (UUID)")
                        .withId("itSystemUuid")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("NumberOfAssociatedSystemRelations")
                        .withTitle("Antal Relationer")
                        .withId("relationCount")
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Number)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.NumberComparision)
                        .withSourceValueEchoRendering()
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ActiveReferenceTitle")
                        .withTitle("Reference")
                        .withId("referenceTitle")
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReference(dataItem.ActiveReferenceTitle, dataItem.ActiveReferenceUrl))
                        .withExcelOutput(dataItem => dataItem.ActiveReferenceTitle ?? dataItem.ActiveReferenceUrl ?? ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("ActiveReferenceExternalReferenceId")
                        .withTitle("Dokument ID/Sagsnr.")
                        .withId("referenceExternalReferenceId")
                        .withStandardWidth(170)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderReferenceId(dataItem.ActiveReferenceExternalReferenceId)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("AccumulatedAcquisitionCost")
                        .withTitle("Anskaffelse")
                        .withId("acquisition")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Number)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.NumberComparision)
                        .withSourceValueEchoRendering()
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("AccumulatedOperationCost")
                        .withTitle("Drift/år")
                        .withId("operation")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Number)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.NumberComparision)
                        .withSourceValueEchoRendering()
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("AccumulatedOtherCost")
                        .withTitle("Andet")
                        .withId("other")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Number)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.NumberComparision)
                        .withSourceValueEchoRendering()
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("OperationRemunerationBegunDate")
                        .withTitle("Driftsvederlag begyndt")
                        .withId("operationRemunerationBegun")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.paymentModel))
                        .withStandardWidth(170)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.OperationRemunerationBegunDate)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.paymentModelIdPropertyName)
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
                        .withRendering(dataItem => dataItem.PaymentModelId ? Helpers.RenderFieldsHelper.renderString(this.paymentModelOptionViewModel.getOptionText(dataItem.PaymentModelId)) : ""))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.paymentFrequencyIdPropertyName)
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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.paymentFrequencyOptionViewModel.getOptionText(dataItem.PaymentFrequencyId))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LatestAuditDate")
                        .withTitle("Audit dato")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withId("auditDate")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.LatestAuditDate))
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("AuditStatusWhite")
                        .withTitle("Audit status")
                        .withId("auditStatus")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.economy.children.extPayment))
                        .withoutSorting()
                        .withRendering(dataItem => {
                            const statuses = createAuditStatusSummary(dataItem);
                            if (statuses.max > 0) {
                                const str = JSON.stringify(statuses);
                                return `<div data-show-status='${str}'></div>`;
                            }
                            return "";
                        })
                        .withExcelOutput(dataItem => {
                            const statuses = createAuditStatusSummary(dataItem);
                            return statuses.max > 0 ?
                                `Hvid: ${statuses.white}, Rød: ${statuses.red}, Gul: ${statuses.yellow}, Grøn: ${statuses.green}, Max: ${statuses.max}` :
                                "";
                        }))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("Duration")
                        .withTitle("Varighed")
                        .withId("duration")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.agreementDeadlines))
                        .withFilteringOperation(Kitos.Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.optionExtendIdPropertyName)
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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.optionExtendOptionViewModel.getOptionText(dataItem.OptionExtendId))))
                .withColumn(builder =>
                    builder
                        .withDataSourceName(this.terminationDeadlineIdPropertyName)
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
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderString(this.terminationDeadlineOptionViewModel.getOptionText(dataItem.TerminationDeadlineId))))
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
                        .withDataSourceName("TerminatedAt")
                        .withTitle("Opsagt")
                        .withId("terminated")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.deadlines.children.termination))
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.TerminatedAt)))
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LastEditedByUserName")
                        .withTitle("Sidst redigeret: Bruger")
                        .withId("lastChangedByUser")
                        .withStandardWidth(170)
                        .withContentOverflow()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                )
                .withColumn(builder =>
                    builder
                        .withDataSourceName("LastEditedAtDate")
                        .withTitle("Sidste redigeret: Dato")
                        .withId("lastChangedDate")
                        .withStandardWidth(170)
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Date)
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Date)
                        .withRendering(dataItem => Helpers.RenderFieldsHelper.renderDate(dataItem.LastEditedAtDate)));
            launcher.launch();
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider) => {
            $stateProvider.state(Kitos.Constants.ApplicationStateId.ContractOverview,
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
                            "_", "organizationUnitOdataService",
                            (_, organizationUnitOdataService: Services.Organization.IOrganizationUnitOdataService) => organizationUnitOdataService
                                .getOrganizationUnits()
                                .then(result => _.addHierarchyLevelOnFlatAndSort(result, "Id", "ParentId"))
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
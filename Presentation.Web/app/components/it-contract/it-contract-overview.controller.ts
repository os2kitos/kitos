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
        private readonly orgUnitStorageKey = "it-contract-overview-orgunit";
        private readonly criticalityOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly contractTypeOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly contractTemplateOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly purchaseFormOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly procurementStrategyOptionViewModel: Models.ViewModel.Generic.OptionTypeViewModel;
        private readonly yesNoUndecided: Models.ViewModel.Shared.YesNoUndecidedOptions;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$http",
            "$timeout",
            "$window",
            "$state",
            "$",
            "_",
            "notify",
            "user",
            "gridStateService",
            "itContractRoles",
            "orgUnits",
            "$uibModal",
            "needsWidthFixService",
            "exportGridToExcelService",
            "userAccessRights",
            "uiState",
            "itContractOptions",
            "kendoGridLauncherFactory"
        ];

        constructor(
            $rootScope: IRootScope,
            $scope: ng.IScope,
            $http: ng.IHttpService,
            $timeout: ng.ITimeoutService,
            $window: ng.IWindowService,
            $state: ng.ui.IStateService,
            $: JQueryStatic,
            _: ILoDashWithMixins,
            //readonly moment: moment.MomentStatic,
            notify,
            user,
            gridStateService: Services.IGridStateFactory,
            itContractRoles,
            orgUnits,
            $modal,
            needsWidthFixService,
            exportGridToExcelService,
            userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO,
            uiState: Models.UICustomization.ICustomizedModuleUI,
            itContractOptions: Models.ItContract.IItContractOptions,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory) {
            $rootScope.page.title = "IT Kontrakt";

            const uiBluePrint = Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

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

            this.yesNoUndecided = new Models.ViewModel.Shared.YesNoUndecidedOptions();
            
            const createItContract = () => {

                $modal.open({
                    windowClass: "modal fade in",
                    templateUrl: "app/components/it-contract/it-contract-modal-create.view.html",
                    controller: [
                        "$scope", "$uibModalInstance", function($scope, $modalInstance) {
                            $scope.formData = {};
                            $scope.type = "IT Kontrakt";
                            $scope.checkAvailbleUrl = "api/itContract/";

                            $scope.saveAndProceed = () => {

                                var orgId = this.user.currentOrganizationId;
                                var msg = this.notify.addInfoMessage("Opretter kontrakt...", false);

                                this.$http.post(`api/itcontract?organizationId=${this.user.currentOrganizationId}`,
                                        { organizationId: orgId, name: $scope.formData.name })
                                    .then(function onSuccess(result: any) {
                                            msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                            var contract = result.data.response;
                                            $modalInstance.close(contract.id);
                                            this.$state.go("it-contract.edit.main", { id: contract.id });
                                        },
                                        function onError(result) {
                                            msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                                        });
                            };

                            $scope.save = () => {

                                var orgId = this.user.currentOrganizationId;
                                var msg = this.notify.addInfoMessage("Opretter kontrakt...", false);

                                this.$http.post(`api/itcontract?organizationId=${this.user.currentOrganizationId}`,
                                        { organizationId: orgId, name: $scope.formData.name })
                                    .then(function onSuccess(result: any) {
                                            msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                            var contract = result.data.response;
                                            $modalInstance.close(contract.id);
                                            this.$state.reload();
                                        },
                                        function onError(result) {
                                            msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                                        });
                            };
                        }
                    ]
                });
            }

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
                    return filterUrl.replace(pattern, `${column}/Any (c:c/IsAgreementConcluded eq ${searchedValue})`);
                }
                
                return filterUrl.replace(pattern, `${column}/Any (c:c/IsAgreementConcluded ne '${yesValue}')`);
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
                                `${this.procurementStrategyPropertyName}($select=Id)`;

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
                            if (parameterMap.$orderby.includes(this.criticalityPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.criticalityPropertyName,
                                    `${this.criticalityPropertyName}/Name`);
                            }
                            if (parameterMap.$orderby.includes(this.contractTypePropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.contractTypePropertyName,
                                    `${this.contractTypePropertyName}/Name`);
                            }
                        }

                        if (parameterMap.$filter) {
                            _.forEach(itContractRoles,
                                role => parameterMap.$filter =
                                replaceRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id));

                            parameterMap.$filter =
                                replaceSystemFilter(parameterMap.$filter, "AssociatedSystemUsages");

                            parameterMap.$filter = Helpers.fixODataUserByNameFilter(parameterMap.$filter,
                                "LastChangedByUser/Name",
                                "LastChangedByUser");

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
                                replaceDprFilter(parameterMap.$filter, "DataProcessingRegistrations");
                        }

                        return parameterMap;
                    })
                    .withResponseParser(response => {
                        //Build lookups/mutations
                        response.forEach(contract => {
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
                        onClick: () => createItContract()
                    } as Utility.KendoGrid.IKendoToolbarEntry)
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
                            () => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage)))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("ItContractId")
                        .withTitle("Kontrakt ID")
                        .withId("contractId")
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput())
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
                            dataItem.Parent?.Id,
                            dataItem.Parent?.Name))
                        .withExcelOutput(dataItem => dataItem.Parent?.Name))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("Name")
                        .withTitle("It Kontrakt")
                        .withId("name")
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
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains))
                    .withColumn(builder =>
                        builder
                        .withDataSourceName("ContractSigner")
                        .withTitle("Kontraktunderskriver")
                        .withId("contractSigner")
                        .withSourceValueEchoRendering()
                        .withSourceValueEchoExcelOutput()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains))
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
                            this.contractTypeOptionViewModel.getOptionText(dataItem.ContractType?.Id))))
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
                            this.purchaseFormOptionViewModel.getOptionText(dataItem.PurchaseForm?.Id))))
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
                        .withDataSourceName("ProcurementPlanYear")
                        .withTitle("Genanskaffelsesplan")
                        .withId("procurementPlanYear")
                        .withDataSourceType(Utility.KendoGrid.KendoGridColumnDataSourceType.Number)
                        //******************************
                        //TODO: FINISH COLUMN
                        //******************************
                        //.withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.FixedValueRange)
                        .withRendering(dataItem => dataItem.ProcurementPlanQuarter && dataItem.ProcurementPlanYear
                            ? `${dataItem.ProcurementPlanYear} | ${dataItem.ProcurementPlanQuarter}`
                            : "")
                        .withExcelOutput(
                            dataItem => `${dataItem.ProcurementPlanYear} | Q${dataItem.ProcurementPlanQuarter}`))
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
                            : ""));

            itContractRoles.forEach(role => {
                const roleColumnId = `itContract${role.Id}`;
                const roleKey = `role${role.Id}`;
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
                            if (dpr.IsAgreementConcluded.toString() === "YES") {
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
                        ]
                    }
                });
        }
        ]);
}
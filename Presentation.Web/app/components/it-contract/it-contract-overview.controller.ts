module Kitos.ItContract.Overview {
    export interface IOverviewController extends Utility.KendoGrid.IGridViewAccess<Models.ItContract.IItContract> {
    }

    export class OverviewController implements IOverviewController {
        mainGrid: IKendoGrid<Models.ItContract.IItContract>;
        mainGridOptions: IKendoGridOptions<Models.ItContract.IItContract>;
        canCreate: boolean;
        private readonly criticalityPropertyName = "Criticality";
        private readonly orgUnitStorageKey = "it-contract-overview-orgunit";

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
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $window: ng.IWindowService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: ILoDashWithMixins,
            private readonly moment: moment.MomentStatic,
            private notify,
            private user,
            private gridStateService: Services.IGridStateFactory,
            private itContractRoles,
            private orgUnits,
            private $modal,
            private needsWidthFixService,
            private exportGridToExcelService,
            private userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO,
            private uiState: Models.UICustomization.ICustomizedModuleUI,
            private itContractOptions: Models.ItContract.IItContractOptions,
            kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory) {
            this.$rootScope.page.title = "IT Kontrakt";
            const uiBluePrint = Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            const createItContract = () => {

                this.$modal.open({
                    windowClass: "modal fade in",
                    templateUrl: "app/components/it-contract/it-contract-modal-create.view.html",
                    controller: ["$scope", "$uibModalInstance", function ($scope, $modalInstance) {
                        $scope.formData = {};
                        $scope.type = "IT Kontrakt";
                        $scope.checkAvailbleUrl = "api/itContract/";

                        $scope.saveAndProceed = () => {

                            var orgId = this.user.currentOrganizationId;
                            var msg = this.notify.addInfoMessage("Opretter kontrakt...", false);

                            this.$http.post(`api/itcontract?organizationId=${this.user.currentOrganizationId}`, { organizationId: orgId, name: $scope.formData.name })
                                .then(function onSuccess(result: any) {
                                    msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                    var contract = result.data.response;
                                    $modalInstance.close(contract.id);
                                    this.$state.go("it-contract.edit.main", { id: contract.id });
                                }, function onError(result) {
                                    msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                                });
                        };

                        $scope.save = () => {

                            var orgId = this.user.currentOrganizationId;
                            var msg = this.notify.addInfoMessage("Opretter kontrakt...", false);

                            this.$http.post(`api/itcontract?organizationId=${this.user.currentOrganizationId}`, { organizationId: orgId, name: $scope.formData.name })
                                .then(function onSuccess(result: any) {
                                    msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                    var contract = result.data.response;
                                    $modalInstance.close(contract.id);
                                    this.$state.reload();
                                }, function onError(result) {
                                    msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                                });
                        };
                    }]
                });
            }
            
            // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(concat(concat(c/User/Name, ' '), c/User/LastName),'foo') and c/RoleId eq {roleId})"
            const replaceRoleFilter = (filterUrl, roleName, roleId) => {
                const pattern = new RegExp(`(\\w+\\()${roleName}(.*?\\))`, "i");
                return filterUrl.replace(pattern, `Rights/any(c: $1concat(concat(c/User/Name, ' '), c/User/LastName)$2 and c/RoleId eq ${roleId})`);
            }

            const replaceSystemFilter = (filterUrl, column) => {
                const pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
                return filterUrl.replace(pattern, "AssociatedSystemUsages/any(c: $1c/ItSystemUsage/ItSystem/Name$2)");
            }

            const replaceCriticalityFilter = (filterUrl, column) => {
                const pattern = new RegExp(`(${column}( eq )\'([0-9]+)\')`, "i");
                const renamedColumn = filterUrl.replace(pattern, `${this.criticalityPropertyName}/Id$2$3`);

                if (renamedColumn.includes("0")) {
                    return renamedColumn.replace("0", "null");
                }

                return renamedColumn;
            }

            var getSourceUrl = () =>
            {
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
                        "DataProcessingRegistrations($select=IsAgreementConcluded)," +
                        "LastChangedByUser($select=Name,LastName)," +
                        "ExternEconomyStreams($select=Acquisition,Operation,Other,AuditStatus,AuditDate)," +
                        `${this.criticalityPropertyName}($select=Id)`;

                var orgUnitId = this.$window.sessionStorage.getItem(this.orgUnitStorageKey);
                var query = `/odata/Organizations(${this.user.currentOrganizationId})/`;
                // if orgunit is set then the org unit filter is active
                if (orgUnitId === null) {
                    return query + `ItContracts` + urlParameters;
                } else {
                    return query + `OrganizationUnits(${orgUnitId})/ItContracts` + urlParameters;
                }
            }

            var contractRoleIdToUserNamesMap = {};

            var launcher =
                kendoGridLauncherFactory
                    .create<Models.ItContract.IItContract>()
                    .withScope($scope)
                    .withGridBinding(this)
                    .withUser(user)
                    .withEntityTypeName("IT Kontrakt")
                    .withExcelOutputName("IT Kontrakt")
                    .withStorageKey("it-contract-overview-options")
                        .withFixedSourceUrl(getSourceUrl())
                    .withParameterMapping((options, type) => {
                        var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                        if (parameterMap.$orderby) {
                            if (parameterMap.$orderby.includes(this.criticalityPropertyName)) {
                                parameterMap.$orderby = parameterMap.$orderby.replace(this.criticalityPropertyName, `${this.criticalityPropertyName}/Name`);
                            }
                        }

                        if (parameterMap.$filter) {
                            this._.forEach(this.itContractRoles,
                                role => parameterMap.$filter = replaceRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id));

                            parameterMap.$filter = replaceSystemFilter(parameterMap.$filter, "AssociatedSystemUsages");

                            parameterMap.$filter = Helpers.fixODataUserByNameFilter(parameterMap.$filter, "LastChangedByUser/Name", "LastChangedByUser");

                            parameterMap.$filter = replaceCriticalityFilter(parameterMap.$filter, this.criticalityPropertyName);
                        }

                        return parameterMap;
                    })
                    .withToolbarEntry({
                        id: "createContract",
                        title: "Opret IT Kontrakt",
                        color: Utility.KendoGrid.KendoToolbarButtonColor.Green,
                        position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                        margins: [Utility.KendoGrid.KendoToolbarMargin.Left],
                        implementation: Utility.KendoGrid.KendoToolbarImplementation.Button,
                        enabled: () => userAccessRights.canCreate,
                        onClick: () => $state.go("it-contract-modal-create")
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
                        .withRendering(dataItem => dataItem.IsActive ? '<span class="fa fa-file text-success" aria-hidden="true"></span>' : '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>')
                        .withContentAlignment(Utility.KendoGrid.KendoColumnAlignment.Center)
                        .withExcelOutput(dataItem => dataItem.IsActive ? "Gyldig" : "Ikke gyldig")
                        .withInclusionCriterion(() => uiState.isBluePrintNodeAvailable(uiBluePrint.children.frontPage)))
                    .withColumn(builder => 
                        builder.withDataSourceName("ItContractId")
                            .withTitle("Kontrakt ID")
                            .withId("contractId")
                            .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                            .withSourceValueEchoRendering())
        }
    }
}
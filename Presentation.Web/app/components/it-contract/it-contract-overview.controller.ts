module Kitos.ItContract.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: IKendoGrid<IItContractOverview>;
        mainGridOptions: kendo.ui.GridOptions;
        roleSelectorOptions: any;

        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): void;
        clearOptions(): void;
    }

    export interface IItContractOverview extends Models.ItContract.IItContract {
        Acquisition: number;
        Operation: number;
        Other: number;
        AuditDate: string;
        status: {
            max: number;
            white: number;
            red: number;
            yellow: number;
            green: number;
        };
        roles: Array<any>;
    }

    export class OverviewController implements IOverviewController {
        private storageKey = "it-contract-overview-options";
        private orgUnitStorageKey = "it-contract-overview-orgunit";
        private gridState = this.gridStateService.getService(this.storageKey, this.user);
        private roleSelectorDataSource;
        public mainGrid: IKendoGrid<IItContractOverview>;
        public mainGridOptions: kendo.ui.GridOptions;
        public canCreate: boolean;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$http",
            "$timeout",
            "$window",
            "$state",
            "$",
            "_",
            "moment",
            "notify",
            "user",
            "gridStateService",
            "itContractRoles",
            "orgUnits",
            "ecoStreamData",
            "$uibModal",
            "needsWidthFixService",
            "exportGridToExcelService",
            "userAccessRights"
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
            private moment: moment.MomentStatic,
            private notify,
            private user,
            private gridStateService: Services.IGridStateFactory,
            private itContractRoles,
            private orgUnits,
            private ecoStreamData,
            private $modal,
            private needsWidthFixService,
            private exportGridToExcelService,
            private userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO) {
            this.$rootScope.page.title = "IT Kontrakt - Økonomi";

            this.$scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.loadGridOptions();

                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    $(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.mainGrid.element, true);
                    });
                }
            });

            //Defer until page change is complete
            setTimeout(() => this.activate(), 1);
        }
        public isValidUrl(Url) {
            var regexp = /(http || https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
            return regexp.test(Url.toLowerCase());
        };
        public opretITKontrakt() {

            var self = this;

            var modalInstance = this.$modal.open({
                windowClass: "modal fade in",
                templateUrl: "app/components/it-contract/it-contract-modal-create.view.html",
                controller: ["$scope", "$uibModalInstance", function ($scope, $modalInstance) {
                    $scope.formData = {};
                    $scope.type = "IT Kontrakt";
                    $scope.checkAvailbleUrl = "api/itProject/";

                    $scope.saveAndProceed = () => {

                        var orgId = self.user.currentOrganizationId;
                        var msg = self.notify.addInfoMessage("Opretter kontrakt...", false);

                        self.$http.post(`api/itcontract?organizationId=${self.user.currentOrganizationId}`, { organizationId: orgId, name: $scope.formData.name })
                            .then(function onSuccess(result: any) {
                                msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                var contract = result.data.response;
                                $modalInstance.close(contract.id);
                                self.$state.go("it-contract.edit.main", { id: contract.id });
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                            });
                    };

                    $scope.save = () => {

                        var orgId = self.user.currentOrganizationId;
                        var msg = self.notify.addInfoMessage("Opretter kontrakt...", false);

                        self.$http.post(`api/itcontract?organizationId=${self.user.currentOrganizationId}`, { organizationId: orgId, name: $scope.formData.name })
                            .then(function onSuccess(result: any) {
                                msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                var contract = result.data.response;
                                $modalInstance.close(contract.id);
                                self.$state.reload();
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                            });
                    };
                }]
            });
        }

        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // Resets the scrollbar position
        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.mainGrid);
        }

        // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(concat(concat(c/User/Name, ' '), c/User/LastName),'foo') and c/RoleId eq {roleId})"
        private fixRoleFilter(filterUrl, roleName, roleId) {
            var pattern = new RegExp(`(\\w+\\()${roleName}(.*?\\))`, "i");
            return filterUrl.replace(pattern, `Rights/any(c: $1concat(concat(c/User/Name, ' '), c/User/LastName)$2 and c/RoleId eq ${roleId})`);
        }

        private fixSystemFilter(filterUrl, column) {
            var pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
            return filterUrl.replace(pattern, "AssociatedSystemUsages/any(c: $1c/ItSystemUsage/ItSystem/Name$2)");
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            this.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            this.gridState.loadGridOptions(this.mainGrid);
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        public saveGridProfile() {
            Utility.KendoFilterProfileHelper.saveProfileLocalStorageData(this.$window, this.orgUnitStorageKey);
            this.gridState.saveGridProfile(this.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        public loadGridProfile() {
            this.gridState.loadGridProfile(this.mainGrid);
            Utility.KendoFilterProfileHelper.saveProfileSessionStorageData(this.$window, this.$, this.orgUnitStorageKey, "ResponsibleOrganizationUnit.Name");
            this.mainGrid.dataSource.read();
            this.notify.addSuccessMessage("Anvender gemte filtre og sortering");
        }

        public clearGridProfile() {
            this.$window.sessionStorage.removeItem(this.orgUnitStorageKey);
            this.gridState.removeProfile();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Filtre og sortering slettet");
            this.reload();
        }

        public doesGridProfileExist() {
            return this.gridState.doesGridProfileExist();
        }

        // clears grid filters by removing the localStorageItem and reloading the page
        public clearOptions() {
            this.$window.localStorage.removeItem(this.orgUnitStorageKey + "-profile");
            this.$window.sessionStorage.removeItem(this.orgUnitStorageKey);
            this.gridState.removeProfile();
            this.gridState.removeLocal();
            this.gridState.removeSession();
            this.notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
            // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
            this.reload();
        }


        public parseOptionEnum(enumName: string): string {
            switch (enumName) {
                case "YES":
                    return "Ja";
                case "NO":
                    return "Nej";
                default:
                    return "";
            }
        }

        private activate() {
            var self = this;
            var clonedItContractRoles = this._.cloneDeep(this.itContractRoles);
            this._.forEach(clonedItContractRoles, n => n.Id = `role${n.Id}`);
            clonedItContractRoles.push({ Id: "ContractSigner", Name: "Kontraktunderskriver" });
            this.roleSelectorDataSource = clonedItContractRoles;

            this.canCreate = this.userAccessRights.canCreate;

            var mainGridOptions: IKendoGridOptions<IItContractOverview> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                var urlParameters =
                                    `?$expand=Reference,Parent,ResponsibleOrganizationUnit,PaymentModel,PaymentFreqency,Rights($expand=User,Role),Supplier,AssociatedSystemUsages($expand=ItSystemUsage($expand=ItSystem)),TerminationDeadline,DataProcessingRegistrations($select=IsAgreementConcluded)`;
                                // if orgunit is set then the org unit filter is active
                                var orgUnitId = self.$window.sessionStorage.getItem(self.orgUnitStorageKey);
                                if (orgUnitId === null) {
                                    return `/odata/Organizations(${self.user.currentOrganizationId})/ItContracts` +
                                        urlParameters;
                                } else {
                                    return `/odata/Organizations(${self.user
                                        .currentOrganizationId})/OrganizationUnits(${orgUnitId})/ItContracts` +
                                        urlParameters;
                                }
                            },
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            // get kendo to map parameters to an odata url
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                self._.forEach(self.itContractRoles,
                                    role => parameterMap.$filter = self
                                        .fixRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id));

                                parameterMap.$filter = self
                                    .fixSystemFilter(parameterMap.$filter, "AssociatedSystemUsages");
                            }

                            return parameterMap;
                        }
                    },
                    sort: {
                        field: "Name",
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    schema: {
                        model: {
                            fields: {
                                OperationRemunerationBegun: { type: "date" },
                                LastChanged: { type: "date" },
                                Concluded: { type: "date" },
                                ExpirationDate: { type: "date" },
                                IrrevocableTo: { type: "date" },
                                Terminated: { type: "date" },
                                Acquisition: { type: "number" },
                                Operation: { type: "number" },
                                Other: { type: "number" },
                                IsActive: { type: "boolean" }
                            }
                        },
                        parse: response => {
                            // iterrate each contract
                            self._.forEach(response.value,
                                contract => {
                                    // HACK to add economy data to result
                                    var ecoData = <Array<any>>self._
                                        .filter(self.ecoStreamData, { "ExternPaymentForId": contract.Id });
                                    contract.Acquisition = self._.sumBy(ecoData, "Acquisition");
                                    contract.Operation = self._.sumBy(ecoData, "Operation");
                                    contract.Other = self._.sumBy(ecoData, "Other");

                                    var earliestAuditDate = self._
                                        .first(self._.sortBy(ecoData, ["AuditDate"], ["desc"]));
                                    if (earliestAuditDate && earliestAuditDate.AuditDate) {
                                        contract.AuditDate = earliestAuditDate.AuditDate;
                                    }

                                    var totalWhiteStatuses = self._.filter(ecoData, { AuditStatus: "White" }).length;
                                    var totalRedStatuses = self._.filter(ecoData, { AuditStatus: "Red" }).length;
                                    var totalYellowStatuses = self._.filter(ecoData, { AuditStatus: "Yellow" }).length;
                                    var totalGreenStatuses = self._.filter(ecoData, { AuditStatus: "Green" }).length;

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

                                    // HACK to flattens the Rights on usage so they can be displayed as single columns
                                    contract.roles = [];
                                    // iterrate each right
                                    self._.forEach(contract.Rights,
                                        right => {
                                            // init an role array to hold users assigned to this role
                                            if (!contract.roles[right.RoleId])
                                                contract.roles[right.RoleId] = [];

                                            // push username to the role array
                                            contract.roles[right.RoleId]
                                                .push([right.User.Name, right.User.LastName].join(" "));
                                        });
                                    if (!contract.Parent) { contract.Parent = { Name: "" }; }
                                    if (!contract.ResponsibleOrganizationUnit) { contract.ResponsibleOrganizationUnit = { Name: "" }; }
                                    if (!contract.Supplier) { contract.Supplier = { Name: "" }; }
                                    if (!contract.Reference) { contract.Reference = { Title: "", ExternalReferenceId: "" }; }
                                    if (!contract.PaymentModel) { contract.PaymentModel = { Name: "" }; }
                                    if (!contract.PaymentFreqency) { contract.PaymentFreqency = { Name: "" }; }
                                    if (!contract.Reference) { contract.Reference = { Title: "", ExternalReferenceId: "" }; }
                                });
                            return response;
                        }
                    }
                },
                toolbar: [
                    {
                        name: "opretITKontrakt",
                        text: "Opret IT Kontrakt",
                        template:
                            "<button ng-click='contractOverviewVm.opretITKontrakt()' data-element-type='createContractButton' class='btn btn-success pull-right' data-ng-disabled=\"!contractOverviewVm.canCreate\">#: text #</Button>"
                    },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template:
                            "<button type='button' data-element-type='resetFilterButton' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='contractOverviewVm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template:
                            "<button type='button' data-element-type='saveFilterButton' class='k-button k-button-icontext' title='Gem filtre og sortering' data-ng-click='contractOverviewVm.saveGridProfile()'>#: text #</button>"
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template:
                            "<button type='button' data-element-type='useFilterButton' class='k-button k-button-icontext' title='Anvend gemte filtre og sortering' data-ng-click='contractOverviewVm.loadGridProfile()' data-ng-disabled='!contractOverviewVm.doesGridProfileExist()'>#: text #</button>"
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template:
                            "<button type='button' data-element-type='removeFilterButton' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='contractOverviewVm.clearGridProfile()' data-ng-disabled='!contractOverviewVm.doesGridProfileExist()'>#: text #</button>"
                    },
                    {
                        template: kendo.template(self.$("#role-selector").html())
                    }
                ],
                excel: {
                    fileName: "IT Kontrakt Overblik.xlsx",
                    filterable: true,
                    allPages: true
                },
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200, "all"],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single"
                },
                reorderable: true,
                resizable: true,
                filterable: {
                    mode: "row"
                },
                groupable: false,
                columnMenu: true,
                height: window.innerHeight - 200,
                dataBound: self.saveGridOptions,
                columnResize: self.saveGridOptions,
                columnHide: self.saveGridOptions,
                columnShow: self.saveGridOptions,
                columnReorder: self.saveGridOptions,
                excelExport: self.exportToExcel,
                page: self.onPaging,
                columns: [
                    {
                        field: "IsActive", title: "Gyldig/Ikke gyldig", width: 150,
                        persistId: "isActive", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.IsActive) {
                                return '<span class="fa fa-file text-success" aria-hidden="true"></span>';
                            }
                            return '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>';
                        },
                        excelTemplate: dataItem => {
                            var isActive = false;
                            if (dataItem) {
                                isActive = dataItem.IsActive;
                            }
                            return isActive.toString();
                        },
                        attributes: { "class": "text-center" },
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "ItContractId", title: "KontraktID", width: 150,
                        persistId: "contractid", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.ItContractId || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Parent.Name", title: "Overordnet kontrakt", width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Parent ? `<a data-ui-sref="it-contract.edit.main({id:${dataItem.Parent.Id}})">${dataItem.Parent.Name}</a>` : "",
                        excelTemplate: dataItem => dataItem && dataItem.Parent && dataItem.Parent.Name || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "IT Kontrakt", width: 260,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: dataItem => `<a data-ui-sref='it-contract.edit.main({id: ${dataItem.Id}})'>${dataItem.Name}</a>`,
                        attributes: {
                            "data-element-type": "contractNameObject"
                        },
                        headerAttributes: {
                            "data-element-type": "contractNameHeader"
                        },
                        excelTemplate: dataItem => dataItem && dataItem.Name || "",
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "ResponsibleOrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 245,
                        persistId: "orgunit", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.ResponsibleOrganizationUnit ? dataItem.ResponsibleOrganizationUnit.Name : "",
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: self.orgUnitDropDownList
                            }
                        }
                    },
                    {
                        field: "AssociatedSystemUsages", title: "IT System", width: 150,
                        persistId: "itsys", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var value = "";
                            if (dataItem.AssociatedSystemUsages.length > 0) {
                                const system = self._.first(dataItem.AssociatedSystemUsages).ItSystemUsage.ItSystem;
                                value = Helpers.SystemNameFormat.apply(system.Name, system.Disabled);
                            }

                            if (dataItem.AssociatedSystemUsages.length > 1) {
                                value += ` (${dataItem.AssociatedSystemUsages.length})`;
                            }

                            return value;
                        },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "Supplier.Name", title: "Leverandør", width: 150,
                        persistId: "suppliername", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Supplier ? dataItem.Supplier.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Reference.Title", title: "Reference", width: 150,
                        persistId: "ReferenceId", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var reference = dataItem.Reference;
                            return Helpers.RenderFieldsHelper.renderReferenceUrl(reference);
                        },
                        excelTemplate: dataItem => {
                            return Helpers.ExcelExportHelper.renderReferenceUrl(dataItem.Reference);
                        },
                        attributes: { "class": "text-left" },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Reference.ExternalReferenceId", title: "Dokument ID / Sagsnr.", width: 150,
                        persistId: "folderref", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            return Helpers.RenderFieldsHelper.renderExternalReferenceId(dataItem.Reference);
                        },
                        excelTemplate: dataItem => {
                            return Helpers.ExcelExportHelper.renderExternalReferenceId(dataItem.Reference);
                        },
                        attributes: { "class": "text-center" },
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Acquisition", title: "Anskaffelse", width: 90,
                        persistId: "acquisition", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.Acquisition.toString() || "",
                        attributes: { "class": "text-right" },
                        format: "{0:n0}",
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "Operation", title: "Drift/År", width: 75,
                        persistId: "operation", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.Operation.toString() || "",
                        attributes: { "class": "text-right" },
                        format: "{0:n0}",
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "Other", title: "Andet", width: 150,
                        persistId: "other", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.Other.toString() || "",
                        attributes: { "class": "text-right" },
                        format: "{0:n0}",
                        hidden: true,
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "DataProcessingRegistrationsConcluded", title: "Databehandleraftale", width: 150,
                        persistId: "dataProcessingRegistrationsConcluded",
                        template: dataItem => {
                            if (dataItem.DataProcessingRegistrations && dataItem.DataProcessingRegistrations.length > 0) {
                                const choicesToRender = dataItem
                                    .DataProcessingRegistrations
                                    .filter(registration => registration.IsAgreementConcluded !== null &&
                                        registration.IsAgreementConcluded !==
                                        Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED);
                                if (choicesToRender.length > 0) {
                                    return choicesToRender
                                        .map(dpr => Models.ViewModel.Shared.YesNoIrrelevantOptions.getText(dpr.IsAgreementConcluded))
                                        .reduce((combined: string, next: string, _) => combined.length === 0 ? next : `${combined}, ${next}`, "");
                                }
                            }
                            return "";
                        },
                        attributes: { "class": "text-left" },
                        hidden: true,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "OperationRemunerationBegun", title: "Driftsvederlag påbegyndt", format: "{0:dd-MM-yyyy}", width: 150,
                        persistId: "opremun", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => {
                            if (!dataItem || !dataItem.OperationRemunerationBegun) {
                                return "";
                            }

                            return self.moment(dataItem.OperationRemunerationBegun).format(Constants.DateFormat.DanishDateFormat);
                        },
                        hidden: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                operator: "gte"
                            }
                        }
                    },
                    {
                        field: "PaymentModel.Name", title: "Betalingsmodel", width: 150,
                        persistId: "paymodel", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.PaymentModel ? dataItem.PaymentModel.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "PaymentFreqency.Name", title: "Betalingsfrekvens", width: 150,
                        persistId: "payfreq", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.PaymentFreqency ? dataItem.PaymentFreqency.Name : "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "AuditDate", title: "Audit dato", width: 90,
                        persistId: "auditdate", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (!dataItem.AuditDate) {
                                return "";
                            }

                            return self.moment(dataItem.AuditDate).format(Constants.DateFormat.DanishDateFormat);
                        },
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "AuditStatus", title: "Audit status", width: 90,
                        persistId: "auditstatus", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.status.max > 0) {
                                var str = JSON.stringify(dataItem.status);
                                return `<div data-show-status='${str}'></div>`;
                            }
                            return "";
                        },
                        excelTemplate: dataItem =>
                            dataItem && dataItem.status && `Hvid: ${dataItem.status.white}, Rød: ${dataItem.status.red}, Gul: ${dataItem.status.yellow}, Grøn: ${dataItem.status.green}, Max: ${dataItem.status.max}` || "",
                        sortable: false,
                        filterable: false
                    }
                ]
            };
            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ''
                });
            }
            // find the index of column where the role columns should be inserted
            var insertIndex = this._.findIndex(mainGridOptions.columns, { 'persistId': "orgunit" }) + 1;

            // add special contract signer role
            var signerRole = {
                field: "ContractSigner",
                title: "Kontraktunderskriver",
                persistId: "roleSigner",
                template: dataItem => dataItem.ContractSigner ? `${dataItem.ContractSigner}` : "",
                width: 130,
                hidden: true,
                sortable: true,
                filterable: {
                    cell: {
                        dataSource: [],
                        showOperators: false,
                        operator: "contains"
                    }
                }
            };
            mainGridOptions.columns.splice(insertIndex, 0, signerRole);

            // add a role column for each of the roles
            // note iterating in reverse so we don't have to update the insert index
            this._.forEachRight(this.itContractRoles, role => {
                var roleColumn: IKendoGridColumn<IItContractOverview> = {
                    field: `role${role.Id}`,
                    title: role.Name,
                    persistId: `role${role.Id}`,
                    template: dataItem => {
                        var roles = "";

                        if (dataItem.roles[role.Id] === undefined)
                            return roles;

                        roles = self.concatRoles(dataItem.roles[role.Id]);

                        var link = `<a data-ui-sref='it-contract.edit.roles({id: ${dataItem.Id}})'>${roles}</a>`;

                        return link;
                    },
                    excelTemplate: dataItem => {
                        var roles = "";

                        if (!dataItem || dataItem.roles[role.Id] === undefined)
                            return roles;

                        return self.concatRoles(dataItem.roles[role.Id]);
                    },
                    width: 200,
                    hidden: !(role.Name === "Kontraktejer"), // hardcoded role name :(
                    sortable: false,
                    filterable: {
                        cell: {
                            dataSource: [],
                            showOperators: false,
                            operator: "contains"
                        }
                    }
                };

                // insert the generated column at the correct location
                mainGridOptions.columns.splice(insertIndex, 0, roleColumn);
            });

            // assign the generated grid options to the scope value, kendo will do the rest
            this.mainGridOptions = mainGridOptions;
        }

        private exportToExcel = (e: IKendoGridExcelExportEvent<IItContractOverview>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
        }

        private orgUnitDropDownList = (args) => {
            var self = this;

            function indent(dataItem: any) {
                var htmlSpace = "&nbsp;&nbsp;&nbsp;&nbsp;";
                return htmlSpace.repeat(dataItem.$level) + dataItem.Name;
            }

            function setDefaultOrgUnit() {
                var kendoElem = this;
                var idTofind = self.$window.sessionStorage.getItem(self.orgUnitStorageKey);

                if (!idTofind) {
                    // if no id was found then do nothing
                    return;
                }

                // find the index of the org unit that matches the users default org unit
                var index = self._.findIndex(kendoElem.dataItems(), (item: any) => (item.Id == idTofind));

                // -1 = no match
                //  0 = root org unit, which should display all. So remove org unit filter
                if (index > 0) {
                    // select the users default org unit
                    kendoElem.select(index);
                }
            }

            function orgUnitChanged() {
                var kendoElem = this;
                // can't use args.dataSource directly,
                // if we do then the view doesn't update.
                // So have to go through $scope - sadly :(
                var dataSource = self.mainGrid.dataSource;
                var selectedIndex = kendoElem.select();
                var selectedId = self._.parseInt(kendoElem.value());

                if (selectedIndex > 0) {
                    // filter by selected
                    self.$window.sessionStorage.setItem(self.orgUnitStorageKey, selectedId.toString());
                } else {
                    // else clear filter because the 0th element should act like a placeholder
                    self.$window.sessionStorage.removeItem(self.orgUnitStorageKey);
                }
                // setting the above session value will cause the grid to fetch from a different URL
                // see the function part of this http://docs.telerik.com/kendo-ui/api/javascript/data/datasource#configuration-transport.read.url
                // so that's why it works
                dataSource.read();
            }

            // http://dojo.telerik.com/ODuDe/5
            args.element.removeAttr("data-bind");
            args.element.kendoDropDownList({
                dataSource: this.orgUnits,
                dataValueField: "Id",
                dataTextField: "Name",
                template: indent,
                dataBound: setDefaultOrgUnit,
                change: orgUnitChanged
            });
        }

        public roleSelectorOptions = (): kendo.ui.DropDownListOptions => {
            return {
                autoBind: false,
                dataSource: this.roleSelectorDataSource,
                dataTextField: "Name",
                dataValueField: "Id",
                optionLabel: "Vælg kontraktrolle...",
                change: e => {
                    // hide all roles column
                    this.mainGrid.hideColumn("ContractSigner");
                    this._.forEach(this.itContractRoles, role => this.mainGrid.hideColumn(`role${role.Id}`));

                    var selectedId = e.sender.value();
                    // show only the selected role column
                    this.mainGrid.showColumn(selectedId);
                    this.needsWidthFixService.fixWidth();
                }
            }
        };

        private concatRoles(roles: Array<any>): string {
            var concatRoles = "";

            // join the first 5 username together
            if (roles.length > 0) {
                concatRoles = roles.slice(0, 4).join(", ");
            }

            // if more than 5 then add an elipsis
            if (roles.length > 5) {
                concatRoles += ", ...";
            }

            return concatRoles;
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
                        // TODO this isn't a sustainable solution - but a workaround for now...
                        ecoStreamData: [
                            "$http", "user", "notify", ($http, user, notify) =>
                                $http.get(`/odata/ExternEconomyStreams(Organization=${user.currentOrganizationId})`)
                                    .then(result => result.data.value, () => $stateProvider.transitionTo("home", { q: "updated search term" }))
                        ]
                    }
                });
        }
        ]);
}

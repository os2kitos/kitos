module Kitos.ItContract.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: IKendoGrid<IItContractOverview>;
        mainGridOptions: kendo.ui.GridOptions;
        roleSelectorOptions: kendo.ui.DropDownListOptions;

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
        ContainsDataHandlerAgreement: string;
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
        private gridState = this.gridStateService.getService(this.storageKey);
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
            "needsWidthFixService"
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
            private needsWidthFixService) {
            this.$rootScope.page.title = "IT Kontrakt - Økonomi";

            this.$scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.loadGridOptions();
                    this.mainGrid.dataSource.read();

                    // show loadingbar when export to excel is clicked
                    // hidden again in method exportToExcel callback
                    $(".k-grid-excel").click(() => {
                        kendo.ui.progress(this.mainGrid.element, true);
                    });
                }
            });

            //this.needsWidthFixService.fixWidthOnClick();

            this.activate();
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

                        self.$http.post("api/itcontract", { organizationId: orgId, name: $scope.formData.name })
                            .success((result: any) => {
                                msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                var contract = result.response;
                                $modalInstance.close(contract.id);
                                self.$state.go("it-contract.edit.main", { id: contract.id });
                            })
                            .error(() => {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                            });
                    };

                    $scope.save = () => {

                        var orgId = self.user.currentOrganizationId;
                        var msg = self.notify.addInfoMessage("Opretter kontrakt...", false);

                        self.$http.post("api/itcontract", { organizationId: orgId, name: $scope.formData.name })
                            .success((result: any) => {
                                msg.toSuccessMessage("En ny kontrakt er oprettet!");
                                var contract = result.response;
                                $modalInstance.close(contract.id);
                                self.$state.reload();
                            })
                            .error(() => {
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
            //Add only excel option if user is not readonly
            if (!this.user.isReadOnly) {
                this.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            }
            this.gridState.loadGridOptions(this.mainGrid);
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        public saveGridProfile() {
            // the stored org unit id must be the current
            var currentOrgUnitId = this.$window.sessionStorage.getItem(this.orgUnitStorageKey);
            this.$window.localStorage.setItem(this.orgUnitStorageKey + "-profile", currentOrgUnitId);

            this.gridState.saveGridProfile(this.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        public loadGridProfile() {
            this.gridState.loadGridProfile(this.mainGrid);

            var orgUnitId = this.$window.localStorage.getItem(this.orgUnitStorageKey + "-profile");
            // update session
            this.$window.sessionStorage.setItem(this.orgUnitStorageKey, orgUnitId);
            // find the org unit filter row section
            var orgUnitFilterRow = this.$(".k-filter-row[data-field='ResponsibleOrganizationUnit.Name']");
            // find the access modifier kendo widget
            var orgUnitFilterWidget = orgUnitFilterRow.find("input").data("kendoDropDownList");
            orgUnitFilterWidget.select(dataItem => (dataItem.Id == orgUnitId));

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
                case "DONTKNOW":
                    return "Ved ikke";
                default:
                    return "";
            }
        }      

        private activate() {

            var clonedItContractRoles = this._.cloneDeep(this.itContractRoles);
            this._.forEach(clonedItContractRoles, n => n.Id = `role${n.Id}`);
            clonedItContractRoles.push({ Id: "ContractSigner", Name: "Kontraktunderskriver" });
            this.roleSelectorDataSource = clonedItContractRoles;

            this.canCreate = !this.user.isReadOnly;

            var mainGridOptions: IKendoGridOptions<IItContractOverview> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                var urlParameters =
                                    `?$expand=Reference,Parent,ResponsibleOrganizationUnit,PaymentModel,PaymentFreqency,Rights($expand=User,Role),Supplier,AssociatedSystemUsages($expand=ItSystemUsage($expand=ItSystem)),TerminationDeadline`;
                                // if orgunit is set then the org unit filter is active
                                var orgUnitId = this.$window.sessionStorage.getItem(this.orgUnitStorageKey);
                                if (orgUnitId === null) {
                                    return `/odata/Organizations(${this.user.currentOrganizationId})/ItContracts` +
                                        urlParameters;
                                } else {
                                    return `/odata/Organizations(${this.user
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
                                this._.forEach(this.itContractRoles,
                                    role => parameterMap.$filter = this
                                        .fixRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id));

                                parameterMap.$filter = this
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
                            this._.forEach(response.value,
                                contract => {
                                    // HACK to add economy data to result
                                    var ecoData = <Array<any>>this._
                                        .filter(this.ecoStreamData, { "ExternPaymentForId": contract.Id });
                                    contract.Acquisition = this._.sumBy(ecoData, "Acquisition");
                                    contract.Operation = this._.sumBy(ecoData, "Operation");
                                    contract.Other = this._.sumBy(ecoData, "Other");

                                    var earliestAuditDate = this._
                                        .first(this._.sortBy(ecoData, ["AuditDate"], ["desc"]));
                                    if (earliestAuditDate && earliestAuditDate.AuditDate) {
                                        contract.AuditDate = earliestAuditDate.AuditDate;
                                    }

                                    var totalWhiteStatuses = this._.filter(ecoData, { AuditStatus: "White" }).length;
                                    var totalRedStatuses = this._.filter(ecoData, { AuditStatus: "Red" }).length;
                                    var totalYellowStatuses = this._.filter(ecoData, { AuditStatus: "Yellow" }).length;
                                    var totalGreenStatuses = this._.filter(ecoData, { AuditStatus: "Green" }).length;

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
                                    this._.forEach(contract.Rights,
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
                                });
                            return response;
                        }
                    }
                },
                toolbar: [
                    {
                        //TODO ng-show='hasWriteAccess'
                        name: "opretITKontrakt",
                        text: "Opret IT Kontrakt",
                        template:
                        "<button ng-click='contractOverviewVm.opretITKontrakt()' class='btn btn-success pull-right' data-ng-disabled=\"!contractOverviewVm.canCreate\">#: text #</Button>"
                    },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template:
                        "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='contractOverviewVm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template:
                        '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="contractOverviewVm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template:
                        '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="contractOverviewVm.loadGridProfile()" data-ng-disabled="!contractOverviewVm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template:
                        "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='contractOverviewVm.clearGridProfile()' data-ng-disabled='!contractOverviewVm.doesGridProfileExist()'>#: text #</button>"
                    },
                    {
                        template: kendo.template(this.$("#role-selector").html())
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
                height: 750,
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
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
                                isActive = this.isContractActive(dataItem);
                            }
                            return isActive.toString();
                        },
                        attributes: { "class": "text-center" },
                        sortable: false,
                        filterable: false
                        //{
                        //    cell: {
                        //        template: args => {
                        //            args.element.kendoDropDownList({
                        //                dataSource: [ { type: "Gyldig", value: true }, { type: "Ikke gyldig", value: false } ],
                        //                dataTextField: "type",
                        //                dataValueField: "value",
                        //                valuePrimitive: true
                        //            });
                        //        },
                        //        showOperators: false
                           
                        //    }
                        //}
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
                                template: this.orgUnitDropDownList
                            }
                        }
                    },
                    {
                        field: "AssociatedSystemUsages", title: "IT System", width: 150,
                        persistId: "itsys", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var value = "";
                            if (dataItem.AssociatedSystemUsages.length > 0) {
                                if (this._.first(dataItem.AssociatedSystemUsages).ItSystemUsage.ItSystem.Disabled)
                                    value = this._.first(dataItem.AssociatedSystemUsages).ItSystemUsage.ItSystem.Name + " (Slettes)";
                                else
                                    value = this._.first(dataItem.AssociatedSystemUsages).ItSystemUsage.ItSystem.Name;
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
                            if (reference != null) {
                                if (reference.URL) {
                                    return "<a target=\"_blank\" style=\"float:left;\" href=\"" + reference.URL + "\">" + reference.Title + "</a>";
                                } else {
                                    return reference.Title;
                                }
                            }
                            return "";
                        },
                        attributes: { "class": "text-center" },
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
                        // TODO Skal muligvis slettes
                        field: "Reference.ExternalReferenceId", title: "Mappe ref", width: 150,
                        persistId: "folderref", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var reference = dataItem.Reference;
                            if (reference != null) {
                                if (reference.URL) {
                                    return "<a target=\"_blank\" style=\"float:left;\" href=\"" +
                                        reference.ExternalReferenceId +
                                        "\">" +
                                        reference.Title + 
                                        "</a>";
                                } else {
                                    return reference.Title;
                                }
                            }
                            return "";
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
                        field: "ContainsDataHandlerAgreement", title: "Databehandleraftale", width: 150,
                        persistId: "ContainsDataHandlerAgreement", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.ContainsDataHandlerAgreement || "",
                        template: dataItem => this.parseOptionEnum(dataItem.ContainsDataHandlerAgreement),
                        attributes: { "class": "text-right" },
                        hidden: true,
                        sortable: false,
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "Ja", value: "YES" }, { type: "Nej", value: "NO" }, { type: "Ved ikke", value: "DONTKNOW" }],
                                        dataTextField: "type",
                                        dataValueField: "value",
                                        valuePrimitive: true
                                    });
                                },
                                showOperators: false
                            }
                        }
                    },
                    {
                        field: "OperationRemunerationBegun", title: "Driftsvederlag påbegyndt", format: "{0:dd-MM-yyyy}", width: 150,
                        persistId: "opremun", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => {
                            if (!dataItem || !dataItem.OperationRemunerationBegun) {
                                return "";
                            }

                            return this.moment(dataItem.OperationRemunerationBegun).format("DD-MM-YYYY");
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

                            return this.moment(dataItem.AuditDate).format("DD-MM-YYYY");
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

                        roles = this.concatRoles(dataItem.roles[role.Id]);

                        var link = `<a data-ui-sref='it-contract.edit.roles({id: ${dataItem.Id}})'>${roles}</a>`;

                        return link;
                    },
                    excelTemplate: dataItem => {
                        var roles = "";

                        if (!dataItem || dataItem.roles[role.Id] === undefined)
                            return roles;

                        return this.concatRoles(dataItem.roles[role.Id]);
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

        //private nextAdviceTemplate(dataItem) {
        //    if (dataItem.Advices.length > 0)
        //        return this.moment(this._.chain(dataItem.Advices).sortBy("AlarmDate").first().AlarmDate).format("DD-MM-YYYY");
        //    return "";
        //}

        private isContractActive(dataItem) {


            if (!dataItem.Active) {
                var today = this.moment().startOf('day');
                var startDate = dataItem.Concluded ? this.moment(dataItem.Concluded).startOf('day') : today;
                var endDate = dataItem.ExpirationDate ? this.moment(dataItem.ExpirationDate).startOf('day') : this.moment("9999-12-30").startOf('day');

                if (dataItem.Terminated) {
                    var terminationDate = this.moment(dataItem.Terminated);
                    if (dataItem.TerminationDeadline) {
                        terminationDate.add(dataItem.TerminationDeadline.Name, "months");
                    }
                    return today >= startDate && today <= terminationDate;
                }
                return today >= startDate && today <= endDate;
            }
            return dataItem.Active;
        }

        private exportFlag = false;
        private exportToExcel = (e: IKendoGridExcelExportEvent<IItContractOverview>) => {
            var columns = e.sender.columns;

            if (!this.exportFlag) {
                e.preventDefault();
                this._.forEach(columns, column => {
                    if (column.hidden) {
                        column.tempVisual = true;
                        e.sender.showColumn(column);
                    }
                });
                this.$timeout(() => {
                    this.exportFlag = true;
                    e.sender.saveAsExcel();
                });
            } else {
                this.exportFlag = false;

                // hide coloumns on visual grid
                this._.forEach(columns, column => {
                    if (column.tempVisual) {
                        delete column.tempVisual;
                        e.sender.hideColumn(column);
                    }
                });

                // render templates
                var sheet = e.workbook.sheets[0];

                // skip header row
                for (var rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
                    var row = sheet.rows[rowIndex];

                    // -1 as sheet has header and dataSource doesn't
                    var dataItem = e.data[rowIndex - 1];

                    for (var columnIndex = 0; columnIndex < row.cells.length; columnIndex++) {
                        if (columns[columnIndex].field === "") continue;
                        var cell = row.cells[columnIndex];

                        var template = this.getTemplateMethod(columns[columnIndex]);

                        cell.value = template(dataItem);
                    }
                }

                // hide loadingbar when export is finished
                kendo.ui.progress(this.mainGrid.element, false);
                this.needsWidthFixService.fixWidth();
            }
        }

        private getTemplateMethod(column) {
            var template: Function;

            if (column.excelTemplate) {
                template = column.excelTemplate;
            } else if (typeof column.template === "function") {
                template = <Function>column.template;
            } else if (typeof column.template === "string") {
                template = kendo.template(<string>column.template);
            } else {
                template = t => t;
            }

            return template;
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
                            "$http", $http => $http.get("/odata/LocalItContractRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                        ],
                        user: [
                            "userService", userService => userService.getUser()
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

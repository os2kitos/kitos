﻿module Kitos.ItContract.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: Kitos.IKendoGrid;
        mainGridOptions: kendo.ui.GridOptions;
        roleSelectorOptions: kendo.ui.DropDownListOptions;

        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): void;
        clearOptions(): void;
    }

    export class OverviewController implements  IOverviewController {
        private storageKey = "it-contract-overview-options";
        private orgUnitStorageKey = "it-contract-overview-orgunit";
        private gridState = this.gridStateService.getService(this.storageKey);
        private roleSelectorDataSource = this._.clone(this.itContractRoles);
        public mainGrid: Kitos.IKendoGrid;
        public mainGridOptions: kendo.ui.GridOptions;

        private static $inject: Array<string> = [
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
            "ecoStreamData"
        ];

        constructor(
            private $rootScope: Kitos.IRootScope,
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $window: ng.IWindowService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: Kitos.ILodashWithMixins,
            private moment: moment.MomentStatic,
            private notify,
            private user,
            private gridStateService: Kitos.Services.IGridStateFactory,
            private itContractRoles,
            private orgUnits,
            private ecoStreamData) {
            this.$rootScope.page.title = "IT Kontrakt - Økonomi";

            this.$scope.$on("kendoWidgetCreated", (event, widget) => {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === this.mainGrid) {
                    this.loadGridOptions();
                    this.mainGrid.dataSource.read();
                }
            });

            this.activate();
        }

        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(c/User/Name,'foo') and c/RoleId eq {roleId})"
        private fixRoleFilter(filterUrl, roleName, roleId) {
            var pattern = new RegExp(`(\\w+\\()${roleName}(.*?\\))`, "i");
            return filterUrl.replace(pattern, `Rights/any(c: $1c/User/Name$2 and c/RoleId eq ${roleId})`);
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            var selectedOrgUnitId = <number> this.$window.sessionStorage.getItem(this.orgUnitStorageKey);
            var selectedOrgUnit: any = this._.find(this.orgUnits, (orgUnit: any) => (orgUnit.Id == selectedOrgUnitId));

            var filter = undefined;
            // if selected is a root then no need to filter as it should display everything anyway
            if (selectedOrgUnit && selectedOrgUnit.$level != 0) {
                filter = this.getFilterWithOrgUnit({}, selectedOrgUnitId, selectedOrgUnit.childIds);
            }

            this.gridState.loadGridOptions(this.mainGrid, filter);
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        public saveGridProfile () {
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
            var orgUnitFilterRow = this.$(".k-filter-row [data-field='ResponsibleOrganizationUnit.Name']");
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

        private activate() {
            var clonedItContractRoles = this._.cloneDeep(this.itContractRoles);
            this._.forEach(clonedItContractRoles, n => n.Id = `role${n.Id}`);
            clonedItContractRoles.push({ Id: "ContractSigner.Name", Name: "Kontraktunderskriver" });
            this.roleSelectorDataSource = clonedItContractRoles;

            var mainGridOptions: Kitos.IKendoGridOptions = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/odata/Organizations(${this.user.currentOrganizationId})/ItContracts?$expand=Parent,ResponsibleOrganizationUnit,PaymentModel,PaymentFreqency,Rights($expand=User,Role),Supplier,AssociatedSystemUsages($expand=ItSystemUsage($expand=ItSystem)),TerminationDeadline,ContractSigner`,
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            // get kendo to map parameters to an odata url
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                this._.forEach(this.itContractRoles, role => parameterMap.$filter = this.fixRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id));
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
                                Other: { type: "number" }
                            }
                        },
                        parse: response => {
                            // iterrate each contract
                            this._.forEach(response.value, contract => {
                                // HACK to add economy data to result
                                var ecoData = <Array<any>> this._.where(this.ecoStreamData, { "ExternPaymentForId": contract.Id });
                                contract.Acquisition = this._.sum(ecoData, "Acquisition");
                                contract.Operation = this._.sum(ecoData, "Operation");
                                contract.Other = this._.sum(ecoData, "Other");
                                var earliestAuditDate = this._.first(this._.sortByOrder(ecoData, ["AuditDate"], ["desc"]));
                                if (earliestAuditDate) {
                                    if (earliestAuditDate.AuditDate) {
                                        contract.AuditDate = this.moment(earliestAuditDate.AuditDate).format("DD-MM-YYYY");
                                    }
                                }

                                var totalWhiteStatuses = this._.where(ecoData, { AuditStatus: "White" }).length;
                                var totalRedStatuses = this._.where(ecoData, { AuditStatus: "Red" }).length;
                                var totalYellowStatuses = this._.where(ecoData, { AuditStatus: "Yellow" }).length;
                                var totalGreenStatuses = this._.where(ecoData, { AuditStatus: "Green" }).length;

                                contract.status = {
                                    max: totalWhiteStatuses + totalRedStatuses + totalYellowStatuses + totalGreenStatuses,
                                    white: totalWhiteStatuses,
                                    red: totalRedStatuses,
                                    yellow: totalYellowStatuses,
                                    green: totalGreenStatuses
                                };

                                // HACK to flattens the Rights on usage so they can be displayed as single columns
                                contract.roles = [];
                                // iterrate each right
                                this._.forEach(contract.Rights, right => {
                                    // init an role array to hold users assigned to this role
                                    if (!contract.roles[right.RoleId])
                                        contract.roles[right.RoleId] = [];

                                    // push username to the role array
                                    contract.roles[right.RoleId].push([right.User.Name, right.User.LastName].join(" "));
                                });
                            });
                            return response;
                        }
                    }
                },
                toolbar: [
                    { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='contractOverviewVm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="contractOverviewVm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="contractOverviewVm.loadGridProfile()" data-ng-disabled="!contractOverviewVm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='contractOverviewVm.clearGridProfile()' data-ng-disabled='!contractOverviewVm.doesGridProfileExist()'>#: text #</button>"
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
                    pageSizes: [10, 25, 50, 100, 200],
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
                columnMenu: {
                    filterable: false
                },
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                columns: [
                    {
                        field: "", title: "Aktiv", width: 50,
                        persistId: "active", // DON'T YOU DARE RENAME!
                        template: this.activeStatusTemplate,
                        attributes: { "class": "text-center" },
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "ItContractId", title: "KontraktID", width: 150,
                        persistId: "contractid", // DON'T YOU DARE RENAME!
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Parent.Name", title: "Overordnet kontrakt", width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: "#= Parent ? '<a data-ui-sref=\"it-contract.edit.systems({id:' + Parent.Id + '})\">' + Parent.Name + '</a>' : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "IT Kontrakt", width: 260,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: "<a data-ui-sref='it-contract.edit.systems({id: #: Id #})'>#: Name #</a>",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "ResponsibleOrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 245,
                        persistId: "orgunit", // DON'T YOU DARE RENAME!
                        template: "#: ResponsibleOrganizationUnit ? ResponsibleOrganizationUnit.Name : '' #",
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
                        template: "#: AssociatedSystemUsages.length > 0 ? _.first(AssociatedSystemUsages).ItSystemUsage.ItSystem.Name : '' #" +
                                    "#= AssociatedSystemUsages.length > 1 ? ' (' + AssociatedSystemUsages.length + ')' : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Supplier.Name", title: "Leverandør", width: 150,
                        persistId: "suppliername", // DON'T YOU DARE RENAME!
                        template: "#: Supplier ? Supplier.Name : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Esdh", title: "ESDH ref", width: 150,
                        persistId: "esdh", // DON'T YOU DARE RENAME!
                        template: "#= Esdh ? '<a target=\"_blank\" href=\"' + Esdh + '\"><i class=\"fa fa-link\"></a>' : '' #",
                        attributes: { "class": "text-center" },
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Folder", title: "Mappe ref", width: 150,
                        persistId: "folderref", // DON'T YOU DARE RENAME!
                        template: "#= Folder ? '<a target=\"_blank\" href=\"' + Folder + '\"><i class=\"fa fa-link\"></i></a>' : '' #",
                        attributes: { "class": "text-center" },
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "Acquisition", title: "Anskaffelse", width: 90,
                        persistId: "acquisition", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-right" },
                        format: "{0:n0}",
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "Operation", title: "Drift/År", width: 75,
                        persistId: "operation", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-right" },
                        format: "{0:n0}",
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "Other", title: "Andet", width: 150,
                        persistId: "other", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-right" },
                        format: "{0:n0}",
                        hidden: true,
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "OperationRemunerationBegun", title: "Driftsvederlag påbegyndt", format: "{0:dd-MM-yyyy}", width: 150,
                        persistId: "opremun", // DON'T YOU DARE RENAME!
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
                        template: "#: PaymentModel ? PaymentModel.Name : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "PaymentFreqency.Name", title: "Betalingsfrekvens", width: 150,
                        persistId: "payfreq", // DON'T YOU DARE RENAME!
                        template: "#: PaymentFreqency ? PaymentFreqency.Name : '' #",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "AuditDate", title: "Audit dato", width: 90,
                        persistId: "auditdate", // DON'T YOU DARE RENAME!
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "", title: "Audit status", width: 90,
                        persistId: "auditstatus", // DON'T YOU DARE RENAME!
                        template: this.audioStatusTemplate,
                        sortable: false,
                        filterable: false,
                    },
                    //{
                    //    field: "Advices.AlarmDate", title: "Dato for næste advis", width: 150,
                    //    persistId: "nextadvis", // DON'T YOU DARE RENAME!
                    //    template: nextAdviceTemplate,
                    //    sortable: false,
                    //    filterable: false,
                    //},
                ]
            };

            // find the index of column where the role columns should be inserted
            var insertIndex = this._.findIndex(mainGridOptions.columns, "persistId", "orgunit") + 1;

            // add special contract signer role
            var signerRole = {
                field: "ContractSigner.Name",
                title: "Kontraktunderskriver",
                persistId: "roleSigner",
                template: "#: ContractSigner ? ContractSigner.Name + ' ' + ContractSigner.LastName : '' #",
                width: 200,
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
                var roleColumn = {
                    field: `role${role.Id}`,
                    title: role.Name,
                    persistId: `role${role.Id}`,
                    template: dataItem => this.roleTemplate(dataItem, role.Id),
                    width: 200,
                    hidden: role.Name == "Kontraktejer" ? false : true, // hardcoded role name :(
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

        private roleTemplate(dataItem, roleId) {
            var roles = "";

            if (dataItem.roles[roleId] === undefined)
                return roles;

            // join the first 5 username together
            if (dataItem.roles[roleId].length > 0)
                roles = dataItem.roles[roleId].slice(0, 4).join(", ");

            // if more than 5 then add an elipsis
            if (dataItem.roles[roleId].length > 5)
                roles += ", ...";

            var link = `<a data-ui-sref='it-contract.edit.roles({id: ${dataItem.Id}})'>${roles}</a>`;

            return link;
        }

        //private nextAdviceTemplate(dataItem) {
        //    if (dataItem.Advices.length > 0)
        //        return this.moment(this._.chain(dataItem.Advices).sortBy("AlarmDate").first().AlarmDate).format("DD-MM-YYYY");
        //    return "";
        //}

        private audioStatusTemplate(dataItem) {
            if (dataItem.status.max > 0) {
                var str = JSON.stringify(dataItem.status);
                return `<div data-show-status='${str}'></div>`;
            }
            return "";
        }

        private activeStatusTemplate = (dataItem) => {
            var isActive = this.isContractActive(dataItem);

            if (isActive)
                return '<span class="fa fa-file text-success" aria-hidden="true"></span>';
            return '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>';
        }

        private isContractActive(dataItem) {
            var today = this.moment();
            var startDate = dataItem.Concluded ? this.moment(dataItem.Concluded) : today;
            var endDate = dataItem.ExpirationDate ? this.moment(dataItem.ExpirationDate) : this.moment("9999-12-30");

            if (dataItem.Terminated) {
                var terminationDate = this.moment(dataItem.Terminated);
                if (dataItem.TerminationDeadline) {
                    terminationDate.add(dataItem.TerminationDeadline.Name, "months");
                }
                // indgået-dato <= dags dato <= opsagt-dato + opsigelsesfrist
                return today >= startDate && today <= terminationDate;
            }

            // indgået-dato <= dags dato <= udløbs-dato
            return today >= startDate && today <= endDate;
        }

        private exportFlag = false;
        private exportToExcel = (e) => {
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
                this._.forEach(columns, column => {
                    if (column.tempVisual) {
                        delete column.tempVisual;
                        e.sender.hideColumn(column);
                    }
                });
            }
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
                var currentFilter = dataSource.filter();
                var selectedIndex = kendoElem.select();
                var selectedId = self._.parseInt(kendoElem.value());
                var childIds = kendoElem.dataItem().childIds;

                self.$window.sessionStorage.setItem(self.orgUnitStorageKey, selectedId.toString());

                if (selectedIndex > 0) {
                    // filter by selected
                    dataSource.filter(self.getFilterWithOrgUnit(currentFilter, selectedId, childIds));
                } else {
                    // else clear filter because the 0th element should act like a placeholder
                    dataSource.filter(self.getFilterWithOrgUnit(currentFilter));
                }
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

        private getFilterWithOrgUnit(currentFilter: kendo.data.DataSourceFilters, selectedId?: number, childIds?: number[]): kendo.data.DataSourceFilters {
            var field = "ResponsibleOrganizationUnit.Id";
            // remove old values first
            var newFilter = this._.removeFiltersForField(currentFilter, field);

            // is selectedId a number?
            if (!isNaN(selectedId)) {
                newFilter = this._.addFilter(newFilter, field, "eq", selectedId, "or");
                // add children to filters
                this._.forEach(childIds, id => newFilter = this._.addFilter(newFilter, field, "eq", id, "or"));
            }
            return newFilter;
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
                    this.mainGrid.hideColumn("ContractSigner.Name");
                    this._.forEach(this.itContractRoles, role => this.mainGrid.hideColumn(`role${role.Id}`));

                    var selectedId = e.sender.value();
                    //var gridFieldName = "role" + selectedId;
                    // show only the selected role column
                    this.mainGrid.showColumn(selectedId);
                }
            }
        };
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-contract.overview", {
                    url: "/overview",
                    templateUrl: "partials/it-contract/it-contract-overview.html",
                    controller: OverviewController,
                    controllerAs: "contractOverviewVm",
                    resolve: {
                        itContractRoles: [
                            "$http", $http => $http.get("/odata/ItContractRoles").then(result => result.data.value)
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        orgUnits: [
                            "$http", "user", "_", ($http, user, _) => $http.get(`/odata/Organizations(${user.currentOrganizationId})/OrganizationUnits`).then(result => _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId"))
                        ],
                        // TODO this isn't a sustainable solution - but a workaround for now...
                        ecoStreamData: [
                            "$http", "user", ($http, user) => $http.get(`/odata/ExternEconomyStreams(Organization=${user.currentOrganizationId})`).then(result => result.data.value)
                        ]
                    }
                });
            }
        ]);
}

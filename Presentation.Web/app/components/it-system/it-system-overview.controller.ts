module Kitos.ItSystem.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: Kitos.IKendoGrid<IItSystemUsageOverview>;
        mainGridOptions: kendo.ui.GridOptions;
        roleSelectorOptions: any;
    }

    export interface IItSystemUsageOverview extends Models.ItSystemUsage.IItSystemUsage {
        roles: Array<string>;
    }

    // Here be dragons! Thou art forewarned.
    // Or perhaps it's samurais, because it's kendos terrible terrible framework that's the cause...
    export class OverviewController implements IOverviewController {
        private storageKey = "it-system-overview-options";
        private orgUnitStorageKey = "it-system-overview-orgunit";
        private gridState = this.gridStateService.getService(this.storageKey, this.user.id);

        public mainGrid: Kitos.IKendoGrid<IItSystemUsageOverview>;
        public mainGridOptions: kendo.ui.GridOptions;
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
            "systemRoles",
            "user",
            "gridStateService",
            "orgUnits",
            "needsWidthFixService",
            "exportGridToExcelService"
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
            private systemRoles: Array<any>,
            private user,
            private gridStateService: Services.IGridStateFactory,
            private orgUnits: Array<any>,
            private needsWidthFixService,
            private exportGridToExcelService) {
            $rootScope.page.title = "IT System - Overblik";

            $scope.$on("kendoWidgetCreated", (event, widget) => {
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

            this.activate();
        }

        // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(concat(concat(c/User/Name, ' '), c/User/LastName),'foo') and c/RoleId eq {roleId})"
        private fixRoleFilter(filterUrl, roleName, roleId) {
            var pattern = new RegExp(`(\\w+\\()${roleName}(.*?\\))`, "i");
            return filterUrl.replace(pattern, `Rights/any(c: $1concat(concat(c/User/Name, ' '), c/User/LastName)$2 and c/RoleId eq ${roleId})`);
        }

        private fixKleIdFilter(filterUrl, column) {
            var pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
            return filterUrl.replace(pattern, "ItSystem/TaskRefs/any(c: $1c/TaskKey$2)");
        }

        private fixKleDescFilter(filterUrl, column) {
            var pattern = new RegExp(`(\\w+\\()${column}(.*?\\))`, "i");
            return filterUrl.replace(pattern, "ItSystem/TaskRefs/any(c: $1c/Description$2)");
        }

        // saves grid state to local storage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // Resets the position of the scrollbar
        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.mainGrid);
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            //Add only excel option if user is not readonly
            this.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            this.gridState.loadGridOptions(this.mainGrid);
        }

        public saveGridProfile() {
            Utility.KendoFilterProfileHelper.saveProfileLocalStorageData(this.$window, this.orgUnitStorageKey);

            this.gridState.saveGridProfile(this.mainGrid);
            this.notify.addSuccessMessage("Filtre og sortering gemt");
        }

        public loadGridProfile() {
            this.gridState.loadGridProfile(this.mainGrid);

            Utility.KendoFilterProfileHelper.saveProfileSessionStorageData(this.$window, this.$, this.orgUnitStorageKey, "ResponsibleUsage.OrganizationUnit.Name");

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
        };

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        public isValidUrl(Url) {
            var regexp = /(http || https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
            return regexp.test(Url.toLowerCase());
        };

        private activate() {
            // overview grid options
            var mainGridOptions: Kitos.IKendoGridOptions<IItSystemUsageOverview> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                var urlParameters = `?$expand=ItSystem($expand=BelongsTo,BusinessType,Parent,TaskRefs),\
ArchivePeriods,\
Reference,\
Organization,\
ResponsibleUsage($expand=OrganizationUnit),\
MainContract($expand=ItContract($expand=Supplier)),\
Contracts($expand=ItContract($expand=ContractType,AssociatedAgreementElementTypes($expand=AgreementElementType))),\
Rights($expand=User,Role),\
ArchiveType,\
SensitiveDataType,\
ObjectOwner,\
LastChangedByUser,\
ItProjects($select=Name)`;
                                // if orgunit is set then the org unit filter is active
                                var orgUnitId = this.$window.sessionStorage.getItem(this.orgUnitStorageKey);
                                if (orgUnitId === null) {
                                    return `/odata/Organizations(${this.user.currentOrganizationId})/ItSystemUsages` + urlParameters;
                                } else {
                                    return `/odata/Organizations(${this.user.currentOrganizationId})/OrganizationUnits(${orgUnitId})/ItSystemUsages` + urlParameters;
                                }
                            },
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            // get kendo to map parameters to an odata url
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                //the role list is sorted since all roles after role 1 will match in regexp resulting in a bad request do not change this.
                                var sortedRoles = this.systemRoles.sort((n1, n2) => {
                                    if (n1.Id > n2.Id) {
                                        return -1;
                                    }
                                    if (n1.Id < n2.Id) {
                                        return 1;
                                    }
                                    return 0;
                                });

                                this._.forEach(sortedRoles, role => {
                                    parameterMap.$filter = this.fixRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id);

                                });

                                parameterMap.$filter = this.fixKleIdFilter(parameterMap.$filter, "ItSystem/TaskRefs/TaskKey");
                                parameterMap.$filter = this.fixKleDescFilter(parameterMap.$filter, "ItSystem/TaskRefs/Description");

                                // replaces "contains(ItSystem/Uuid,'11')" with "contains(CAST(ItSystem/Uuid, 'Edm.String'),'11')"
                                parameterMap.$filter = parameterMap.$filter.replace(/contains\(ItSystem\/Uuid,/, "contains(CAST(ItSystem/Uuid, 'Edm.String'),");
                                parameterMap.$filter = parameterMap.$filter.replace(`ItSystem/TaskRefs/any(c: startswith(c/TaskKey,'""'))`, `ItSystem/TaskRefs/any(c: contains(c/TaskKey,'')) eq false`);
                                parameterMap.$filter = parameterMap.$filter.replace(`ItSystem/TaskRefs/any(c: startswith(c/TaskKey,'alt'))`, `ItSystem/TaskRefs/any(c: contains(c/TaskKey,'')) eq true`);
                            }

                            return parameterMap;
                        }
                    },
                    sort: {
                        field: "SystemName",
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    schema: {
                        model: {
                            fields: {
                                LastChanged: { type: "date" },
                                Concluded: { type: "date" },
                                ArchiveDuty: { type: "number" },
                                Registertype: { type: "boolean" },
                                EndDate: { from: "ArchivePeriods.EndDate", type: "date" },
                                SystemName: { from: "ItSystem.Name", type: "string" },
                                IsActive: { type: "boolean" },
                            }
                        },
                        parse: response => {
                            // HACK to flattens the Rights on usage so they can be displayed as single columns

                            // iterrate each usage
                            this._.forEach(response.value, usage => {
                                usage.roles = [];
                                // iterrate each right
                                this._.forEach(usage.Rights, right => {
                                    // init an role array to hold users assigned to this role
                                    if (!usage.roles[right.RoleId])
                                        usage.roles[right.RoleId] = [];

                                    // push username to the role array
                                    usage.roles[right.RoleId].push([right.User.Name, right.User.LastName].join(" "));
                                });

                                if (!usage.ItSystem.Parent) { usage.ItSystem.Parent = { Name: "" }; }
                                if (!usage.ResponsibleUsage) { usage.ResponsibleUsage = { OrganizationUnit: { Name: "" } }; }
                                if (!usage.ItSystem.BusinessType) { usage.ItSystem.BusinessType = { Name: "" }; }
                                if (!usage.ItSystem.TaskRefs) { usage.ItSystem.TaskRefs = { TaskKey: "", Description: "" }; }
                                if (!usage.SensitiveDataType) { usage.SensitiveDataType = { Name: "" }; }
                                if (!usage.MainContract) { usage.MainContract = { ItContract: { Supplier: { Name: "" } } }; }
                                if (!usage.Reference) { usage.Reference = { Title: "", ExternalReferenceId: "" }; }
                                if (!usage.MainContract.ItContract.Supplier) { usage.MainContract.ItContract.Supplier = { Name: "" }; }
                                if (!usage.ItSystem.BelongsTo) { usage.ItSystem.BelongsTo = { Name: "" }; }
                            });
                            return response;
                        }
                    }
                },
                toolbar: [
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='systemOverviewVm.clearOptions()' data-element-type='resetFilterButton'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Gem filtre og sortering' data-ng-click='systemOverviewVm.saveGridProfile()' data-element-type='saveFilterButton'>#: text #</button>"
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Anvend gemte filtre og sortering' data-ng-click='systemOverviewVm.loadGridProfile()' data-ng-disabled='!systemOverviewVm.doesGridProfileExist()' data-element-type='useFilterButton'>#: text #</button>"
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='systemOverviewVm.clearGridProfile()' data-ng-disabled='!systemOverviewVm.doesGridProfileExist()' data-element-type='removeFilterButton'>#: text #</button>"
                    },
                    {
                        template: kendo.template(this.$("#role-selector").html())
                    }
                ],
                excel: {
                    fileName: "IT System Overblik.xlsx",
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
                dataBound: this.saveGridOptions,
                columnResize: this.saveGridOptions,
                columnHide: this.saveGridOptions,
                columnShow: this.saveGridOptions,
                columnReorder: this.saveGridOptions,
                excelExport: this.exportToExcel,
                page: this.onPaging,
                columns: [
                    {
                        field: "IsActive", title: "Gyldig/Ikke gyldig", width: 90,
                        persistId: "isActive",
                        template: dataItem => {
                            if (dataItem.IsActive) {
                                return '<span class="fa fa-file text-success" aria-hidden="true"></span>';
                            }
                            return '<span class="fa fa-file-o text-muted" aria-hidden="true"></span>';
                        },
                        excelTemplate: dataItem => {
                            var isActive = this.isContractActive(dataItem);
                            return isActive.toString();
                        },
                        attributes: { "class": "text-center" },
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "LocalSystemId", title: "Lokal system ID", width: 150,
                        persistId: "localid",
                        excelTemplate: dataItem => dataItem && dataItem.LocalSystemId || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            },
                            ignoreCase: true
                        }
                    },
                    {
                        field: "ItSystem.Uuid", title: "UUID", width: 150,
                        persistId: "uuid",
                        excelTemplate: dataItem => dataItem.ItSystem.Uuid,
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
                        field: "ItSystem.Parent.Name", title: "Overordnet IT System", width: 150,
                        persistId: "parentsysname",
                        template: dataItem => dataItem.ItSystem.Parent ? dataItem.ItSystem.Parent.Name : "",
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
                        field: "SystemName", title: "IT System", width: 320,
                        persistId: "sysname",
                        template: dataItem => {
                            if (dataItem.ItSystem.Disabled)
                                return `<a data-ui-sref='it-system.usage.main({id: ${dataItem.Id}})'>${dataItem.ItSystem.Name} (Slettes) </a>`;
                            else
                                return `<a data-ui-sref='it-system.usage.main({id: ${dataItem.Id}})'>${dataItem.ItSystem.Name}</a>`;
                        },
                        attributes: {
                            "data-element-type": "systemNameKendoObject"
                        },
                        headerAttributes: {
                            "data-element-type": "systemNameKendoHeader"
                        },
                        excelTemplate: dataItem => {
                            if (dataItem && dataItem.ItSystem && dataItem.ItSystem.Name) {
                                if (dataItem.ItSystem.Disabled)
                                    return dataItem.ItSystem.Name + " (Slettes)";
                                else
                                    return dataItem.ItSystem.Name;
                            } else {
                                return "";
                            }
                        },
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
                        field: "Version", title: "Version", width: 150,
                        persistId: "version",
                        excelTemplate: dataItem => dataItem && dataItem.Version || "",
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
                        field: "LocalCallName", title: "Lokal kaldenavn", width: 150,
                        persistId: "localname",
                        excelTemplate: dataItem => dataItem && dataItem.LocalCallName || "",
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
                        field: "ResponsibleUsage.OrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 190,
                        persistId: "orgunit",
                        template: dataItem => dataItem.ResponsibleUsage ? dataItem.ResponsibleUsage.OrganizationUnit.Name : "",
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.orgUnitDropDownList
                            }
                        }
                    },
                    {
                        field: "ItSystem.BusinessType.Name", title: "Forretningstype", width: 150,
                        persistId: "busitype",
                        template: dataItem => dataItem.ItSystem.BusinessType ? dataItem.ItSystem.BusinessType.Name : "",
                        attributes: { "class": "might-overflow" },
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
                        field: "ItSystem.TaskRefs.TaskKey", title: "KLE ID", width: 150,
                        persistId: "taskkey",
                        template: dataItem => dataItem.ItSystem.TaskRefs.length > 0 ? this._.map(dataItem.ItSystem.TaskRefs, "TaskKey").join(", ") : "",
                        attributes: { "class": "might-overflow" },
                        hidden: true,
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "startswith"
                            }
                        },
                        sortable: false
                    },
                    {
                        field: "ItSystem.TaskRefs.Description", title: "KLE navn", width: 150,
                        persistId: "klename",
                        template: dataItem => dataItem.ItSystem.TaskRefs.length > 0 ? this._.map(dataItem.ItSystem.TaskRefs, "Description").join(", ") : "",
                        attributes: { "class": "might-overflow" },
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
                        field: "Reference.Title", title: "Reference", width: 150,
                        persistId: "ReferenceId",
                        template: dataItem => {
                            var reference = dataItem.Reference;
                            if (reference != null) {
                                if (Utility.Validation.validateUrl(reference.URL)) {
                                    return "<a target=\"_blank\" style=\"float:left;\" href=\"" + reference.URL + "\">" + reference.Title + "</a>";
                                } else {
                                    return reference.Title;
                                }
                            }
                            return "";
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
                        field: "Reference.ExternalReferenceId", title: "Mappe ref", width: 150,
                        persistId: "folderref",
                        template: dataItem => {
                            var reference = dataItem.Reference;
                            if (reference != null) {
                                if (Utility.Validation.validateUrl(reference.ExternalReferenceId)) {
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
                    //{
                    //    field: "DataLevel", title: "Datatype", width: 150,
                    //    persistId: "dataLevel",
                    //    template: dataItem => {
                    //        switch (dataItem.DataLevel) {
                    //            case "PERSONALDATA":
                    //                return "Persondata";
                    //            case "PERSONALDATANDSENSITIVEDATA":
                    //                return "Persondata og følsomme persondata";
                    //            default:
                    //                return "Ingen persondata";
                    //        }
                    //    },
                    //    attributes: { "class": "might-overflow" },
                    //    hidden: true,
                    //    filterable: {
                    //        cell: {
                    //            template: function (args) {
                    //                args.element.kendoDropDownList({
                    //                    dataSource: [{ type: "Ingen persondata", value: "NONE" }, { type: "Persondata", value: "PERSONALDATA" }, { type: "Persondata og følsomme persondata", value: "PERSONALDATANDSENSITIVEDATA" }],
                    //                    dataTextField: "type",
                    //                    dataValueField: "value",
                    //                    valuePrimitive: true
                    //                });
                    //            },
                    //            showOperators: false
                    //        }
                    //    }
                    //},
                    {
                        field: "MainContract", title: "Kontrakt", width: 120,
                        persistId: "contract",
                        template: dataItem => {
                            if (!dataItem.MainContract || !dataItem.MainContract.ItContract || !dataItem.MainContract.ItContract.Name) {
                                return "";
                            }
                            if (this.isContractActive(dataItem.MainContract.ItContract)) {
                                return `<a data-ui-sref="it-system.usage.contracts({id: ${dataItem.Id}})"><span class="fa fa-file text-success" aria-hidden="true"></span></a>`;
                            } else {
                                return `<a data-ui-sref="it-system.usage.contracts({id: ${dataItem.Id}})"><span class="fa fa-file-o text-muted" aria-hidden="true"></span></a>`;
                            }
                        },
                        excelTemplate: dataItem =>
                            dataItem && dataItem.MainContract && dataItem.MainContract.ItContract ? this.isContractActive(dataItem.MainContract.ItContract).toString() : "",
                        attributes: { "class": "text-center" },
                        sortable: false,
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.contractFilterDropDownList
                            }
                        }
                    },
                    {
                        field: "MainContract.ItContract.Supplier.Name", title: "Leverandør", width: 175,
                        persistId: "supplier",
                        template: dataItem =>
                            dataItem.MainContract &&
                            dataItem.MainContract.ItContract &&
                            dataItem.MainContract.ItContract.Supplier &&
                            dataItem.MainContract.ItContract.Supplier.Name || "",
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
                        field: "ItSystem.BelongsTo.Name", title: "Rettighedshaver", width: 210,
                        persistId: "belongsto",
                        template: dataItem => dataItem.ItSystem.BelongsTo ? dataItem.ItSystem.BelongsTo.Name : "",
                        attributes: {
                            "data-element-type": "systemRightsOwnerObject"
                        },
                        headerAttributes: {
                            "data-element-type": "systemRightsOwnerHeader"
                        },
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        },
                    },
                    {
                        field: "ItProjects", title: "IT Projekt", width: 150,
                        persistId: "sysusage",
                        template: dataItem => dataItem.ItProjects.length > 0 ? this._.first(dataItem.ItProjects).Name : "",
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
                        field: "ObjectOwner.Name", title: "Taget i anvendelse af", width: 150,
                        persistId: "ownername",
                        template: dataItem => `${dataItem.ObjectOwner.Name} ${dataItem.ObjectOwner.LastName}`,
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
                        field: "LastChangedByUser.Name", title: "Sidst redigeret: Bruger", width: 150,
                        persistId: "lastchangedname",
                        template: dataItem => `${dataItem.LastChangedByUser.Name} ${dataItem.LastChangedByUser.LastName}`,
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
                        field: "LastChanged", title: "Sidste redigeret: Dato", format: "{0:dd-MM-yyyy}", width: 150,
                        persistId: "changed",
                        excelTemplate: dataItem => {
                            if (!dataItem || !dataItem.LastChanged) {
                                return "";
                            }
                            return this.moment(dataItem.LastChanged).format("DD-MM-YYYY");
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
                        field: "Concluded", title: "Ibrugtagningsdato", format: "{0:dd-MM-yyyy}", width: 150,
                        persistId: "concludedSystemFrom",
                        hidden: false,
                        excelTemplate: dataItem => {
                            if (!dataItem || !dataItem.Concluded) {
                                return "";
                            }
                            return dataItem.Concluded.toLocaleDateString("da-DK");
                        },
                        filterable:
                        {
                            operators: {
                                date: {
                                    eq: "Lig med",
                                    gte: "Fra og med",
                                    lte: "Til og med"
                                }
                            }
                        }
                    },
                    {
                        field: "ArchiveDuty", title: "Arkiveringspligt", width: 160,
                        persistId: "ArchiveDuty",
                        template: dataItem => {
                            switch (dataItem.ArchiveDuty) {
                                case 1:
                                    return "B";
                                case 2:
                                    return "K";
                                case 3:
                                    return "Ved ikke";
                                default:
                                    return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            switch (dataItem.ArchiveDuty) {
                                case 1:
                                    return "B";
                                case 2:
                                    return "K";
                                case 3:
                                    return "Ved ikke";
                                default:
                                    return "";
                            }
                        },
                        hidden: false,
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "B", value: 1 }, { type: "K", value: 2 }, { type: "Ved ikke", value: 3 }],
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
                        field: "Registertype", title: "Er dokumentbærende", width: 160,
                        persistId: "Registertype",
                        template: dataItem => { return dataItem.Registertype ? "Ja" : "Nej"; },
                        hidden: false,
                        filterable: {
                            cell: {
                                template: function (args) {
                                    args.element.kendoDropDownList({
                                        dataSource: [{ type: "Ja", value: true }, { type: "Nej", value: false }],
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
                        field: "EndDate", title: "Journalperiode slutdato", format: "{0:dd-MM-yyyy}", width: 180,
                        persistId: "ArchivePeriodsEndDate",
                        template: dataItem => {
                            if (!dataItem || !dataItem.ArchivePeriods) {
                                return "";
                            }
                            let dateList;
                            _.each(dataItem.ArchivePeriods, x => {
                                if (moment().isBetween(moment(x.StartDate).startOf('day'), moment(x.EndDate).endOf('day'), null, '[]')) {
                                    if (!dateList || dateList.StartDate > x.StartDate) {
                                        dateList = x;
                                    }
                                }
                            });
                            if (!dateList) {
                                return "";
                            } else {
                                return this.moment(dateList.EndDate).format("DD-MM-YYYY");
                            }

                        },
                        hidden: true,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "RiskSupervisionDocumentationUrlName", title: "Risikovurdering", width: 150,
                        persistId: "riskSupervisionDocumentationUrlName",
                        template: dataItem => {
                            if (dataItem.RiskSupervisionDocumentationUrl != null && dataItem.RiskSupervisionDocumentationUrlName != null) {
                                return "<a href=\"" + dataItem.RiskSupervisionDocumentationUrl + "\">" + dataItem.RiskSupervisionDocumentationUrlName + "</a>";
                            }
                            else if (dataItem.RiskSupervisionDocumentationUrl != null && dataItem.RiskSupervisionDocumentationUrlName == null) {
                                return "<a href=\"" + dataItem.RiskSupervisionDocumentationUrl + "\">" + dataItem.RiskSupervisionDocumentationUrl + "</a>";
                            }
                            else if (dataItem.RiskSupervisionDocumentationUrlName != null) {
                                return dataItem.RiskSupervisionDocumentationUrlName;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            return Helpers.ExcelExportHelper.renderUrlOrFallback(
                                dataItem.RiskSupervisionDocumentationUrl,
                                dataItem.RiskSupervisionDocumentationUrlName);
                        },
                        attributes: { "class": "text-left" },
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
                        field: "LinkToDirectoryUrlName", title: "Fortegnelse", width: 150,
                        persistId: "LinkToDirectoryUrlName",
                        template: dataItem => {
                            if (dataItem.LinkToDirectoryUrl != null && dataItem.LinkToDirectoryUrlName != null) {
                                return "<a href=\"" + dataItem.LinkToDirectoryUrl + "\">" + dataItem.LinkToDirectoryUrlName + "</a>";
                            }
                            else if (dataItem.LinkToDirectoryUrl != null && dataItem.LinkToDirectoryUrlName == null) {
                                return "<a href=\"" + dataItem.LinkToDirectoryUrl + "\">" + dataItem.LinkToDirectoryUrl + "</a>";
                            }
                            else if (dataItem.LinkToDirectoryUrlName != null) {
                                return dataItem.LinkToDirectoryUrlName;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            return Helpers.ExcelExportHelper.renderUrlOrFallback(
                                dataItem.LinkToDirectoryUrl,
                                dataItem.LinkToDirectoryUrlName);
                        },
                        attributes: { "class": "text-left" },
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
                        field: "ItContractDataHandler", title: "Databehandleraftale", width: 150,
                        persistId: "ItContractDataHandler",
                        template: dataItem => {
                            if (dataItem.Contracts != null) {
                                if (dataItem.Contracts.some(x => x.ItContract.ContractType !== null && x.ItContract.ContractType.Name === "Databehandleraftale") || dataItem.Contracts.some(x => x.ItContract.AssociatedAgreementElementTypes !== null && x.ItContract.AssociatedAgreementElementTypes.some(x => x.AgreementElementType.Name === "Databehandleraftale"))) {
                                    return "Ja";
                                } else {
                                    return "Nej";
                                }
                            } else {
                                return "Nej";
                            }
                        },
                        attributes: { "class": "text-left" },
                        hidden: true,
                        filterable: false,
                        sortable: false
                    }
                ]
            };

            // find the index of column where the role columns should be inserted
            var insertIndex = this._.findIndex(mainGridOptions.columns, { 'persistId': 'orgunit' }) + 1;

            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ''
                });
            }
            // add a role column for each of the roles
            // note iterating in reverse so we don't have to update the insert index
            this._.forEachRight(this.systemRoles, role => {
                var roleColumn = {
                    field: `role${role.Id}`,
                    title: role.Name,
                    persistId: `role${role.Id}`,
                    template: dataItem => {
                        var roles = "";

                        if (dataItem.roles[role.Id] === undefined)
                            return roles;

                        roles = this.concatRoles(dataItem.roles[role.Id]);

                        var link = `<a data-ui-sref='it-system.usage.roles({id: ${dataItem.Id}})'>${roles}</a>`;

                        return link;
                    },
                    excelTemplate: dataItem => {
                        if (!dataItem || dataItem.roles[role.Id] === undefined) {
                            return "";
                        }

                        return this.concatRoles(dataItem.roles[role.Id]);
                    },
                    width: 145,
                    hidden: !(role.Name === "Systemejer"), // hardcoded role name :(
                    sortable: false,
                    filterable: {
                        cell: {
                            template: customFilter,
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

        private isContractActive(dataItem) {
            if (!dataItem.Active) {
                var today = moment();
                var startDate = dataItem.Concluded ? moment(dataItem.Concluded, "YYYY-MM-DD").startOf('day') : moment().startOf('day');
                var endDate = dataItem.ExpirationDate ? moment(dataItem.ExpirationDate, "YYYY-MM-DD").endOf('day') : this.moment("9999-12-30", "YYYY-MM-DD").endOf('day');
                if (dataItem.Terminated) {
                    var terminationDate = moment(dataItem.Terminated, "YYYY-MM-DD").endOf('day');
                    if (dataItem.TerminationDeadline) {
                        terminationDate.add(dataItem.TerminationDeadline.Name, "months");
                    }
                    // indgået-dato <= dags dato <= opsagt-dato + opsigelsesfrist
                    return today.isBetween(startDate, terminationDate, null, '[]');
                }
                // indgået-dato <= dags dato <= udløbs-dato
                return today.isBetween(startDate, endDate, null, '[]');
            }
            return dataItem.Active;
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

        private contractFilterDropDownList = (args) => {
            var self = this;
            var gridDataSource = args.dataSource;

            function setContractFilter() {
                var kendoElem = this;
                var currentFilter = gridDataSource.filter();
                var contractFilterObj = self._.findKeyDeep(currentFilter, { field: "MainContract" });

                if (contractFilterObj.operator == "neq") {
                    kendoElem.select(1); // index of "Har kontrakt"
                } else if (contractFilterObj.operator == "eq") {
                    kendoElem.select(2); // index of "Ingen kontrakt"
                }
            }

            function filterByContract() {
                var kendoElem = this;
                // can't use args.dataSource directly,
                // if we do then the view doesn't update.
                // So have to go through $scope - sadly :(
                var dataSource = self.mainGrid.dataSource;
                var selectedValue = kendoElem.value();
                var field = "MainContract";
                var currentFilter = dataSource.filter();
                // remove old value first
                var newFilter = self._.removeFiltersForField(currentFilter, field);

                if (selectedValue === "Har kontrakt") {
                    newFilter = self._.addFilter(newFilter, field, "ne", null, "and");
                } else if (selectedValue === "Ingen kontrakt") {
                    newFilter = self._.addFilter(newFilter, field, "eq", null, "and");
                }

                dataSource.filter(newFilter);
            }

            // http://dojo.telerik.com/ODuDe/5
            args.element.removeAttr("data-bind");
            args.element.kendoDropDownList({
                dataSource: ["Har kontrakt", "Ingen kontrakt"],
                optionLabel: "Vælg filter...",
                dataBound: setContractFilter,
                change: filterByContract
            });
        }

        public roleSelectorOptions = () => {
            return {
                autoBind: false,
                dataSource: this.systemRoles,
                dataTextField: "Name",
                dataValueField: "Id",
                optionLabel: "Vælg systemrolle...",
                change: e => {
                    // hide all roles column
                    this._.forEach(this.systemRoles, role => {
                        this.mainGrid.hideColumn(`role${role.Id}`);
                    });

                    var selectedId = e.sender.value();
                    var gridFieldName = `role${selectedId}`;
                    // show only the selected role column
                    this.mainGrid.showColumn(gridFieldName);
                    this.needsWidthFixService.fixWidth();
                }
            }
        };

        private exportToExcel = (e: IKendoGridExcelExportEvent<IItSystemUsageOverview>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid);
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-system.overview", {
                    url: "/overview",
                    templateUrl: "app/components/it-system/it-system-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "systemOverviewVm",
                    resolve: {
                        systemRoles: [
                            "$http", $http => $http.get("odata/LocalItSystemRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        orgUnits: [
                            "$http", "user", "_",
                            ($http, user, _) => $http.get(`/odata/Organizations(${user.currentOrganizationId})/OrganizationUnits`)
                                .then(result => _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId"))
                        ]
                    }
                });
            }
        ]);
}

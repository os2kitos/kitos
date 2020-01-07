module Kitos.ItProject.OverviewInactive {
    "use strict";

    export interface IOverviewInactiveController {
        mainGrid: IKendoGrid<IItProjectInactiveOverview>;
        mainGridOptions: kendo.ui.GridOptions;
        roleSelectorOptions: any;

        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): void;
        clearOptions(): void;
    }

    export interface IItProjectInactiveOverview extends Models.ItProject.IItProject {
        roles: Array<any>;
        CurrentPhaseObj: Models.ItProject.IItProjectPhase;
    }

    export class OverviewInactiveController implements IOverviewInactiveController {
        private storageKey = "it-project-overview-inactive-options";
        private orgUnitStorageKey = "it-project-overview-inactive-orgunit";
        private gridState = this.gridStateService.getService(this.storageKey);
        public mainGrid: IKendoGrid<IItProjectInactiveOverview>;
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
            "projectRoles",
            "user",
            "gridStateService",
            "orgUnits",
            "economyCalc",
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
            private projectRoles,
            private user,
            private gridStateService: Services.IGridStateFactory,
            private orgUnits: any,
            private economyCalc,
            private needsWidthFixService) {
            this.$rootScope.page.title = "IT Projekt - Overblik";

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

            this.activate();
        }

        // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(concat(concat(c/User/Name, ' '), c/User/LastName),'foo') and c/RoleId eq {roleId})"
        private fixRoleFilter(filterUrl, roleName, roleId) {
            var pattern = new RegExp(`(\\w+\\()${roleName}(.*?\\))`, "i");
            return filterUrl.replace(pattern, `Rights/any(c: $1concat(concat(c/User/Name, ' '), c/User/LastName)$2 and c/RoleId eq ${roleId})`);
        }

        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // Resets the scrollbar position
        private onPaging = () => {
            this.mainGrid.content.scrollTop(0);
            this.mainGrid.content.scrollLeft(0);
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            //Add only excel option if user is not readonly
            if (!this.user.isReadOnly) {
                this.mainGrid.options.toolbar.push({ name: "excel", text: "Eksportér til Excel", className: "pull-right" });
            }
            this.gridState.loadGridOptions(this.mainGrid);
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
            var orgUnitFilterRow = this.$(".k-filter-row [data-field='ResponsibleUsage.OrganizationUnit.Name']");
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

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        private activate() {
            var mainGridOptions: IKendoGridOptions<IItProjectInactiveOverview> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                var urlParameters = `?$expand=ItProjectStatuses,Parent,ItProjectType,Rights($expand=User,Role),ResponsibleUsage($expand=OrganizationUnit),GoalStatus,EconomyYears,ItProjectStatusUpdates($orderby=Created desc;$top=1)`;
                                // if orgunit is set then the org unit filter is active
                                var orgUnitId = this.$window.sessionStorage.getItem(this.orgUnitStorageKey);
                                if (orgUnitId === null) {
                                    return `/odata/Organizations(${this.user.currentOrganizationId})/ItProjects` + urlParameters;
                                } else {
                                    return `/odata/Organizations(${this.user.currentOrganizationId})/OrganizationUnits(${orgUnitId})/ItProjects` + urlParameters;
                                }
                            },
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            // get kendo to map parameters to an odata url
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                this._.forEach(this.projectRoles, role => {
                                    parameterMap.$filter = this.fixRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id);
                                });
                            }

                            return parameterMap;
                        }
                    },
                    filter: { field: "IsArchived", operator: "eq", value: true }, // default filter
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
                                StatusDate: { type: "date" },
                                LastChanged: { type: "date" },
                                IsTransversal: { type: "boolean" },
                                IsStrategy: { type: "boolean" },
                                IsArchived: { type: "boolean" }
                            }
                        },
                        parse: response => {
                            // HACK to flatten the Rights on usage so they can be displayed as single columns

                            // iterrate each project
                            this._.forEach(response.value, project => {
                                project.roles = [];
                                // iterrate each right
                                this._.forEach(project.Rights, right => {
                                    // init an role array to hold users assigned to this role
                                    if (!project.roles[right.RoleId])
                                        project.roles[right.RoleId] = [];

                                    // push username to the role array
                                    project.roles[right.RoleId].push([right.User.Name, right.User.LastName].join(" "));
                                });

                                var phase = `Phase${project.CurrentPhase}`;
                                project.CurrentPhaseObj = project[phase];

                                if (this.user.isGlobalAdmin || this.user.isLocalAdmin || this._.find(project.Rights, { 'userId': this.user.id })) {
                                    project.hasWriteAccess = true;
                                }
                                if (!project.Parent) { project.Parent = { Name: "" }; }
                                if (!project.ResponsibleUsage) { project.ResponsibleUsage = { OrganizationUnit: { Name: "" } } };
                                if (!project.ItProjectType) { project.ItProjectType = { Name: "" }; }
                                if (!project.GoalStatus) { project.GoalStatus = { Status: "" }; }
                                if (!project.Reference) { project.Reference = { Title: "", ExternalReferenceId: "" }; }
                            });
                            return response;
                        }
                    }
                },
                toolbar: [
                    {
                        name: "clearFilter",
                        text: "Nulstil",
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='projectOverviewVm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="projectOverviewVm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="projectOverviewVm.loadGridProfile()" data-ng-disabled="!projectOverviewVm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='projectOverviewVm.clearGridProfile()' data-ng-disabled='!projectOverviewVm.doesGridProfileExist()'>#: text #</button>"
                    },
                    {
                        template: kendo.template(this.$("#role-selector").html())
                    }
                ],
                excel: {
                    fileName: "IT Project inaktive.xlsx",
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
                    mode: "row",
                    messages: {
                        isTrue: "✔",
                        isFalse: "✖"
                    }
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
                        field: "ItProjectId", title: "ProjektID", width: 115,
                        persistId: "projid", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.ItProjectId ? dataItem.ItProjectId : "",
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
                        field: "Parent.Name", title: "Overordnet IT Projekt", width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Parent ? `<a data-ui-sref="it-project.edit.main({id:${dataItem.Parent.Id}})">${dataItem.Parent.Name}</a>` : "",
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
                        field: "Name", title: "IT Projekt", width: 370,
                        persistId: "projname", // DON'T YOU DARE RENAME!
                        template: dataItem => `<a data-ui-sref="it-project.edit.main({id: ${dataItem.Id}})">${dataItem.Name}</a>`,
                        excelTemplate: dataItem => dataItem && dataItem.Name ? dataItem.Name : "",
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
                        field: "ResponsibleUsage.OrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 245,
                        persistId: "orgunit", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.ResponsibleUsage && dataItem.ResponsibleUsage.OrganizationUnit.Name || "",
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: this.orgUnitDropDownList
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
                        persistId: "folderref", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            var reference = dataItem.Reference;
                            if (reference != null) {
                                if (reference.ExternalReferenceId) {
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
                        field: "ItProjectType.Name", title: "Projekttype", width: 135,
                        persistId: "projtype", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.ItProjectType && dataItem.ItProjectType.Name || "",
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
                        field: "CurrentPhaseObj", title: "Fase", width: 100,
                        persistId: "phasename", // DON'T YOU DARE RENAME!
                        template: dataItem =>
                            dataItem.CurrentPhaseObj ? `<a data-ui-sref="it-project.edit.phases({id:${dataItem.Id}})">${dataItem.CurrentPhaseObj.Name}</a>` : "",
                        excelTemplate: dataItem => dataItem && dataItem.CurrentPhaseObj && dataItem.CurrentPhaseObj.Name || "",
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "CurrentPhaseObj.StartDate", title: "Fase: Startdato", format: "{0:dd-MM-yyyy}", width: 85,
                        persistId: "phasestartdate", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            // handles null cases
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.StartDate) {
                                return "";
                            }

                            return this.moment(dataItem.CurrentPhaseObj.StartDate).format("DD-MM-YYYY");
                        },
                        excelTemplate: dataItem => {
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.StartDate) {
                                return "";
                            }

                            return this.moment(dataItem.CurrentPhaseObj.StartDate).format("DD-MM-YYYY");
                        },
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "CurrentPhaseObj.EndDate", title: "Fase: Slutdato", format: "{0:dd-MM-yyyy}", width: 85,
                        persistId: "phaseenddate", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => {
                            // handles null cases
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.EndDate) {
                                return "";
                            }

                            return this.moment(dataItem.CurrentPhaseObj.EndDate).format("DD-MM-YYYY");
                        },
                        excelTemplate: dataItem => {
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.EndDate) {
                                return "";
                            }

                            return this.moment(dataItem.CurrentPhaseObj.EndDate).format("DD-MM-YYYY");
                        },
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "ItProjectStatus", title: "Status: Samlet", width: 100,
                        persistId: "statusproj", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusTime = latestStatus.TimeStatus;
                                var statusQuality = latestStatus.QualityStatus;
                                var statusResources = latestStatus.ResourcesStatus;
                                if (latestStatus.IsCombined) {
                                    return `<span data-square-traffic-light="${latestStatus.CombinedStatus}"></span>`;
                                } else {
                                    /* If no combined status exists, take the lowest status from the splitted status */
                                    if (statusTime === "Red" || statusQuality === "Red" || statusResources === "Red") {
                                        return "<span data-square-traffic-light='Red'></span>";
                                    } else if (statusTime === "Yellow" || statusQuality === "Yellow" || statusResources === "Yellow") {
                                        return "<span data-square-traffic-light='Yellow'></span>";
                                    } else if (statusTime === "Green" || statusQuality === "Green" || statusResources === "Green") {
                                        return "<span data-square-traffic-light='Green'></span>";
                                    } else {
                                        return "<span data-square-traffic-light='White'></span>";
                                    }
                                }
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusTime = latestStatus.TimeStatus;
                                var statusQuality = latestStatus.QualityStatus;
                                var statusResources = latestStatus.ResourcesStatus;
                                if (latestStatus.IsCombined) {
                                    return `<span data-square-traffic-light="${latestStatus.CombinedStatus}"></span>`;
                                } else {
                                    /* If no combined status exists, take the lowest status from the splitted status */
                                    if (statusTime === "Red" || statusQuality === "Red" || statusResources === "Red") {
                                        return "<span data-square-traffic-light='Red'></span>";
                                    } else if (statusTime === "Yellow" || statusQuality === "Yellow" || statusResources === "Yellow") {
                                        return "<span data-square-traffic-light='Yellow'></span>";
                                    } else if (statusTime === "Green" || statusQuality === "Green" || statusResources === "Green") {
                                        return "<span data-square-traffic-light='Green'></span>";
                                    } else {
                                        return "<span data-square-traffic-light='White'></span>";
                                    }
                                }
                            } else {
                                return "";
                            }
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "ItProjectTimeStatus", title: "Status: Tid", width: 100,
                        persistId: "statusproj", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : latestStatus.TimeStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : latestStatus.TimeStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "ItProjectQualityStatus", title: "Status: Kvalitet", width: 100,
                        persistId: "statusproj", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : latestStatus.QualityStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : latestStatus.QualityStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "ItProjectResourcesStatus", title: "Status: Ressourcer", width: 100,
                        persistId: "statusproj", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : latestStatus.ResourcesStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : latestStatus.ResourcesStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "StatusDate", title: "Status projekt: Dato", format: "{0:dd-MM-yyyy}", width: 130,
                        persistId: "statusdateproj", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => {
                            if (!dataItem || !dataItem.StatusDate) {
                                return "";
                            }

                            return this.moment(dataItem.StatusDate).format("DD-MM-YYYY");
                        },
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
                        field: "Assignments", title: "Opgaver", width: 150,
                        persistId: "assignments", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => this._.filter(dataItem.ItProjectStatuses, n => this._.includes(n["@odata.type"], "Assignment")).length.toString(),
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "GoalStatus.Status", title: "Status Mål", width: 150,
                        persistId: "goalstatus", // DON'T YOU DARE RENAME!
                        template: dataItem => `<span data-square-traffic-light="${dataItem.GoalStatus.Status}"></span>`,
                        excelTemplate: dataItem => dataItem && dataItem.GoalStatus && dataItem.GoalStatus.Status.toString() || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "eq"
                            }
                        },
                        values: [
                            { text: "Hvid", value: 0 },
                            { text: "Rød", value: 1 },
                            { text: "Gul", value: 2 },
                            { text: "Grøn", value: 3 }
                        ]
                    },
                    {
                        field: "IsTransversal", title: "Tværgående", width: 150,
                        persistId: "trans", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => dataItem.IsTransversal ? `<i class="text-success fa fa-check"></i>` : `<i class="text-danger fa fa-times"></i>`,
                        excelTemplate: dataItem => dataItem && dataItem.IsTransversal.toString() || ""
                    },
                    {
                        field: "IsStrategy", title: "Strategisk", width: 150,
                        persistId: "strat", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => dataItem.IsStrategy ? `<i class="text-success fa fa-check"></i>` : `<i class="text-danger fa fa-times"></i>`,
                        excelTemplate: dataItem => dataItem && dataItem.IsStrategy.toString() || ""
                    },
                    {
                        field: "EconomyYears", title: "Økonomi", width: 150,
                        persistId: "eco", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => {
                            var total = 0;
                            this._.forEach(dataItem.EconomyYears, eco => {
                                total += this.economyCalc.getTotalBudget(eco);
                            });
                            return (-total).toString();
                        },
                        filterable: false,
                        sortable: false
                    }
                ]
            };
            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ''
                });
            }
            // find the index of column where the role columns should be inserted
            var insertIndex = this._.findIndex(mainGridOptions.columns, { 'persistId': 'orgunit' }) + 1;

            // add a role column for each of the roles
            // note iterating in reverse so we don't have to update the insert index
            this._.forEachRight(this.projectRoles, role => {
                var roleColumn: IKendoGridColumn<IItProjectInactiveOverview> = {
                    field: `role${role.Id}`,
                    title: role.Name,
                    persistId: `role${role.Id}`,
                    template: dataItem => {
                        var roles = "";

                        if (dataItem.roles[role.Id] === undefined)
                            return roles;

                        roles = this.concatRoles(dataItem.roles[role.Id]);

                        var link = `<a data-ui-sref='it-project.edit.roles({id: ${dataItem.Id}})'>${roles}</a>`;

                        return link;
                    },
                    excelTemplate: dataItem => {
                        var roles = "";

                        if (!dataItem || dataItem.roles[role.Id] === undefined)
                            return roles;

                        return this.concatRoles(dataItem.roles[role.Id]);
                    },
                    width: 215,
                    hidden: !(role.Name === "Projektleder"), // hardcoded role name :(
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

        private economyTemplate = (dataItem) => {
            var total = 0;
            this._.forEach(dataItem.EconomyYears, eco => {
                total += this.economyCalc.getTotalBudget(eco);
            });
            return -total;
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

        public roleSelectorOptions = () => {
            return {
                autoBind: false,
                dataSource: this.projectRoles,
                dataTextField: "Name",
                dataValueField: "Id",
                optionLabel: "Vælg projektrolle...",
                change: e => {
                    // hide all roles column
                    this._.forEach(this.projectRoles, role => {
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

        private exportFlag = false;
        private exportToExcel = (e: IKendoGridExcelExportEvent<IItProjectInactiveOverview>) => {
            var columns = e.sender.columns;

            if (!this.exportFlag) {
                e.preventDefault();
                this._.forEach(columns, kendoColumn => {
                    var column = kendoColumn;
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
                this._.forEach(columns, kendoColumn => {
                    var column = kendoColumn;

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
    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-project.overview-inactive", {
                    url: "/overview-inactive",
                    templateUrl: "app/components/it-project/it-project-overview.view.html",
                    controller: OverviewInactiveController,
                    controllerAs: "projectOverviewVm",
                    resolve: {
                        projectRoles: [
                            "$http", $http => $http.get("odata/LocalItProjectRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        orgUnits: [
                            "$http", "user", "_", ($http, user, _) => $http.get(`/odata/Organizations(${user.currentOrganizationId})/OrganizationUnits`)
                                .then(result => _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId"))
                        ]
                    }
                });
            }
        ]);
}

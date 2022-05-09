﻿module Kitos.ItProject.Overview {
    "use strict";

    export interface IOverviewController {
        mainGrid: Kitos.IKendoGrid<IItProjectOverview>;
        mainGridOptions: kendo.ui.GridOptions;
        roleSelectorOptions: any;
        saveGridProfile(): void;
        loadGridProfile(): void;
        clearGridProfile(): void;
        doesGridProfileExist(): boolean;
        clearOptions(): void;
    }

    export interface IItProjectOverview extends Models.ItProject.IItProject {
        CurrentPhaseObj: Models.ItProject.IItProjectPhase;
        roles: Array<any>;
        Rights: Array<any>;
        OriginalEntity: any;
    }

    export class OverviewController implements IOverviewController {
        private storageKey = "it-project-overview-options";
        private orgUnitStorageKey = "it-project-overview-orgunit";
        private gridState = this.gridStateService.getService(this.storageKey, this.user);
        public mainGrid: Kitos.IKendoGrid<IItProjectOverview>;
        public mainGridOptions: kendo.ui.GridOptions;
        public canCreate: boolean;
        public projectIdToAccessLookup = {};

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
            "$uibModal",
            "needsWidthFixService",
            "exportGridToExcelService",
            "userAccessRights",
            "authorizationServiceFactory"
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
            private $modal,
            private needsWidthFixService,
            private exportGridToExcelService,
            private userAccessRights: Models.Api.Authorization.EntitiesAccessRightsDTO,
            private authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) {
            this.$rootScope.page.title = "IT Projekt - Overblik";

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
        public opretITProjekt() {
            var self = this;

            this.$modal.open({
                // fade in instead of slide from top, fixes strange cursor placement in IE
                // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                windowClass: 'modal fade in',
                templateUrl: 'app/components/it-project/it-project-modal-create.view.html',
                controller: ['$scope', '$uibModalInstance', function ($scope, $modalInstance) {
                    $scope.formData = {};
                    $scope.type = 'IT Projekt';
                    $scope.checkAvailbleUrl = 'api/itProject/';

                    $scope.saveAndProceed = function () {

                        let orgUnitId = self.user.currentOrganizationUnitId;
                        const payload = {
                            name: $scope.formData.name,
                            responsibleOrgUnitId: orgUnitId,
                            organizationId: self.user.currentOrganizationId
                        };

                        var msg = self.notify.addInfoMessage('Opretter projekt...', false);

                        self.$http.post(`api/itproject?organizationId=${self.user.currentOrganizationId}`, payload)
                            .then(function onSuccess(result: any) {
                                msg.toSuccessMessage("Et nyt projekt er oprettet!");
                                let projectId = result.data.response.id;
                                $modalInstance.close(projectId);
                                if (orgUnitId) {
                                    // add users default org unit to the new project
                                    self.$http.post(`api/itproject/${projectId}?organizationunit=${orgUnitId}&organizationId=${self.user.currentOrganizationId}`, null);
                                }
                                self.$state.go("it-project.edit.main", { id: projectId });
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette nyt projekt!");
                            });
                    };

                    $scope.save = function () {

                        let orgUnitId = self.user.currentOrganizationUnitId;
                        const payload = {
                            name: $scope.formData.name,
                            responsibleOrgUnitId: orgUnitId,
                            organizationId: self.user.currentOrganizationId
                        };

                        var msg = self.notify.addInfoMessage('Opretter projekt...', false);

                        self.$http.post(`api/itproject?organizationId=${self.user.currentOrganizationId}`, payload)
                            .then(function onSuccess(result: any) {
                                msg.toSuccessMessage("Et nyt projekt er oprettet!");
                                let projectId = result.data.response.id;
                                $modalInstance.close(projectId);
                                if (orgUnitId) {
                                    // add users default org unit to the new project
                                    self.$http.post(`api/itproject/${projectId}?organizationunit=${orgUnitId}&organizationId=${self.user.currentOrganizationId}`, null);
                                }
                                self.$state.reload();
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke oprette nyt projekt!");
                            });
                    };
                }]
            });
        };

        // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(concat(concat(c/User/Name, ' '), c/User/LastName),'foo') and c/RoleId eq {roleId})"
        private fixRoleFilter(filterUrl, roleName, roleId) {
            var pattern = new RegExp(`(\\w+\\()${roleName}(,.*?\\))`, "i");
            var newurl = filterUrl.replace(pattern, `Rights/any(c: $1concat(concat(c/User/Name, ' '), c/User/LastName)$2 and c/RoleId eq ${roleId})`);
            return newurl;
        }

        // saves grid state to localStorage
        private saveGridOptions = () => {
            this.gridState.saveGridOptions(this.mainGrid);
        }

        // Resets the scrollbar position
        private onPaging = () => {
            Utility.KendoGrid.KendoGridScrollbarHelper.resetScrollbarPosition(this.mainGrid);
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
        }
        public checkIfRoleIsAvailable(roleId) {
            return !_.find(this.projectRoles, (option: any) => (option.Id === parseInt(roleId, 10)));
        }

        public toggleLock(dataItem) {
            if (dataItem.OriginalEntity.hasWriteAccess) {
                dataItem.IsPriorityLocked = !dataItem.IsPriorityLocked;
            }
        }

        // loads kendo grid options from localstorage
        private loadGridOptions() {
            this.gridState.loadGridOptions(this.mainGrid);
        }

        private reload() {
            this.$state.go(".", null, { reload: true });
        }

        public isValidUrl(Url) {
            var regexp = /(http || https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
            return regexp.test(Url.toLowerCase());
        };
        private activate() {
            var self = this;
            this.canCreate = this.userAccessRights.canCreate;
            var mainGridOptions: Kitos.IKendoGridOptions<IItProjectOverview> = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                var urlParameters =
                                    `?$expand=ItProjectStatuses,Reference,Parent,ItProjectType,Rights($expand=User,Role),ResponsibleUsage($expand=OrganizationUnit),GoalStatus,EconomyYears,ItProjectStatusUpdates($orderby=Created desc;$top=1)`;
                                // if orgunit is set then the org unit filter is active
                                var orgUnitId = self.$window.sessionStorage.getItem(self.orgUnitStorageKey);
                                if (orgUnitId === null) {
                                    return `/odata/Organizations(${self.user.currentOrganizationId})/ItProjects` +
                                        urlParameters;
                                } else {
                                    return `/odata/Organizations(${self.user.currentOrganizationId
                                        })/OrganizationUnits(${orgUnitId})/ItProjects` +
                                        urlParameters;
                                }
                            },
                            dataType: "json"
                        },
                        parameterMap: (options, type) => {
                            // get kendo to map parameters to an odata url
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                self._.forEach(self.projectRoles,
                                    role => {
                                        parameterMap.$filter =
                                            self.fixRoleFilter(parameterMap.$filter, `role${role.Id}`, role.Id);
                                    });
                            }

                            return parameterMap;
                        }
                    },
                    filter: { field: "IsArchived", operator: "eq", value: false }, // default filter
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
                            },
                        },
                        parse: response => {
                            // HACK to flatten the Rights on usage so they can be displayed as single columns

                            // iterrate each project
                            let projectIds = [];
                            self._.forEach(response.value,
                                project => {
                                    projectIds.push(project.Id);
                                    self.projectIdToAccessLookup[project.Id] = { project: project }
                                    project.roles = [];
                                    // iterrate each right
                                    self._.forEach(project.Rights,
                                        right => {
                                            // init an role array to hold users assigned to this role
                                            if (!project.roles[right.RoleId])
                                                project.roles[right.RoleId] = [];

                                            // push username to the role array
                                            project.roles[right.RoleId].push(
                                                [right.User.Name, right.User.LastName].join(" "));
                                        });

                                    var phase = `Phase${project.CurrentPhase}`;
                                    project.CurrentPhaseObj = project[phase];

                                    project.hasWriteAccess = true;

                                    if (!project.Parent) {
                                        project.Parent = { Name: "" };
                                    }
                                    if (!project.ResponsibleUsage) {
                                        project.ResponsibleUsage = { OrganizationUnit: { Name: "" } }
                                    };
                                    if (!project.Reference) {
                                        project.Reference = { Title: "", ExternalReferenceId: "" };
                                    }
                                    if (!project.ItProjectType) {
                                        project.ItProjectType = { Name: "" };
                                    }
                                    if (!project.GoalStatus) {
                                        project.GoalStatus = { Status: "" };
                                    }
                                });

                            //Lazy load access rights in a batch
                            if (projectIds.length > 0) {
                                self.authorizationServiceFactory
                                    .createProjectAuthorization()
                                    .getAuthorizationForItems(projectIds)
                                    .then(accessRights => {
                                        self._.forEach(accessRights, rights => self.projectIdToAccessLookup[rights.id].project.hasWriteAccess = rights.canEdit);
                                    });
                            }

                            return response;
                        }
                    }
                },
                toolbar: [
                    {
                        name: "opretITProjekt",
                        text: "Opret IT Projekt",
                        template:
                            "<button data-element-type='createProjectButton' ng-click='projectOverviewVm.opretITProjekt()' class='btn kendo-btn-sm btn-success pull-right kendo-margin-left' data-ng-disabled=\"!projectOverviewVm.canCreate\">#: text #</button>"
                    },
                    {
                        name: "clearFilter",
                        text: "Gendan kolonneopsætning",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='projectOverviewVm.clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template:
                            '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="projectOverviewVm.saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template:
                            '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="projectOverviewVm.loadGridProfile()" data-ng-disabled="!projectOverviewVm.doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template:
                            "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='projectOverviewVm.clearGridProfile()' data-ng-disabled='!projectOverviewVm.doesGridProfileExist()'>#: text #</button>"
                    },
                    {
                        template: kendo.template(self.$("#role-selector").html())
                    }
                ],
                excel: {
                    fileName: "IT Project Overblik.xlsx",
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
                dataBound: self.saveGridOptions,
                columnResize: self.saveGridOptions,
                columnHide: self.saveGridOptions,
                columnShow: self.saveGridOptions,
                columnReorder: self.saveGridOptions,
                excelExport: (e:any) => self.exportToExcel(e),
                page: self.onPaging,
                columns: [
                    {
                        field: "ItProjectId",
                        title: "ProjektID",
                        width: 115,
                        persistId: "projid", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => dataItem && dataItem.ItProjectId || "",
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
                        field: "Parent.Name",
                        title: "Overordnet IT Projekt",
                        width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.Parent
                            ? `<a data-ui-sref="it-project.edit.main({id:${dataItem.Parent.Id}})">${dataItem.Parent.Name
                            }</a>`
                            : "",
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
                        field: "Name",
                        title: "IT Projekt",
                        attributes: {
                            "data-element-type": "projectNameKendoObject"
                        },
                        width: 340,
                        persistId: "projname", // DON'T YOU DARE RENAME!
                        template: dataItem => `<a data-ui-sref="it-project.edit.main({id: ${dataItem.Id}})">${dataItem
                            .Name}</a>`,
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
                        field: "ResponsibleUsage.OrganizationUnit.Name",
                        title: "Ansv. organisationsenhed",
                        width: 245,
                        persistId: "orgunit", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.ResponsibleUsage &&
                            dataItem.ResponsibleUsage.OrganizationUnit.Name ||
                            "",
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: self.orgUnitDropDownList
                            }
                        }
                    },
                    {
                        field: "Reference.Title",
                        title: "Reference",
                        width: 150,
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
                        field: "Reference.ExternalReferenceId",
                        title: "Dokument ID / Sagsnr.",
                        width: 150,
                        persistId: "folder", // DON'T YOU DARE RENAME!
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
                        field: "ItProjectType.Name",
                        title: "Projekttype",
                        width: 125,
                        persistId: "projtype", // DON'T YOU DARE RENAME!
                        template: dataItem => dataItem.ItProjectType ? dataItem.ItProjectType.Name : "",
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
                        field: "CurrentPhaseObj.Name",
                        title: "Fase",
                        width: 100,
                        persistId: "phasename", // DON'T YOU DARE RENAME!
                        template: dataItem =>
                            dataItem.CurrentPhaseObj
                                ? `<a data-ui-sref="it-project.edit.phases({id:${dataItem.Id}})">${dataItem.CurrentPhaseObj
                                    .Name}</a>`
                                : "",
                        excelTemplate: dataItem =>
                            dataItem && dataItem.CurrentPhaseObj && dataItem.CurrentPhaseObj.Name || "",
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "CurrentPhaseObj.StartDate",
                        title: "Fase: Startdato",
                        format: "{0:dd-MM-yyyy}",
                        width: 85,
                        persistId: "phasestartdate", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => {
                            // handles null cases
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.StartDate) {
                                return "";
                            }

                            return self.moment(dataItem.CurrentPhaseObj.StartDate).format(Constants.DateFormat.DanishDateFormat);
                        },
                        excelTemplate: dataItem => {
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.StartDate) {
                                return "";
                            }

                            return Helpers.ExcelExportHelper.renderDate(dataItem.CurrentPhaseObj.StartDate);
                        },
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "CurrentPhaseObj.EndDate",
                        title: "Fase: Slutdato",
                        format: "{0:dd-MM-yyyy}",
                        width: 85,
                        persistId: "phaseenddate", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            // handles null cases
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.EndDate) {
                                return "";
                            }

                            return self.moment(dataItem.CurrentPhaseObj.EndDate).format(Constants.DateFormat.DanishDateFormat);
                        },
                        excelTemplate: dataItem => {
                            if (!dataItem.CurrentPhaseObj || !dataItem.CurrentPhaseObj.EndDate) {
                                return "";
                            }

                            return Helpers.ExcelExportHelper.renderDate(dataItem.CurrentPhaseObj.EndDate);
                        },
                        sortable: false,
                        filterable: false
                    },
                    {
                        field: "ItProjectStatus",
                        title: "Status: Samlet",
                        width: 100,
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
                                    } else if (statusTime === "Yellow" ||
                                        statusQuality === "Yellow" ||
                                        statusResources === "Yellow") {
                                        return "<span data-square-traffic-light='Yellow'></span>";
                                    } else if (statusTime === "Green" ||
                                        statusQuality === "Green" ||
                                        statusResources === "Green") {
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
                            if (!dataItem.ItProjectStatusUpdates) {
                                return "";
                            }
                            return Helpers.ExcelExportHelper.renderProjectStatusColor(dataItem.ItProjectStatusUpdates);
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "ItProjectTimeStatus",
                        title: "Status: Tid",
                        width: 100,
                        persistId: "statusprojtime", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined)
                                    ? latestStatus.CombinedStatus
                                    : latestStatus.TimeStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                return Helpers.ExcelExportHelper.renderStatusColorWithStatus(dataItem, dataItem.ItProjectStatusUpdates[0].TimeStatus);
                            } else {
                                return "";
                            }
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "ItProjectQualityStatus",
                        title: "Status: Kvalitet",
                        width: 100,
                        persistId: "statusprojqual", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined)
                                    ? latestStatus.CombinedStatus
                                    : latestStatus.QualityStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                return Helpers.ExcelExportHelper.renderStatusColorWithStatus(dataItem, dataItem.ItProjectStatusUpdates[0].QualityStatus);
                            } else {
                                return "";
                            }
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "ItProjectResourcesStatus",
                        title: "Status: Ressourcer",
                        width: 100,
                        persistId: "statusprojress", // DON'T YOU DARE RENAME!
                        template: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                var latestStatus = dataItem.ItProjectStatusUpdates[0];
                                var statusToShow = (latestStatus.IsCombined)
                                    ? latestStatus.CombinedStatus
                                    : latestStatus.ResourcesStatus;
                                return `<span data-square-traffic-light="${statusToShow}"></span>`;
                            } else {
                                return "";
                            }
                        },
                        excelTemplate: dataItem => {
                            if (dataItem.ItProjectStatusUpdates.length > 0) {
                                return Helpers.ExcelExportHelper.renderStatusColorWithStatus(dataItem, dataItem.ItProjectStatusUpdates[0].ResourcesStatus);
                            } else {
                                return "";
                            }
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "StatusDate",
                        title: "Status projekt: Dato",
                        format: "{0:dd-MM-yyyy}",
                        width: 130,
                        persistId: "statusdateproj", // DON'T YOU DARE RENAME!
                        excelTemplate: dataItem => {
                            if (!dataItem || !dataItem.StatusDate) {
                                return "";
                            }
                            return Helpers.ExcelExportHelper.renderDate(dataItem.StatusDate);
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
                        field: "Assignments",
                        title: "Opgaver",
                        width: 150,
                        persistId: "assignments", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => self._.filter(dataItem.ItProjectStatuses,
                            n => self._.includes(n["@odata.type"], "Assignment")).length.toString(),
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "GoalStatus.Status",
                        title: "Status Mål",
                        width: 150,
                        persistId: "goalstatus", // DON'T YOU DARE RENAME!
                        template: dataItem => `<span data-square-traffic-light="${dataItem.GoalStatus.Status}"></span>`,
                        excelTemplate: dataItem => {
                            if (!dataItem.GoalStatus) {
                                return "";
                            }
                            return Helpers.ExcelExportHelper.getGoalStatus(dataItem.GoalStatus.Status);
                        },
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
                        field: "IsTransversal",
                        title: "Tværgående",
                        width: 150,
                        persistId: "trans", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => dataItem.IsTransversal
                            ? `<i class="text-success fa fa-check"></i>`
                            : `<i class="text-danger fa fa-times"></i>`,
                        excelTemplate: dataItem => dataItem && dataItem.IsTransversal.toString() || ""
                    },
                    {
                        field: "IsStrategy",
                        title: "Strategisk",
                        width: 150,
                        persistId: "strat", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => dataItem.IsStrategy
                            ? `<i class="text-success fa fa-check"></i>`
                            : `<i class="text-danger fa fa-times"></i>`,
                        excelTemplate: dataItem => dataItem && dataItem.IsStrategy.toString() || ""
                    },
                    {
                        field: "EconomyYears",
                        title: "Økonomi",
                        width: 150,
                        persistId: "eco", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: dataItem => {
                            var total = 0;
                            self._.forEach(dataItem.EconomyYears,
                                eco => {
                                    total += self.economyCalc.getTotalBudget(eco);
                                });
                            return (-total).toString();
                        },
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "Priority",
                        title: "Prioritet: Projekt",
                        width: 120,
                        persistId: "priority", // DON'T YOU DARE RENAME!
                        template: (dataItem) => {
                            dataItem.OriginalEntity = self.projectIdToAccessLookup[dataItem.Id].project;
                            return `<select data-ng-model="dataItem.Priority" data-autosave="api/itproject/{{dataItem.Id}}" data-field="priority" data-ng-disabled="dataItem.IsPriorityLocked || !dataItem.OriginalEntity.hasWriteAccess">
                                                    <option value="None">-- Vælg --</option>
                                                    <option value="High">Høj</option>
                                                    <option value="Mid">Mellem</option>
                                                    <option value="Low">Lav</option>
                                                </select>`;
                        },
                        excelTemplate: dataItem => dataItem && dataItem.Priority.toString() || "",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "eq"
                            }
                        },
                        values: [
                            { text: "Ingen", value: "None" },
                            { text: "Lav", value: "Low" },
                            { text: "Mellem", value: "Mid" },
                            { text: "Høj", value: "High" }
                        ]
                    },
                    {
                        field: "PriorityPf",
                        title: "Prioritet: Portefølje",
                        width: 150,
                        persistId: "prioritypf", // DON'T YOU DARE RENAME!
                        template: (dataItem) => {
                            dataItem.OriginalEntity = self.projectIdToAccessLookup[dataItem.Id].project;
                            return `<div class="btn-group btn-group-sm" data-toggle="buttons">
                                                    <label class="btn btn-star" data-ng-class="{ 'unstarred': !dataItem.IsPriorityLocked, 'disabled': !dataItem.OriginalEntity.hasWriteAccess }" data-ng-click="projectOverviewVm.toggleLock(dataItem)">
                                                        <input type="checkbox" data-ng-model="dataItem.IsPriorityLocked" data-autosave="api/itproject/{{dataItem.Id}}" data-field="IsPriorityLocked" data-ng-disabled="!dataItem.OriginalEntity.hasWriteAccess">
                                                        <i class="glyphicon glyphicon-lock"></i>
                                                    </label>
                                                </div>
                                                <select data-ng-model="dataItem.PriorityPf" data-autosave="api/itproject/{{dataItem.Id}}" data-field="priorityPf" data-ng-disabled="!dataItem.OriginalEntity.hasWriteAccess">
                                                    <option value="None">-- Vælg --</option>
                                                    <option value="High">Høj</option>
                                                    <option value="Mid">Mellem</option>
                                                    <option value="Low">Lav</option>
                                                </select>`;
                        },
                        excelTemplate: dataItem => dataItem && dataItem.PriorityPf.toString() || "",
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "eq"
                            }
                        },
                        values: [
                            { text: "Ingen", value: 0 },
                            { text: "Lav", value: 1 },
                            { text: "Mellem", value: 2 },
                            { text: "Høj", value: 3 }
                        ]
                    },
                    {
                        // filtering doesn't allow to sort on an array of values, it needs a single value for each row...
                        field: "Rights.Role", title: `${self.user.fullName}`, width: 150,
                        persistId: "usersRoles", // DON'T YOU DARE RENAME!
                        template: (dataItem) => {
                            const rightsInProject = self.getCurrentUserRights(dataItem);
                            var result = "";
                            rightsInProject.forEach(right => {
                                result = (result === "" ? result : (result + ", ")) + right.Role.Name + (self.checkIfRoleIsAvailable(right.Role.Id) ? "(udgået)" : "");
                            });
                            return result;
                        },
                        excelTemplate: dataItem => {
                            const rightsInProject = self.getCurrentUserRights(dataItem);
                            return Helpers.ExcelExportHelper.renderUserRoles(rightsInProject, self.projectRoles);
                        },
                        attributes: { "class": "might-overflow" },
                        hidden: true,
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
            var insertIndex = this._.findIndex(mainGridOptions.columns, { 'persistId': 'orgunit' }) + 1;

            // add a role column for each of the roles
            // note iterating in reverse so we don't have to update the insert index
            this._.forEachRight(this.projectRoles, role => {
                var roleColumn: IKendoGridColumn<IItProjectOverview> = {
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
                    width: 135,
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

            Helpers.ExcelExportHelper.setupExcelExportDropdown(() => this.excelConfig,
                () => this.mainGrid,
                this.$scope,
                this.mainGridOptions.toolbar);
        }

        private getCurrentUserRights(project: IItProjectOverview) {
            var self = this;
            return _.filter(project.Rights, right => {
                return right.User.Id == self.user.id;
            });
        }

        private orgUnitDropDownList = args => {
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

        private readonly excelConfig: Models.IExcelConfig = {
        };

        private exportToExcel = (e: IKendoGridExcelExportEvent<Models.ItProject.IItProject>) => {
            this.exportGridToExcelService.getExcel(e, this._, this.$timeout, this.mainGrid, this.excelConfig);
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

    }

    angular
        .module("app")
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-project.overview", {
                    url: "/overview",
                    templateUrl: "app/components/it-project/it-project-overview.view.html",
                    controller: OverviewController,
                    controllerAs: "projectOverviewVm",
                    resolve: {
                        projectRoles: [
                            "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                                localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItProjectRoles).getAll()
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        orgUnits: [
                            "$http", "user", "_", ($http, user, _) => $http.get(`/odata/Organizations(${user.currentOrganizationId})/OrganizationUnits`)
                                .then(result => _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId"))
                        ],
                        userAccessRights: ["authorizationServiceFactory", (authorizationServiceFactory: Services.Authorization.IAuthorizationServiceFactory) =>
                            authorizationServiceFactory
                                .createProjectAuthorization()
                                .getOverviewAuthorization()
                        ],
                    }
                });
            }
        ]);
}

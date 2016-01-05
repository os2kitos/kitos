(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.overview-inactive', {
            url: '/overview-inactive',
            templateUrl: 'partials/it-project/overview.html',
            controller: 'project.EditOverviewInactiveCtrl',
            resolve: {
                projectRoles: ['$http', function ($http) {
                    return $http.get('/odata/ItProjectRoles').then(function(result) {
                        return result.data.value;
                    });
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                orgUnits: [
                    '$http', 'user', '_', function ($http, user, _) {
                        return $http.get("/odata/Organizations(" + user.currentOrganizationId + ")/OrganizationUnits").then(function (result) {
                            return _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId");
                        });
                    }
                ]
            }
        });
    }]);

    app.controller('project.EditOverviewInactiveCtrl',
    ['$rootScope', '$scope', '$http', '$timeout', '_', 'moment', '$state', 'notify', 'projectRoles', 'user', 'gridStateService', 'orgUnits', '$q', 'economyCalc',
        function ($rootScope, $scope, $http, $timeout, _, moment, $state, notify, projectRoles, user, gridStateService, orgUnits, $q, economyCalc) {
            $rootScope.page.title = "IT Projekt - Overblik";

            var storageKey = "it-project-overview-inactive-options";
            var orgUnitStorageKey = "it-project-overview-inactive-orgunit";
            var gridState = gridStateService.getService(storageKey);

            // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(c/User/Name,'foo') and c/RoleId eq {roleId})"
            function fixRoleFilter(filterUrl, roleName, roleId) {
                var pattern = new RegExp("(\\w+\\()" + roleName + "(.*?\\))", "i");
                return filterUrl.replace(pattern, "Rights/any(c: $1c/User/Name$2 and c/RoleId eq " + roleId + ")");
            }

            // saves grid state to localStorage
            function saveGridOptions() {
                gridState.saveGridOptions($scope.mainGrid);
            }

            // loads kendo grid options from localstorage
            function loadGridOptions() {
                var selectedOrgUnitId = sessionStorage.getItem(orgUnitStorageKey);
                var selectedOrgUnit = _.find(orgUnits, function (orgUnit) {
                    return orgUnit.Id == selectedOrgUnitId;
                });

                var filter = undefined;
                // if selected is a root then no need to filter as it should display everything anyway
                if (selectedOrgUnit && selectedOrgUnit.$level != 0) {
                    filter = getFilterWithOrgUnit({}, selectedOrgUnitId, selectedOrgUnit.childIds);
                }

                gridState.loadGridOptions($scope.mainGrid, filter);
            }

            $scope.saveGridProfile = function () {
                // the stored org unit id must be the current
                var currentOrgUnitId = sessionStorage.getItem(orgUnitStorageKey);
                localStorage.setItem(orgUnitStorageKey + "-profile", currentOrgUnitId);

                gridState.saveGridProfile($scope.mainGrid);
                notify.addSuccessMessage("Filtre og sortering gemt");
            };

            $scope.loadGridProfile = function () {
                gridState.loadGridProfile($scope.mainGrid);

                var orgUnitId = localStorage.getItem(orgUnitStorageKey + "-profile");
                // update session
                sessionStorage.setItem(orgUnitStorageKey, orgUnitId);
                // find the org unit filter row section
                var orgUnitFilterRow = $(".k-filter-row [data-field='ResponsibleUsage.OrganizationUnit.Name']");
                // find the access modifier kendo widget
                var orgUnitFilterWidget = orgUnitFilterRow.find("input").data("kendoDropDownList");
                orgUnitFilterWidget.select(function (dataItem) {
                    return dataItem.Id == orgUnitId;
                });

                $scope.mainGrid.dataSource.read();
                notify.addSuccessMessage("Anvender gemte filtre og sortering");
            };

            $scope.clearGridProfile = function () {
                sessionStorage.removeItem(orgUnitStorageKey);
                gridState.removeProfile();
                gridState.removeSession();
                notify.addSuccessMessage("Filtre og sortering slettet");
                reload();
            };

            $scope.doesGridProfileExist = function () {
                return gridState.doesGridProfileExist();
            };

            // clears grid filters by removing the localStorageItem and reloading the page
            $scope.clearOptions = function () {
                localStorage.removeItem(orgUnitStorageKey + "-profile");
                sessionStorage.removeItem(orgUnitStorageKey);
                gridState.removeProfile();
                gridState.removeLocal();
                gridState.removeSession();
                notify.addSuccessMessage("Sortering, filtering og kolonnevisning, -bredde og –rækkefølge nulstillet");
                // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
                reload();
            };

            function reload() {
                $state.go('.', null, { reload: true });
            }

            $scope.$on("kendoWidgetCreated", function (event, widget) {
                // the event is emitted for every widget; if we have multiple
                // widgets in this controller, we need to check that the event
                // is for the one we're interested in.
                if (widget === $scope.mainGrid) {
                    loadGridOptions();
                    $scope.mainGrid.dataSource.read();
                }
            });

            var mainGridOptions = {
                autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItProjects?$expand=ItProjectStatuses,Parent,ItProjectType,Rights($expand=User,Role),ResponsibleUsage($expand=OrganizationUnit),GoalStatus,EconomyYears",
                            dataType: "json"
                        },
                        parameterMap: function(options, type) {
                            // get kendo to map parameters to an odata url
                            var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                            if (parameterMap.$filter) {
                                _.forEach(projectRoles, function(role) {
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "role" + role.Id, role.Id);
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
                                IsArchived: { type: "boolean" },
                            }
                        },
                        parse: function(response) {
                            // HACK to flatten the Rights on usage so they can be displayed as single columns

                            // iterrate each project
                            _.forEach(response.value, function(project) {
                                project.roles = [];
                                // iterrate each right
                                _.forEach(project.Rights, function(right) {
                                    // init an role array to hold users assigned to this role
                                    if (!project.roles[right.RoleId])
                                        project.roles[right.RoleId] = [];

                                    // push username to the role array
                                    project.roles[right.RoleId].push([right.User.Name, right.User.LastName].join(" "));
                                });

                                var phase = "Phase" + project.CurrentPhase;
                                project.CurrentPhaseObj = project[phase];

                                if (user.isGlobalAdmin || user.isLocalAdmin || _.find(project.Rights, { 'userId': user.id })) {
                                    project.hasWriteAccess = true;
                                }
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
                        template: "<button type='button' class='k-button k-button-icontext' title='Nulstil sortering, filtering og kolonnevisning, -bredde og –rækkefølge' data-ng-click='clearOptions()'>#: text #</button>"
                    },
                    {
                        name: "saveFilter",
                        text: "Gem filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Gem filtre og sortering" data-ng-click="saveGridProfile()">#: text #</button>'
                    },
                    {
                        name: "useFilter",
                        text: "Anvend filter",
                        template: '<button type="button" class="k-button k-button-icontext" title="Anvend gemte filtre og sortering" data-ng-click="loadGridProfile()" data-ng-disabled="!doesGridProfileExist()">#: text #</button>'
                    },
                    {
                        name: "deleteFilter",
                        text: "Slet filter",
                        template: "<button type='button' class='k-button k-button-icontext' title='Slet filtre og sortering' data-ng-click='clearGridProfile()' data-ng-disabled='!doesGridProfileExist()'>#: text #</button>"
                    },
                    {
                        template: kendo.template($("#role-selector").html())
                    }
                ],
                excel: {
                    fileName: "IT System Overblik.xlsx",
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
                    mode: "row",
                    messages: {
                        isTrue: "✔",
                        isFalse: "✖"
                    }
                },
                groupable: false,
                columnMenu: {
                    filterable: false
                },
                dataBound: saveGridOptions,
                columnResize: saveGridOptions,
                columnHide: saveGridOptions,
                columnShow: saveGridOptions,
                columnReorder: saveGridOptions,
                excelExport: exportToExcel,
                error: function(e) {
                    console.log(e);
                },
                columns: [
                    {
                        field: "ItProjectId", title: "ProjektID", width: 115,
                        persistId: "projid", // DON'T YOU DARE RENAME!
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
                        field: "Parent.Name", title: "Overordnet IT Projekt", width: 150,
                        persistId: "parentname", // DON'T YOU DARE RENAME!
                        template: "#= Parent ? '<a data-ui-sref=\"it-project.edit.status-project({id:' + Parent.Id + '})\">' + Parent.Name + '</a>' : '' #",
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
                        field: "Name", title: "IT Projekt", width: 340,
                        persistId: "projname", // DON'T YOU DARE RENAME!
                        template: '<a data-ui-sref="it-project.edit.status-project({id: #: Id #})">#: Name #</a>',
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "ResponsibleUsage.OrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 245,
                        persistId: "orgunit", // DON'T YOU DARE RENAME!
                        template: "#: ResponsibleUsage ? ResponsibleUsage.OrganizationUnit.Name : '' #",
                        filterable: {
                            cell: {
                                showOperators: false,
                                template: orgUnitDropDownList
                            }
                        }
                    },
                    // MySQL ERROR: String was not recognized as a valid Boolean
                    //{
                    //    field: "ItSystemUsages.ItSystem.Name", title: "IT System", width: 150,
                    //    persistId: "sysnames", // DON'T YOU DARE RENAME!
                    //    template: "#: Parent ? Parent.Name : '' #",
                    //    hidden: true,
                    //    filterable: {
                    //        cell: {
                    //            dataSource: [],
                    //            showOperators: false,
                    //            operator: "contains"
                    //        }
                    //    }
                    //},
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
                        persistId: "folder", // DON'T YOU DARE RENAME!
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
                        field: "ItProjectType.Name", title: "Projekttype", width: 125,
                        persistId: "projtype", // DON'T YOU DARE RENAME!
                        template: "#: ItProjectType ? ItProjectType.Name : '' #",
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    },
                    {
                        field: "", title: "Fase", width: 100,
                        persistId: "phasename", // DON'T YOU DARE RENAME!
                        template: "#= CurrentPhaseObj ? '<a data-ui-sref=\"it-project.edit.phases({id:' + Id + '})\">' + CurrentPhaseObj.Name + '</a>'  : '' #",
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "", title: "Fase: Startdato", format: "{0:dd-MM-yyyy}", width: 85,
                        persistId: "phasestartdate", // DON'T YOU DARE RENAME!
                        template: phaseStartDateTemplate,
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "", title: "Fase: Slutdato", format: "{0:dd-MM-yyyy}", width: 85,
                        persistId: "phaseenddate", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: phaseEndDateTemplate,
                        sortable: false,
                        filterable: false,
                    },
                    {
                        field: "StatusDate", title: "Status projekt: Dato", format: "{0:dd-MM-yyyy}", width: 130,
                        persistId: "statusdateproj", // DON'T YOU DARE RENAME!
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
                        field: "", title: "Opgaver", width: 150,
                        persistId: "assignments", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: assigmentTemplate,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "GoalStatus.Status", title: "Status Mål", width: 150,
                        persistId: "goalstatus", // DON'T YOU DARE RENAME!
                        template: '<span data-square-traffic-light="#: GoalStatus.Status #"></span>',
                        hidden: true,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "eq",
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
                        template: "#= IsTransversal ? '<i class=\"text-success fa fa-check\"></i>' : '<i class=\"text-danger fa fa-times\"></i>' #",
                    },
                    {
                        field: "IsStrategy", title: "Strategisk", width: 150,
                        persistId: "strat", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: "#= IsStrategy ? '<i class=\"text-success fa fa-check\"></i>' : '<i class=\"text-danger fa fa-times\"></i>' #",
                    },
                    {
                        field: "EconomyYears", title: "Økonomi", width: 150,
                        persistId: "eco", // DON'T YOU DARE RENAME!
                        hidden: true,
                        template: economyTemplate,
                    },
                ]
            };

            function activate() {
                // find the index of column where the role columns should be inserted
                var insertIndex = _.findIndex(mainGridOptions.columns, "persistId", "orgunit") + 1;

                // add a role column for each of the roles
                // note iterating in reverse so we don't have to update the insert index
                _.forEachRight(projectRoles, function (role) {
                    var roleColumn = {
                        field: "role" + role.Id,
                        title: role.Name,
                        persistId: "role" + role.Id,
                        template: function (dataItem) {
                            return roleTemplate(dataItem, role.Id);
                        },
                        width: 135,
                        hidden: role.Name == "Projektleder" ? false : true, // hardcoded role name :(
                        sortable: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains",
                            }
                        }
                    };

                    // insert the generated column at the correct location
                    mainGridOptions.columns.splice(insertIndex, 0, roleColumn);
                });

                // assign the generated grid options to the scope value, kendo will do the rest
                $scope.mainGridOptions = mainGridOptions;
            }
            activate();

            function economyTemplate(dataItem) {
                var total = 0;
                _.each(dataItem.EconomyYears, function (eco) {
                    total += economyCalc.getTotalBudget(eco);
                });
                return -total;
            }

            function roleTemplate(dataItem, roleId) {
                var roles = "";

                if (dataItem.roles[roleId] === undefined)
                    return roles;

                // join the first 5 username together
                if (dataItem.roles[roleId].length > 0)
                    roles = dataItem.roles[roleId].slice(0, 4).join(", ");

                // if more than 5 then add an elipsis
                if (dataItem.roles[roleId].length > 5)
                    roles += ", ...";

                var link = "<a data-ui-sref='it-system.usage.roles({id: " + dataItem.Id + "})'>" + roles + "</a>";

                return link;
            }

            function phaseStartDateTemplate(dataItem) {
                if (dataItem.CurrentPhaseObj) {
                    if (dataItem.CurrentPhaseObj.StartDate) {
                        return moment(dataItem.CurrentPhaseObj.StartDate).format("DD-MM-YYYY");
                    }
                }

                return "";
            }

            function phaseEndDateTemplate(dataItem) {
                if (dataItem.CurrentPhaseObj) {
                    if (dataItem.CurrentPhaseObj.EndDate) {
                        return moment(dataItem.CurrentPhaseObj.EndDate).format("DD-MM-YYYY");
                    }
                }

                return "";
            }

            function assigmentTemplate(dataItem) {
                var res = _.filter(dataItem.ItProjectStatuses, function (n) {
                    return _.contains(n["@odata.type"], "Assignment");
                });

                return res.length;
            }

            function orgUnitDropDownList(args) {
                function indent(dataItem) {
                    var htmlSpace = "&nbsp;&nbsp;&nbsp;&nbsp;";
                    return htmlSpace.repeat(dataItem.$level) + dataItem.Name;
                }

                function setDefaultOrgUnit() {
                    var kendoElem = this;
                    var idTofind = sessionStorage.getItem(orgUnitStorageKey);

                    if (!idTofind) {
                        // if no id was found then do nothing
                        return;
                    }

                    // find the index of the org unit that matches the users default org unit
                    var index = _.findIndex(kendoElem.dataItems(), function (item) {
                        return item.Id == idTofind;
                    });

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
                    var dataSource = $scope.mainGrid.dataSource;
                    var currentFilter = dataSource.filter();
                    var selectedIndex = kendoElem.select();
                    var selectedId = _.parseInt(kendoElem.value());
                    var childIds = kendoElem.dataItem().childIds;

                    sessionStorage.setItem(orgUnitStorageKey, selectedId);

                    if (selectedIndex > 0) {
                        // filter by selected
                        dataSource.filter(getFilterWithOrgUnit(currentFilter, selectedId, childIds));
                    } else {
                        // else clear filter because the 0th element should act like a placeholder
                        dataSource.filter(getFilterWithOrgUnit(currentFilter));
                    }
                }

                // http://dojo.telerik.com/ODuDe/5
                args.element.removeAttr("data-bind");
                args.element.kendoDropDownList({
                    dataSource: orgUnits,
                    dataValueField: "Id",
                    dataTextField: "Name",
                    template: indent,
                    dataBound: setDefaultOrgUnit,
                    change: orgUnitChanged
                });
            }

            function getFilterWithOrgUnit(currentFilter, selectedId, childIds) {
                var field = "ResponsibleUsage.OrganizationUnit.Id";
                // remove old values first
                var newFilter = _.removeFiltersForField(currentFilter, field);

                // is selectedId a number?
                if (!isNaN(selectedId)) {
                    newFilter = _.addFilter(newFilter, field, "eq", selectedId, "or");
                    // add children to filters
                    _.forEach(childIds, function (id) {
                        newFilter = _.addFilter(newFilter, field, "eq", id, "or");
                    });
                }
                return newFilter;
            }

            $scope.roleSelectorOptions = {
                autoBind: false,
                dataSource: projectRoles,
                dataTextField: "Name",
                dataValueField: "Id",
                optionLabel: "Vælg projektrolle...",
                change: function (e) {
                    // hide all roles column
                    _.forEach(projectRoles, function (role) {
                        $scope.mainGrid.hideColumn("role" + role.Id);
                    });

                    var selectedId = e.sender.value();
                    var gridFieldName = "role" + selectedId;
                    // show only the selected role column
                    $scope.mainGrid.showColumn(gridFieldName);
                }
            };

            $scope.archiveFilterSelectorOptions = {
                autoBind: false,
                dataSource: ["Vis kun aktive", "Vis kun arkiverede"],
                optionLabel: "Vælg arkiveret filter...",
                change: function (e) {
                    var selectedText = e.sender.value();

                    var showArchived;
                    if (selectedText === "Vis kun aktive") {
                        showArchived = false;
                    } else if (selectedText === "Vis kun arkiverede") {
                        showArchived = true;
                    } else {
                        return;
                    }

                    var field = "IsArchived";
                    var dataSource = $scope.mainGrid.dataSource;
                    var currentFilter = dataSource.filter();
                    // remove old values first
                    var newFilter = _.removeFiltersForField(currentFilter, field);
                    // add new filter
                    newFilter = _.addFilter(newFilter, field, "eq", showArchived, "or");
                    // set it on the data source
                    dataSource.filter(newFilter);
                }
            };

            var exportFlag = false;
            function exportToExcel(e) {
                var columns = e.sender.columns;

                if (!exportFlag) {
                    e.preventDefault();
                    _.forEach(columns, function (column) {
                        if (column.hidden) {
                            column.tempVisual = true;
                            e.sender.showColumn(column);
                        }
                    });
                    $timeout(function () {
                        exportFlag = true;
                        e.sender.saveAsExcel();
                    });
                } else {
                    exportFlag = false;
                    _.forEach(columns, function (column) {
                        if (column.tempVisual) {
                            delete column.tempVisual;
                            e.sender.hideColumn(column);
                        }
                    });
                }
            }























            $scope.pagination = {
                search: '',
                skip: 0,
                take: 25,
                orderBy: 'Name'
            };

            $scope.csvUrl = 'api/itProject?csv&orgId=' + user.currentOrganizationId;

            $scope.projects = [];
            $scope.projectRoles = projectRoles;

            $scope.$watchCollection('pagination', function (newVal, oldVal) {
                loadProjects();
            });

            function loadProjects() {
                var deferred = $q.defer();

                var url = 'api/itProject?overview&orgId=' + user.currentOrganizationId;

                url += '&skip=' + $scope.pagination.skip;
                url += '&take=' + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += "&q=";

                $scope.projects = [];
                $http.get(url).success(function (result, status, headers) {

                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;

                    setCanEdit(result.response).then(function(canEditResult) {
                        _.each(canEditResult, pushProject);
                    });

                }).error(function () {
                    notify.addErrorMessage("Kunne ikke hente projekter!");
                });
            }

            function setCanEdit(projectCollection) {
                return $q.all(_.map(projectCollection, function(iteratee) {
                    var deferred = $q.defer();

                    setTimeout(function() {
                        $http.get("api/itProject/" + iteratee.id + "?hasWriteAccess" + '&organizationId=' + user.currentOrganizationId)
                            .success(function(result) {
                                iteratee.canBeEdited = result.response;
                                deferred.resolve(iteratee);
                            })
                            .error(function(result) {
                                iteratee.canBeEdited = false;
                                deferred.reject(result);
                                }
                            );
                    }, 0);

                    return deferred.promise;
                }));
            }

            function pushProject(project) {
                // Due to https://github.com/angular/angular.js/blob/master/CHANGELOG.md#breaking-changes-8
                // we have to convert these values to strings
                project.priority = project.priority.toString();
                project.priorityPf = project.priorityPf.toString();

                $scope.projects.push(project);
            }
        }]);
})(angular, app);

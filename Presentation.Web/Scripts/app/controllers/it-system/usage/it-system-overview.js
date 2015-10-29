(function (ng, app) {
    app.config([
        '$stateProvider', '$urlRouterProvider', function($stateProvider) {
            $stateProvider.state('it-system.overview', {
                url: '/overview',
                templateUrl: 'partials/it-system/overview-it-system.html',
                controller: 'system.OverviewCtrl',
                resolve: {
                    user: [
                        'userService', function(userService) {
                            return userService.getUser();
                        }
                    ],
                    systemRoles: [
                        '$http', function ($http) {
                            return $http.get("/odata/ItSystemRoles").then(function (result) {
                                return result.data.value;
                            });
                        }
                    ],
                    orgUnits: [
                        '$http', 'user', '_', function ($http, user, _) {
                            return $http.get("/odata/Organizations(" + user.currentOrganizationId + ")/OrganizationUnits").then(function (result) {
                                return _.addHierarchyLevelOnFlatAndSort(result.data.value, "Id", "ParentId");
                            });
                        }
                    ]
                }
            });
        }
    ]);

    // Here be dragons! Thou art forewarned.
    // Or perhaps it's samurais, because it's kendos terrible terrible framework that's the cause...
    app.controller('system.OverviewCtrl',
        [
            '$rootScope', '$scope', '$http', '$timeout', '_', '$state', 'user', 'gridStateService', 'systemRoles', 'orgUnits', 'notify',
            function ($rootScope, $scope, $http, $timeout, _, $state, user, gridStateService, systemRoles, orgUnits, notify) {
                $rootScope.page.title = 'IT System - Overblik';

                // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(c/User/Name,'foo') and c/RoleId eq {roleId})"
                function fixRoleFilter(filterUrl, roleName, roleId) {
                    var pattern = new RegExp("(\\w+\\()" + roleName + "(.*?\\))", "i");
                    return filterUrl.replace(pattern, "Rights/any(c: $1c/User/Name$2 and c/RoleId eq " + roleId + ")");
                }

                var storageKey = "it-system-overview-options";
                var orgUnitStorageKey = "it-system-overview-orgunit";
                var gridState = gridStateService.getService(storageKey);

                // saves grid state to localStorage
                function saveGridOptions() {
                    gridState.saveGridOptions($scope.mainGrid);
                }

                // loads kendo grid options from localstorage
                function loadGridOptions() {
                    var selectedOrgUnitId = getStoredOrgUnitId();
                    var selectedOrgUnit = _.find(orgUnits, function (orgUnit) {
                        return orgUnit.Id == selectedOrgUnitId;
                    });

                    var filter = undefined;
                    // if selected is a root then no need to filter as it should display everything anyway
                    if (selectedOrgUnit.$level != 0) {
                        filter = getFilterWithOrgUnit({}, selectedOrgUnitId, selectedOrgUnit.childIds);
                    }

                    gridState.loadGridOptions($scope.mainGrid, filter);
                }

                $scope.saveGridProfile = function() {
                    gridState.saveGridProfile($scope.mainGrid);
                    notify.addSuccessMessage("Filtre og sortering gemt");
                }

                $scope.clearGridProfile = function () {
                    sessionStorage.removeItem(orgUnitStorageKey);
                    gridState.clearGridProfile($scope.mainGrid);
                    notify.addSuccessMessage("Filtre og sortering slettet");
                }

                $scope.doesGridProfileExist = function() {
                    return gridState.doesGridProfileExist();
                }

                // clears grid filters by removing the localStorageItem and reloading the page
                $scope.clearOptions = function () {
                    sessionStorage.removeItem(orgUnitStorageKey);
                    gridState.clearOptions();
                    notify.addSuccessMessage("Nulstiller tilbage til standard sortering, viste kolonner, kolonne vide og kolonne rækkefølge samt fjerner filtre");
                    // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
                    reload();
                }

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

                // overview grid options
                var mainGridOptions = {
                    autoBind: false, // disable auto fetch, it's done in the kendoRendered event handler
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystemUsages?$expand=ItSystem($expand=AppTypeOption,BusinessType,Parent,TaskRefs),Organization,ResponsibleUsage($expand=OrganizationUnit),MainContract($expand=ItContract($expand=Supplier)),Rights($expand=User,Role),ArchiveType,SensitiveDataType,ObjectOwner,LastChangedByUser,ItProjects",
                                dataType: "json"
                            },
                            parameterMap: function (options, type) {
                                // get kendo to map parameters to an odata url
                                var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                                if (parameterMap.$filter) {
                                    _.forEach(systemRoles, function (role) {
                                        parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "role" + role.Id, role.Id);
                                    });
                                }

                                return parameterMap;
                            }
                        },
                        sort: {
                            field: "ItSystem.Name",
                            dir: "asc"
                        },
                        pageSize: 100,
                        serverPaging: true,
                        serverSorting: true,
                        serverFiltering: true,
                        schema: {
                            model: {
                                fields: {
                                    LastChanged: { type: "date" }
                                }
                            },
                            parse: function (response) {
                                // HACK to flattens the Rights on usage so they can be displayed as single columns

                                // iterrate each usage
                                _.forEach(response.value, function (usage) {
                                    usage.roles = [];
                                    // iterrate each right
                                    _.forEach(usage.Rights, function (right) {
                                        // init an role array to hold users assigned to this role
                                        if (!usage.roles[right.RoleId])
                                            usage.roles[right.RoleId] = [];

                                        // push username to the role array
                                        usage.roles[right.RoleId].push(right.User.Name + " " + right.User.LastName);
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
                            template: "<button type='button' class='k-button k-button-icontext' data-ng-click='clearOptions()' title='Nulstiller tilbage til standard sortering, filter, kolonne vide og kolonne rækkefølge'>#: text #</button>"
                        },
                        {
                            name: "saveFilter",
                            text: "Gem filter",
                            template: "<button type='button' class='k-button k-button-icontext' data-ng-click='saveGridProfile()' title='Gemmer sortering og filtre'>#: text #</button>"
                        },
                        {
                            name: "deleteFilter",
                            text: "Slet filter",
                            template: "<button type='button' class='k-button k-button-icontext' data-ng-click='clearGridProfile()' data-ng-disabled='!doesGridProfileExist()'>#: text #</button>"
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
                        mode: "row"
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
                    error: function(e) {
                        console.log(e);
                    },
                    columns: [
                        {
                            field: "LocalSystemId", title: "Lokal system ID", width: 150,
                            persistId: "localid", // DON'T YOU DARE RENAME!
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
                            field: "ItSystem.Parent.Name", title: "Overordnet IT System", width: 150,
                            persistId: "parentsysname", // DON'T YOU DARE RENAME!
                            template: "#: ItSystem.Parent ? ItSystem.Parent.Name : '' #",
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
                            field: "ItSystem.Name", title: "IT System", width: 350,
                            persistId: "sysname", // DON'T YOU DARE RENAME!
                            template: "<a data-ui-sref='it-system.usage.interfaces({id: #: Id #})'>#: ItSystem.Name #</a>",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains"
                                }
                            }
                        },
                        {
                            field: "Version", title: "Version", width: 150,
                            persistId: "version", // DON'T YOU DARE RENAME!
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
                            field: "LocalCallName", title: "Lokal kaldenavn", width: 150,
                            persistId: "localname", // DON'T YOU DARE RENAME!
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
                            field: "ResponsibleUsage.OrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 250,
                            persistId: "orgunit", // DON'T YOU DARE RENAME!
                            template: "#: ResponsibleUsage ? ResponsibleUsage.OrganizationUnit.Name : '' #",
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    template: orgUnitDropDownList
                                }
                            }
                        },
                        {
                            field: "ItSystem.BusinessType.Name", title: "Forretningstype", width: 200,
                            persistId: "busitype", // DON'T YOU DARE RENAME!
                            template: "#: ItSystem.BusinessType ? ItSystem.BusinessType.Name : '' #",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ItSystem.AppTypeOption.Name", title: "Applikationstype", width: 150,
                            persistId: "apptype", // DON'T YOU DARE RENAME!
                            template: "#: ItSystem.AppTypeOption ? ItSystem.AppTypeOption.Name : '' #",
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
                            field: "ItSystem.TaskKey", title: "KLE ID", width: 150,
                            persistId: "taskkey", // DON'T YOU DARE RENAME!
                            template: "#: ItSystem.TaskRefs.length > 0 ? _.pluck(ItSystem.TaskRefs.slice(0,4), 'TaskKey').join(', ') : '' ##: ItSystem.TaskRefs.length > 5 ? ', ...' : '' #",
                            hidden: true,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "startswith",
                                }
                            },
                            sortable: false
                        },
                        {
                            field: "EsdhRef", title: "ESDH ref", width: 150,
                            persistId: "esdh", // DON'T YOU DARE RENAME!
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
                            field: "DirectoryOrUrlRef", title: "Mappe ref", width: 150,
                            persistId: "folderref", // DON'T YOU DARE RENAME!
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
                            field: "CmdbRef", title: "CMDB ref", width: 150,
                            persistId: "cmdb", // DON'T YOU DARE RENAME!
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
                            field: "ArchiveType.Name", title: "Arkivering", width: 150,
                            persistId: "archive", // DON'T YOU DARE RENAME!
                            template: "#: ArchiveType ? ArchiveType.Name : '' #",
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
                            field: "SensitiveDataType.Name", title: "Personfølsom", width: 150,
                            persistId: "sensitive", // DON'T YOU DARE RENAME!
                            template: "#: SensitiveDataType ? SensitiveDataType.Name : '' #",
                            hidden: true,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        //{
                        //    field: "", title: "IT System: Anvendes af", width: 100,
                        //    persistId: "sysusage", // DON'T YOU DARE RENAME!
                        //    template: "TODO",
                        //    filterable: false,
                        //    sortable: false
                        //},
                        //{
                        //    field: "ItSystem.ItInterfaceExhibits", title: "Snitflader: Udstilles ???", width: 95,
                        //    persistId: "exhibit", // DON'T YOU DARE RENAME!
                        //    template: "<a data-ng-click=\"showExposureDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.ItInterfaceExhibits.length #</a>",
                        //    hidden: true,
                        //    filterable: false,
                        //    sortable: false
                        //},
                        //{
                        //    field: "ItSystem.CanUseInterfaces", title: "Snitflader: Anvendes ???", width: 95,
                        //    persistId: "canuse", // DON'T YOU DARE RENAME!
                        //    template: "<a data-ng-click=\"showUsageDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.CanUseInterfaces.length #</a>",
                        //    hidden: true,
                        //    filterable: false,
                        //    sortable: false
                        //},
                        {
                            field: "MainContract", title: "Kontrakt", width: 120,
                            persistId: "contract", // DON'T YOU DARE RENAME!
                            template: contractTemplate,
                            attributes: { "class": "text-center" },
                            sortable: false,
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    template: contractFilterDropDownList
                                }
                            },
                        },
                        {
                            field: "MainContract.ItContract.Supplier.Name", title: "Leverandør", width: 180,
                            persistId: "supplier", // DON'T YOU DARE RENAME!
                            template: supplierTemplate,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "", title: "IT Projekt", width: 150,
                            persistId: "sysusage", // DON'T YOU DARE RENAME!
                            template: "#: ItProjects.length > 0 ? _.first(ItProjects).Name : '' #",
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
                            field: "ObjectOwner.Name", title: "Taget i anvendelse af", width: 150,
                            persistId: "ownername", // DON'T YOU DARE RENAME!
                            template: "#: ObjectOwner.Name + ' ' + ObjectOwner.LastName #",
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
                            field: "LastChangedByUser.Name", title: "Sidst redigeret: Bruger", width: 150,
                            persistId: "lastchangedname", // DON'T YOU DARE RENAME!
                            template: "#: LastChangedByUser.Name + ' ' + LastChangedByUser.LastName #",
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
                            field: "LastChanged", title: "Sidste redigeret: Dato", format: "{0:dd-MM-yyyy}", width: 150,
                            persistId: "changed", // DON'T YOU DARE RENAME!
                            hidden: true,
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    operator: "gte"
                                }
                            }
                        }
                    ]
                };

                function activate() {
                    // find the index of column where the role columns should be inserted
                    var insertIndex = _.findIndex(mainGridOptions.columns, "persistId", "orgunit") + 1;

                    // add a role column for each of the roles
                    // note iterating in reverse so we don't have to update the insert index
                    _.forEachRight(systemRoles, function(role) {
                        var roleColumn = {
                            field: "role" + role.Id,
                            title: role.Name,
                            persistId: "role" + role.Id,
                            template: function (dataItem) {
                                return roleTemplate(dataItem, role.Id);
                            },
                            width: 150,
                            hidden: role.Name == "Systemejer" ? false : true, // hardcoded role name :(
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

                function contractTemplate(dataItem) {
                    if (dataItem.MainContract) {
                        return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="fa fa-file-o" aria-hidden="true"></span></a>';

                        // TODO this has been disabled for now because $expand=MainContract($expand=ItContract) fails when ItContract.Terminated has a value
                        // re-enable when a workaround has been found
                        //if (dataItem.MainContract.ItContract)
                        //    if (dataItem.MainContract.ItContract.IsActive)
                        //        return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="fa fa-file text-success" aria-hidden="true"></span></a>';
                        //    else
                        //        return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="fa fa-file-o text-muted" aria-hidden="true"></span></a>';
                    }
                    return "";
                }

                function supplierTemplate(dataItem) {
                    if (dataItem.MainContract) {
                        if (dataItem.MainContract.ItContract) {
                            if (dataItem.MainContract.ItContract.Supplier) {
                                return dataItem.MainContract.ItContract.Supplier.Name;
                            }
                        }
                    }
                    return "";
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

                // show exposureDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
                $scope.showExposureDetails = function (usageId, systemName) {
                    // filter by usageId
                    $scope.exhibitGrid.dataSource.filter({ field: "ItSystemId", operator: "eq", value: usageId });
                    // set title
                    $scope.exhibitModal.setOptions({ title: systemName + " udstiller følgende snitflader" });
                    // open modal
                    $scope.exhibitModal.center().open();
                };

                $scope.exhibitDetailsGrid = {
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: "/odata/ItInterfaceExhibits?$expand=ItInterface",
                                dataType: "json"
                            }
                        },
                        serverPaging: true,
                        serverSorting: true,
                        serverFiltering: true
                    },
                    autoBind: false,
                    columns: [
                        {
                            field: "ItInterface.ItInterfaceId", title: "Snitflade ID"
                        },
                        {
                            field: "ItInterface.Name", title: "Snitflade"
                        }
                    ],
                    dataBound: exposureDetailsBound
                };

                // exposuredetails grid empty-grid handling
                function exposureDetailsBound(e) {
                    var grid = e.sender;
                    if (grid.dataSource.total() == 0) {
                        var colCount = grid.columns.length;
                        $(e.sender.wrapper)
                            .find('tbody')
                            .append('<tr class="kendo-data-row"><td colspan="' + colCount + '" class="no-data text-muted">System udstiller ikke nogle snitflader</td></tr>');
                    }
                }

                // show usageDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
                $scope.showUsageDetails = function(systemId, systemName) {
                    // filter by systemId
                    $scope.usageGrid.dataSource.filter({ field: "ItSystemId", operator: "eq", value: systemId });
                    // set modal title
                    $scope.modal.setOptions({ title: "Anvendelse af " + systemName });
                    // open modal
                    $scope.modal.center().open();
                };

                // usagedetails grid - shows which organizations has a given itsystem in local usage
                $scope.usageDetailsGrid = {
                    dataSource: {
                        type: "odata-v4",
                        transport:
                        {
                            read: {
                                url: "/odata/ItInterfaceUses/?$expand=ItInterface",
                                dataType: "json"
                            },
                        },
                        serverPaging: true,
                        serverSorting: true,
                        serverFiltering: true
                    },
                    autoBind: false,
                    columns: [
                        {
                            field: "ItInterfaceId", title: "Snitflade ID"
                        },
                        {
                            field: "ItInterface.Name", title: "Snitflade"
                        }
                    ],
                    dataBound: detailsBound
                };

                // usagedetails grid empty-grid handling
                function detailsBound(e) {
                    var grid = e.sender;
                    if (grid.dataSource.total() == 0) {
                        var colCount = grid.columns.length;
                        $(e.sender.wrapper)
                            .find('tbody')
                            .append('<tr class="kendo-data-row"><td colspan="' + colCount + '" class="no-data text-muted">System anvendens ikke</td></tr>');
                    }
                };

                function getStoredOrgUnitId() {
                    return sessionStorage.getItem(orgUnitStorageKey) ? sessionStorage.getItem(orgUnitStorageKey) : user.defaultOrganizationUnitId;
                }

                function orgUnitDropDownList(args) {
                    function indent(dataItem) {
                        var htmlSpace = "&nbsp;&nbsp;&nbsp;&nbsp;";
                        return htmlSpace.repeat(dataItem.$level) + dataItem.Name;
                    }

                    function setDefaultOrgUnit() {
                        var kendoElem = this;
                        var idTofind = getStoredOrgUnitId();

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
                            dataSource.filter(getFilterWithOrgUnit());
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

                function contractFilterDropDownList(args) {
                    var gridDataSource = args.dataSource;

                    function setContractFilter() {
                        var kendoElem = this;
                        var currentFilter = gridDataSource.filter();
                        var contractFilterObj = _.findKeyDeep(currentFilter, { field: "MainContract" });

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
                        var dataSource = $scope.mainGrid.dataSource;
                        var selectedValue = kendoElem.value();
                        var field = "MainContract";
                        var currentFilter = dataSource.filter();
                        // remove old value first
                        var newFilter = _.removeFiltersForField(currentFilter, field);

                        if (selectedValue == "Har kontrakt") {
                            newFilter = _.addFilter(newFilter, field, "ne", null, "and");
                        } else if (selectedValue == "Ingen kontrakt") {
                            newFilter = _.addFilter(newFilter, field, "eq", null, "and");
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

                $scope.roleSelectorOptions = {
                    autoBind: false,
                    dataSource: systemRoles,
                    dataTextField: "Name",
                    dataValueField: "Id",
                    optionLabel: "Vælg rolle...",
                    change: function (e) {
                        // hide all roles column
                        _.forEach(systemRoles, function(role) {
                            $scope.mainGrid.hideColumn("role" + role.Id);
                        });

                        var selectedId = e.sender.value();
                        var gridFieldName = "role" + selectedId;
                        // show only the selected role column
                        $scope.mainGrid.showColumn(gridFieldName);
                    }
                };
            }
        ]
    );
})(angular, app);

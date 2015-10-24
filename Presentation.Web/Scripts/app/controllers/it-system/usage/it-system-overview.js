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
                    ]
                }
            });
        }
    ]);

    // Here be dragons! Thou art forewarned.
    // Or perhaps it's samurais, because it's kendo that's terrible terrible framework that's the cause...
    app.controller('system.OverviewCtrl',
        [
            '$rootScope', '$scope', '$http', '$timeout', '$state', 'user', 'gridStateService',
            function ($rootScope, $scope, $http, $timeout, $state, user, gridStateService) {
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
                    gridState.loadGridOptions($scope.mainGrid);
                }

                $scope.saveGridProfile = function() {
                    gridState.saveGridProfile($scope.mainGrid);
                }

                $scope.clearGridProfile = function () {
                    sessionStorage.removeItem(orgUnitStorageKey);
                    gridState.clearGridProfile($scope.mainGrid);
                }

                // clears grid filters by removing the localStorageItem and reloading the page
                $scope.clearOptions = function () {
                    sessionStorage.removeItem(orgUnitStorageKey);
                    gridState.clearOptions();
                    // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
                    reload();
                }

                function reload() {
                    $state.go('.', null, { reload: true });
                }

                var kendoRendered = false;
                // fires when kendo is finished rendering all its goodies
                $scope.$on("kendoRendered", function () {
                    kendoRendered = true;
                    loadGridOptions();
                    $scope.mainGrid.dataSource.fetch();
                });

                // overview grid options
                $scope.mainGridOptions = {
                    autoBind: false, // do not set to true, it works because the org unit filter inits the query
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystemUsages?$expand=ItSystem($expand=AppTypeOption,BusinessType,CanUseInterfaces,ItInterfaceExhibits,Parent,TaskRefs),Organization,ResponsibleUsage($expand=OrganizationUnit),Overview($expand=ItSystem),MainContract,Rights($expand=User,Role),ArchiveType,SensitiveDataType,ObjectOwner,LastChangedByUser,ItProjects",
                                dataType: "json"
                            },
                            parameterMap: function (options, type) {
                                // get kendo to map parameters to an odata url
                                var parameterMap = kendo.data.transports["odata-v4"].parameterMap(options, type);

                                if (parameterMap.$filter) {
                                    // replaces "startswith(SystemOwner,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo' and c/RoleId eq 1)"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "SystemOwner", 1);

                                    // replaces "startswith(SystemResponsible,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 2"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "SystemResponsible", 2);

                                    // replaces "startswith(BusinessOwner,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 3"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "BusinessOwner", 3);

                                    // replaces "startswith(SuperUserResponsible,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 4"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "SuperUserResponsible", 4);

                                    // replaces "startswith(SuperUser,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 5"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "SuperUser", 5);

                                    // replaces "startswith(SecurityResponsible,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 6"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "SecurityResponsible", 6);

                                    // replaces "startswith(ChangeManager,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 7"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "ChangeManager", 7);

                                    // replaces "startswith(DataOwner,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 8"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "DataOwner", 8);

                                    // replaces "startswith(SystemAdmin,'foo')" with "Rights/any(c: startswith(c/User/Name),'foo') and RoleId eq 9"
                                    parameterMap.$filter = fixRoleFilter(parameterMap.$filter, "SystemAdmin", 9);
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
                                    LastChanged: { type: "date" },
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
                            template: "<a class='k-button k-button-icontext' data-ng-click='clearOptions()'>#: text #</a>"
                        },
                        {
                            name: "saveFilter",
                            text: "Gem filter",
                            template: "<a class='k-button k-button-icontext' data-ng-click='saveGridProfile()'>#: text #</a>"
                        },
                        {
                            name: "deleteFilter",
                            text: "Slet filter",
                            template: "<a class='k-button k-button-icontext' data-ng-click='clearGridProfile()'>#: text #</a>"
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
                            field: "ItSystem.Name", title: "IT System", width: 150,
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
                            field: "ResponsibleUsage.OrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 150,
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
                            field: "SystemOwner", title: "Systemejer",
                            persistId: "sysowner", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 1);
                            },
                            width: 150,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "SystemResponsible", title: "Systemansvarlig",
                            persistId: "sysresp", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 2);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "BusinessOwner", title: "Forretningsejer",
                            persistId: "busiowner", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 3);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "SuperUserResponsible", title: "Superbrugeransvarlig",
                            persistId: "superuserresp", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 4);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "SuperUser", title: "Superbruger",
                            persistId: "superuser", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 5);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "SecurityResponsible", title: "Sikkerhedsansvarlig",
                            persistId: "secresp", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 6);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ChangeManager", title: "Changemanager",
                            persistId: "changemanager", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 7);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "DataOwner", title: "Dataejer",
                            persistId: "dataowner", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 8);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "SystemAdmin", title: "Systemadminstrator",
                            persistId: "sysadm", // DON'T YOU DARE RENAME!
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 9);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ItSystem.BusinessType.Name", title: "Forretningstype", width: 150,
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
                        {
                            field: "", title: "IT System: Anvendes af", width: 150,
                            persistId: "sysusage", // DON'T YOU DARE RENAME!
                            template: "TODO",
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    showOperators: false,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ItSystem.ItInterfaceExhibits", title: "Snitflader: Udstilles ???", width: 95,
                            persistId: "exhibit", // DON'T YOU DARE RENAME!
                            template: "<a data-ng-click=\"showExposureDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.ItInterfaceExhibits.length #</a>",
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "ItSystem.CanUseInterfaces", title: "Snitflader: Anvendes ???", width: 95,
                            persistId: "canuse", // DON'T YOU DARE RENAME!
                            template: "<a data-ng-click=\"showUsageDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.CanUseInterfaces.length #</a>",
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "MainContract", title: "Kontrakt", width: 80,
                            persistId: "contract", // DON'T YOU DARE RENAME!
                            template: contractTemplate,
                            sortable: false,
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    template: contractFilterDropDownList
                                }
                            },
                        },
                        {
                            field: "", title: "IT Projekt", width: 150,
                            persistId: "sysusage", // DON'T YOU DARE RENAME!
                            template: "#: ItProjects.length > 0 ? _.first(ItProjects).Name : '' #",
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
                            field: "LastChanged", title: "Sidste redigeret: Dato", format: "{0:dd-MM-yyyy HH:mm}", width: 150,
                            persistId: "changed", // DON'T YOU DARE RENAME!
                            hidden: true,
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    operator: "gte"
                                }
                            }
                        }
                    ],
                    dataBound: saveGridOptions,
                    columnResize: saveGridOptions,
                    columnHide: saveGridOptions,
                    columnShow: saveGridOptions,
                    columnReorder: saveGridOptions,
                    error: function(e) {
                        console.log(e);
                    }
                };

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

                function orgUnitDropDownList(args) {
                    // check if the kendoRendered event has been called
                    // because this function is apparently called multiple
                    // times before the grid is actually ready
                    if (kendoRendered) {
                        // http://dojo.telerik.com/ODuDe/5
                        args.element.removeAttr("data-bind");
                        args.element.kendoDropDownList({
                            dataSource: {
                                type: "odata-v4",
                                transport: {
                                    read: {
                                        url: "/odata/Organizations(" + user.currentOrganizationId + ")/OrganizationUnits",
                                        dataType: "json",
                                    }
                                },
                                serverFiltering: true,
                                schema: {
                                    parse: function (response) {
                                        // add hierarchy level to each item
                                        response.value = _.addHierarchyLevelOnFlatAndSort(response.value, "Id", "ParentId");
                                        return response;
                                    }
                                }
                            },
                            dataValueField: "Id",
                            dataTextField: "Name",
                            template: indent,
                            dataBound: setDefaultOrgUnit,
                            change: orgUnitChanged
                        });
                    }
                }

                function indent(dataItem) {
                    var htmlSpace = "&nbsp;&nbsp;&nbsp;&nbsp;";
                    return htmlSpace.repeat(dataItem.$level) + dataItem.Name;
                }

                function setDefaultOrgUnit() {
                    var kendoElem = this;
                    var idTofind = sessionStorage.getItem(orgUnitStorageKey) ? sessionStorage.getItem(orgUnitStorageKey) : user.defaultOrganizationUnitId;;

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
                    var selectedIndex = kendoElem.select();
                    var selectedId = _.parseInt(kendoElem.value());
                    var childIds = kendoElem.dataItem().childIds;

                    sessionStorage.setItem(orgUnitStorageKey, selectedId);

                    if (selectedIndex > 0) {
                        // filter by selected
                        filterByOrgUnit(selectedId, childIds);
                    } else {
                        // else clear filter because the 0th element should act like a placeholder
                        filterByOrgUnit();
                    }
                }

                function filterByOrgUnit(selectedId, childIds) {
                    var dataSource = $scope.mainGrid.dataSource;
                    var field = "ResponsibleUsage.OrganizationUnit.Id";
                    var currentFilter = dataSource.filter();
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
                    // can't use datasource object directly,
                    // if we do then the view doesn't update.
                    // So have to go through $scope - sadly :(
                    dataSource.filter(newFilter);
                }

                function contractFilterDropDownList(args) {
                    // check if the kendoRendered event has been called
                    // because this function is apparently called multiple
                    // times before the grid is actually ready
                    if (kendoRendered) {
                        // http://dojo.telerik.com/ODuDe/5
                        args.element.removeAttr("data-bind");
                        args.element.kendoDropDownList({
                            dataSource: ["Har kontrakt", "Ingen kontrakt"],
                            optionLabel: "Vælg filter...",
                            dataBound: setContractFilter,
                            change: filterByContract
                        });
                    }
                }

                function filterByContract() {
                    var kendoElem = this;
                    var selectedValue = kendoElem.value();
                    var dataSource = $scope.mainGrid.dataSource;
                    var field = "MainContract";
                    var currentFilter = dataSource.filter();
                    // remove old value first
                    var newFilter = _.removeFiltersForField(currentFilter, field);

                    if (selectedValue == "Har kontrakt") {
                        newFilter = _.addFilter(newFilter, field, "ne", null, "and");
                    } else if (selectedValue == "Ingen kontrakt") {
                        newFilter = _.addFilter(newFilter, field, "eq", null, "and");
                    }
                    // can't use datasource object directly,
                    // if we do then the view doesn't update.
                    // So have to go through $scope - sadly :(
                    dataSource.filter(newFilter);
                }

                function setContractFilter() {
                    var kendoElem = this;
                    var dataSource = $scope.mainGrid.dataSource;
                    var currentFilter = dataSource.filter();
                    var contractFilterObj = _.findKeyDeep(currentFilter, { field: "MainContract" });

                    if (contractFilterObj.operator == "neq") {
                        kendoElem.select(1); // index of "Har kontrakt"
                    } else if (contractFilterObj.operator == "eq") {
                        kendoElem.select(2); // index of "Ingen kontrakt"
                    }
                }
            }
        ]
    );
})(angular, app);

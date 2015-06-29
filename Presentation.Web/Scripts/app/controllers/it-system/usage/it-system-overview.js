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

    app.controller('system.OverviewCtrl',
        [
            '$rootScope', '$scope', '$http', '$timeout', '$state', 'user', 'gridStateService',
            function ($rootScope, $scope, $http, $timeout, $state, user, gridStateService) {
                $rootScope.page.title = 'IT System - Overblik';
                
                // replaces "anything({roleName},'foo')" with "Rights/any(c: anything(c/User/Name),'foo' and c/RoleId eq {roleId})"
                function fixRoleFilter(filterUrl, roleName, roleId) {
                    var pattern = new RegExp("(\\w+\\()" + roleName + "(.*?\\))", "i");
                    return filterUrl.replace(pattern, "Rights/any(c: $1c/User/Name$2 and c/RoleId eq " + roleId + ")");
                }

                var localStorageKey = "it-system-overview-options";
                var sessionStorageKey = "it-system-overview-options";
                var gridState = gridStateService.getService(localStorageKey, sessionStorageKey);

                // saves grid state to localStorage
                function saveGridOptions() {
                    gridState.saveGridOptions($scope.mainGrid);
                }

                // loads kendo grid options from localstorage
                function loadGridOptions() {
                    gridState.loadGridOptions($scope.mainGrid);
                }

                // clears grid filters by removing the localStorageItem and reloading the page
                $scope.clearOptions = function () {
                    gridState.clearOptions(localStorageKey, sessionStorageKey);
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
                });

                // overview grid options
                $scope.mainGridOptions = {
                    autoBind: false, // do not set to true, it works because the org unit filter inits the query
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystemUsages?$expand=ItSystem($expand=AppTypeOption,BusinessType,CanUseInterfaces,ItInterfaceExhibits),Organization,ResponsibleUsage($expand=OrganizationUnit),Overview($expand=ItSystem),MainContract($expand=ItContract),Rights($expand=User,Role)",
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
                        pageSize: 10,
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
                        }
                    ],
                    excel: {
                        fileName: "IT System Overblik.xlsx",
                        filterable: true,
                        allPages: true
                    },
                    pageable: {
                        refresh: true,
                        pageSizes: [10, 20, 50, 100, 200],
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
                            field: "ItSystem.Name", title: "IT System", width: 150, persistId: "sysname",
                            template: "<a data-ui-sref='it-system.usage.interfaces({id: #: Id #})'>#: ItSystem.Name #</a>",
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ResponsibleUsage.OrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 150, persistId: "orgunit",
                            template: "#: ResponsibleUsage ? ResponsibleUsage.OrganizationUnit.Name : '' #",
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    template: orgUnitDropDownList
                                }
                            }
                        },
                        {
                            field: "LocalSystemId", title: "Lokal system ID", width: 150, persistId: "localid",
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ItSystem.BusinessType.Name", title: "Forretningstype", width: 150, persistId: "busitype",
                            template: "#: ItSystem.BusinessType ? ItSystem.BusinessType.Name : '' #",
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ItSystem.AppTypeOption.Name", title: "Applikationstype", width: 150, persistId: "apptype",
                            template: "#: ItSystem.AppTypeOption ? ItSystem.AppTypeOption.Name : '' #",
                            hidden: true,
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "MainContract", title: "Kontrakt", width: 80, persistId: "contract",
                            template: contractTemplate,
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "ItSystem.CanUseInterfaces", title: "Anvender", width: 95, persistId: "canuse",
                            template: "<a data-ng-click=\"showUsageDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.CanUseInterfaces.length #</a>",
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "ItSystem.ItInterfaceExhibits", title: "Udstiller", width: 95, persistId: "exhibit",
                            template: "<a data-ng-click=\"showExposureDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.ItInterfaceExhibits.length #</a>",
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "Overview.ItSystem.Name", title: "Overblik", width: 150, persistId: "overview",
                            template: "#: Overview ? Overview.ItSystem.Name : '' #",
                            hidden: true
                        },
                        {
                            field: "LastChanged", title: "Opdateret", format: "{0:dd-MM-yyyy}", width: 150, persistId: "changed",
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "SystemOwner", title: "Systemejer", persistId: "sysowner",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 1);
                            },
                            width: 150,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "SystemResponsible", title: "Systemansvarlig", persistId: "sysresp",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 2);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "BusinessOwner", title: "Forretningsejer", persistId: "busiowner",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 3);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "SuperUserResponsible", title: "Superbrugeransvarlig", persistId: "superuserresp",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 4);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "SuperUser", title: "Superbruger", persistId: "superuser",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 5);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "SecurityResponsible", title: "Sikkerhedsansvarlig", persistId: "secresp",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 6);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "ChangeManager", title: "Changemanager", persistId: "changemanager",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 7);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "DataOwner", title: "Dataejer", persistId: "dataowner",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 8);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "SystemAdmin", title: "Systemadminstrator", persistId: "sysadm",
                            template: function (dataItem) {
                                return roleTemplate(dataItem, 9);
                            },
                            width: 150,
                            hidden: true,
                            sortable: false,
                            filterable: {
                                cell: {
                                    dataSource: [],
                                    delay: 1500,
                                    operator: "contains",
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
                    if (dataItem.MainContract)
                        if (dataItem.MainContract.ItContract)
                            if (dataItem.MainContract.ItContract.IsActive)
                                return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="glyphicon glyphicon-file text-success" aria-hidden="true"></span></a>';
                            else
                                return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="glyphicon glyphicon-file text-muted" aria-hidden="true"></span></a>';

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
                        pageSize: 10,
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
                        pageSize: 10,
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
                            optionLabel: "Vælg Organisationsenhed",
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
                    var optionLabelOffset = 1;

                    // find the index of the org unit that matches the users default org unit
                    var index = _.findIndex(kendoElem.dataItems(), function (item) {
                        return item.Id == user.defaultOrganizationUnitId;
                    });
                    
                    if (index !== -1) {
                        // select the users default org unit + the optionLabel offset
                        kendoElem.select(index + optionLabelOffset);

                        var selectedId = _.parseInt(kendoElem.value());
                        var childIds = kendoElem.dataItem().childIds;
                        // apply filter
                        filterByOrgUnit(selectedId, childIds);
                    }
                }

                function orgUnitChanged() {
                    var kendoElem = this;
                    var selectedId = _.parseInt(kendoElem.value());
                    var childIds = kendoElem.dataItem().childIds;
                    // apply filter
                    filterByOrgUnit(selectedId, childIds);
                }

                function filterByOrgUnit(selectedId, childIds) {
                    var field = "ResponsibleUsage.OrganizationUnit.Id";
                    var currentFilter = itSystemOverviewDataSource.filter();
                    var newFilter;

                    if (isNaN(selectedId)) {
                        // TODO remove filter(s) on this field only
                        newFilter = _.removeFiltersForField(currentFilter, field);
                    } else {
                        var filters = [{ field: field, operator: "eq", value: selectedId }];
                        // add children to filters
                        _.forEach(childIds, function (id) {
                            filters.push({ field: field, operator: "eq", value: id });
                        });

                        newFilter = {
                            logic: "or",
                            filters: filters
                        };
                    }
                    // can't use datasource object directly,
                    // if we do then the view doesn't update.
                    // So have to go through $scope - sadly :(
                    $scope.mainGrid.dataSource.filter(newFilter);
                }
            }
        ]
    );
})(angular, app);

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

                var itSystemOverviewDataSource = new kendo.data.DataSource({
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
                });

                var localStorageKey = "it-system-overview-options";
                var sessionStorageKey = "it-system-overview-options";

                // saves grid state to localStorage
                function saveGridOptions() {
                    if ($scope.mainGrid) {
                        // timeout fixes columnReorder saves before the column is actually reordered 
                        // http://stackoverflow.com/questions/21270748/kendo-grid-saving-state-in-columnreorder-event
                        $timeout(function () {
                            var options = $scope.mainGrid.getOptions();
                            gridStateService.save(localStorageKey, sessionStorageKey, options);
                        });
                    }
                }

                // loads kendo grid options from localstorage
                function loadGridOptions() {
                    var options = gridStateService.get(localStorageKey, sessionStorageKey);
                    $scope.mainGrid.setOptions(options);
                }
                
                // fires when kendo is finished rendering all its goodies
                $scope.$on("kendoRendered", function (e) {
                    loadGridOptions();
                });

                // clears grid filters by removing the localStorageItem and reloading the page
                $scope.clearOptions = function () {
                    gridStateService.clear(localStorageKey, sessionStorageKey);
                    // have to reload entire page, as dataSource.read() + grid.refresh() doesn't work :(
                    reload();
                }

                function reload() {
                    $state.go('.', null, { reload: true });
                }

                // overview grid options
                $scope.mainGridOptions = {
                    dataSource: itSystemOverviewDataSource,
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
                            field: "ItSystem.Name", title: "IT System", width: 150,
                            template: "<a data-ui-sref='it-system.usage.interfaces({id: #: Id #})'>#: ItSystem.Name #</a>",
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ResponsibleUsage.OrganizationUnit.Id", title: "Ansv. organisationsenhed", width: 150,
                            template: "#: ResponsibleUsage ? ResponsibleUsage.OrganizationUnit.Name : '' #",
                            filterable: {
                                cell: {
                                    showOperators: false,
                                    template: function orgUnitDropDownList(args) {
                                        // http://dojo.telerik.com/ODuDe/5
                                        args.element.removeAttr("data-bind");
                                        args.element.kendoDropDownList({
                                            dataSource: {
                                                type: "odata-v4",
                                                transport: {
                                                    read: {
                                                        url: "/odata/Organizations(" + localStorage.getItem("currentOrgId") + ")/OrganizationUnits",
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
                                            template: function indent(dataItem) {
                                                var htmlSpace = "&nbsp;&nbsp;&nbsp;&nbsp;";
                                                return htmlSpace.repeat(dataItem.$level) + dataItem.Name;
                                            },
                                            dataBound: function setDefaultOrgUnit() {
                                                var kendoElem = this;
                                                var id = localStorage.getItem("defaultOrgUnitId");
                                                var index = _.findIndex(kendoElem.dataItems(), function (item) {
                                                    return item.Id == id;
                                                });
                                                kendoElem.select(index);

                                                // WARN exactly the same as below, but because of KENDO BULLSHIT
                                                // we have to c/p... just great!
                                                var selectedId = _.parseInt(kendoElem.value());
                                                var childIds = kendoElem.dataItem().childIds;
                                                var field = "ResponsibleUsage.OrganizationUnit.Id";

                                                var filters = [{ field: field, operator: "eq", value: selectedId }];
                                                // add children to filters
                                                _.forEach(childIds, function (id) {
                                                    filters.push({ field: field, operator: "eq", value: id });
                                                });

                                                var filter = {
                                                    logic: "or",
                                                    filters: filters
                                                };

                                                // this doesn't work... susepct we have to do a $apply(), but can't
                                                // args.dataSource.filter(filter);
                                                // so resorting to EVAL()... ARE YOU KIDDING ME KENDO?!!
                                                eval("itSystemOverviewDataSource.filter(filter);"); // this sometimes throws: itSystemOverviewDataSource is not defined
                                                // but it works... 
                                            },
                                            change: function () {
                                                var kendoElem = this;
                                                var selectedId = _.parseInt(kendoElem.value());
                                                var childIds = kendoElem.dataItem().childIds;
                                                var field = "ResponsibleUsage.OrganizationUnit.Id";

                                                var filters = [{ field: field, operator: "eq", value: selectedId }];
                                                // add children to filters
                                                _.forEach(childIds, function (id) {
                                                    filters.push({ field: field, operator: "eq", value: id });
                                                });

                                                var filter = {
                                                    logic: "or",
                                                    filters: filters
                                                };

                                                // this doesn't work... susepct we have to do a $apply(), but can't
                                                // args.dataSource.filter(filter);
                                                // so resorting to EVAL()... ARE YOU KIDDING ME KENDO?!!
                                                eval("itSystemOverviewDataSource.filter(filter);");
                                            }
                                        });
                                    }
                                }
                            }
                        },
                        {
                            field: "LocalSystemId", title: "Lokal system ID", width: 150,
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ItSystem.BusinessType.Name", title: "Forretningstype", width: 150,
                            template: "#: ItSystem.BusinessType ? ItSystem.BusinessType.Name : '' #",
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ItSystem.AppTypeOption.Name", title: "Applikationstype", width: 150,
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
                            field: "MainContract", title: "Kontrakt", width: 80,
                            template: kendoTemplate.contractTemplate,
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "ItSystem.CanUseInterfaces", title: "Anvender", width: 95,
                            template: "<a data-ng-click=\"showUsageDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.CanUseInterfaces.length #</a>",
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "ItSystem.ItInterfaceExhibits", title: "Udstiller", width: 95,
                            template: "<a data-ng-click=\"showExposureDetails(#: ItSystem.Id #,'#: ItSystem.Name #')\">#: ItSystem.ItInterfaceExhibits.length #</a>",
                            filterable: false,
                            sortable: false
                        },
                        {
                            field: "Overview.ItSystem.Name", title: "Overblik", width: 150,
                            template: "#: Overview ? Overview.ItSystem.Name : '' #",
                            hidden: true
                        },
                        {
                            field: "LastChanged", title: "Opdateret", format: "{0:dd-MM-yyyy}", width: 150,
                        },
                        {
                            // DON'T YOU DARE RENAME!
                            field: "SystemOwner", title: "Systemejer",
                            template: function(dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 1);
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
                            field: "SystemResponsible", title: "Systemansvarlig",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 2);
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
                            field: "BusinessOwner", title: "Forretningsejer",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 3);
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
                            field: "SuperUserResponsible", title: "Superbrugeransvarlig",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 4);
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
                            field: "SuperUser", title: "Superbruger",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 5);
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
                            field: "SecurityResponsible", title: "Sikkerhedsansvarlig",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 6);
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
                            field: "ChangeManager", title: "Changemanager",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 7);
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
                            field: "DataOwner", title: "Dataejer",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 8);
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
                            field: "SystemAdmin", title: "Systemadminstrator",
                            template: function (dataItem) {
                                return kendoTemplate.roleTemplate(dataItem, 9);
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

                // show exposureDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
                $scope.showExposureDetails = function (usageId, systemName) {
                    // filter by usageId
                    exhibitDetailDataSource.filter({ field: "ItSystemId", operator: "eq", value: usageId });
                    // set title
                    $scope.exhibitModal.setOptions({ title: systemName + " udstiller følgende snitflader" });
                    // open modal
                    $scope.exhibitModal.center().open();
                };

                var exhibitDetailDataSource = new kendo.data.DataSource({
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
                });

                $scope.exhibitDetailsGrid = {
                    dataSource: exhibitDetailDataSource,
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
                    usageDetailDataSource.filter({ field: "ItSystemId", operator: "eq", value: systemId });
                    // set modal title
                    $scope.modal.setOptions({ title: "Anvendelse af " + systemName });
                    // open modal
                    $scope.modal.center().open();
                };
                
                var usageDetailDataSource = new kendo.data.DataSource({
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
                });

                // usagedetails grid - shows which organizations has a given itsystem in local usage
                $scope.usageDetailsGrid = {
                    dataSource: usageDetailDataSource,
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
            }
        ]
    );
})(angular, app);

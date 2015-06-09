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
            '$rootScope', '$scope', '$http', 'user',
            function($rootScope, $scope, $http, user) {
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
                            url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystemUsages?$expand=ItSystem($expand=AppTypeOption,BusinessType,CanUseInterfaces,ItInterfaceExhibits),Organization,ResponsibleUsage,Overview($expand=ItSystem),MainContract($expand=ItContract),Rights($expand=User,Role)",
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

                var localStorageKey = "kendo-grid-it-system-overview-options";

                // saves grid state to localStorage
                function saveGridOptions(e) {
                    if ($scope.mainGrid) {
                        // timeout fixes columnReorder saves before the column is actually reordered 
                        // http://stackoverflow.com/questions/21270748/kendo-grid-saving-state-in-columnreorder-event
                        $timeout(function () {
                            var options = $scope.mainGrid.getOptions();
                            var pickedOptions = {}; // _.pick(options, 'columns'); BUG disabled for now as saving column data overwrites source changes - FUBAR!
                            pickedOptions.dataSource = _.pick(options.dataSource, ['filter', 'sort', 'page', 'pageSize']);
                            localStorage[localStorageKey] = kendo.stringify(pickedOptions);
                        });
                    }
                }

                // loads kendo grid options from localstorage
                function loadOptions() {
                    var options = localStorage[localStorageKey];
                    if (options) {
                        $scope.mainGrid.setOptions(JSON.parse(options));
                    }
                }

                // fires when kendo is finished rendering all its goodies
                $scope.$on("kendoRendered", function (e) {
                    loadOptions();
                });

                // clears grid filters by removing the localStorageItem and reloading the page
                $scope.clearOptions = function () {
                    localStorage.removeItem(localStorageKey);
                    itSystemOverviewDataSource.read();
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
                        fileName: "IT System Katalog.xlsx",
                        filterable: true,
                        allPages: true
                    },
                    pageable: {
                        refresh: true,
                        pageSizes: true,
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
                            locked: true,
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
                                }
                            }
                        },
                        {
                            field: "ResponsibleUsage.OrganizationUnit.Name", title: "Ansv. organisationsenhed", width: 150,
                            template: "#: ResponsibleUsage ? ResponsibleUsage.OrganizationUnit.Name : '' #",
                            filterable: {
                                cell: {
                                    delay: 1500,
                                    operator: "contains",
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
                            template: function (dataItem) {
                                return contractTemplate(dataItem);
                            },
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
                            field: "SystemResponsible", title: "Systemansvarlig",
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
                            field: "BusinessOwner", title: "Forretningsejer",
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
                            field: "SuperUserResponsible", title: "Superbrugeransvarlig",
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
                            field: "SuperUser", title: "Superbruger",
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
                            field: "SecurityResponsible", title: "Sikkerhedsansvarlig",
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
                            field: "ChangeManager", title: "Changemanager",
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
                            field: "DataOwner", title: "Dataejer",
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
                            field: "SystemAdmin", title: "Systemadminstrator",
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

                function contractTemplate(dataItem) {
                    if (dataItem.MainContract)
                        if (dataItem.MainContract.ItContract)
                            if (dataItem.MainContract.ItContract.IsActive)
                                return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="glyphicon glyphicon-file text-success" aria-hidden="true"></span></a>';
                            else
                                return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="glyphicon glyphicon-file text-muted" aria-hidden="true"></span></a>';

                    return "";
                }

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

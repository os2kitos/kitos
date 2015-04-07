(function (ng, app) {
    app.config([
        '$stateProvider', '$urlRouterProvider', function($stateProvider) {
            $stateProvider.state('it-system.overview', {
                url: '/overview',
                templateUrl: 'partials/it-system/overview-it-system.html',
                controller: 'system.OverviewCtrl',
                resolve: {
                    businessTypes: [
                        '$http', function($http) {
                            return $http.get("api/businesstype");
                        }
                    ],
                    organizations: [
                        '$http', function($http) {
                            return $http.get("api/organization");
                        }
                    ],
                    user: [
                        'userService', function(userService) {
                            return userService.getUser();
                        }
                    ],
                    itSystemRoles: ['$http', function ($http) {
                        return $http.get("api/itsystemrole/")
                            .then(function (result) {
                                return result.data.response;
                            });
                    }]
                }
            });
        }
    ]);

    app.controller('system.OverviewCtrl',
        [
            '$rootScope', '$scope', '$http', 'notify', 'businessTypes', 'organizations', 'user', 'itSystemRoles',
            function($rootScope, $scope, $http, notify, businessTypesHttp, organizationsHttp, user, itSystemRoles) {
                $rootScope.page.title = 'IT System - Overblik';
                $scope.itSystemRoles = itSystemRoles;
                $scope.pagination = {
                    search: '',
                    skip: 0,
                    take: 20
                };

                $scope.csvUrl = 'api/itSystemUsage?csv&organizationId=' + user.currentOrganizationId;

                var businessTypes = businessTypesHttp.data.response;

                $scope.showSystemId = 'localSystemId';
                $scope.showType = 'itSystem.appType.name';

                // clear lists 
                $scope.activeContracts = [];
                $scope.inactiveContracts = [];

                function loadUsages() {
                    $scope.itSystemUsage = [];

                    var url = 'api/itSystemUsage?overview&organizationId=' + user.currentOrganizationId + '&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += '&orderBy=' + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                    }

                    if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                    else url += "&q=";

                    $http.get(url).success(function(result, status, headers) {
                        $scope.systemUsages = result.response;

                        var paginationHeader = JSON.parse(headers('X-Pagination'));
                        $scope.totalCount = paginationHeader.TotalCount;

                        _.each(result.response, function(usage) {
                            usage.itSystem.businessType = _.findWhere(businessTypes, { id: usage.itSystem.businessTypeId });

                            $scope.itSystemUsage.push(usage);

                            loadOverviewSystem(usage);
                        });

                        function loadOverviewSystem(usage) {
                            if (!usage.overviewItSystem) return null;

                            return $http.get("api/itsystem/" + usage.overviewItSystemId).success(function(result) {
                                usage.overviewItSystem = result.response;
                            });
                        }

                    });
                }

                // overview grid options
                $scope.mainGridOptions = {
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystemUsages?$expand=ItSystem($expand=Parent,AppTypeOption,BusinessType,Usages,ItInterfaceExhibits),Organization,ResponsibleUsage,Overview($expand=ItSystem),MainContract($expand=ItContract)"
                            }
                        },
                        pageSize: 5,
                        serverPaging: true,
                        serverSorting: false
                    },
                    toolbar: [
                        { name: "excel", text: "Eksportér til Excel", className: "pull-right" },
                    ],
                    excel: {
                        fileName: "IT System Katalog.xlsx",
                        filterable: false,
                        allPages: true
                    },
                    pageable: {
                        refresh: true,
                        pageSizes: true,
                        buttonCount: 5
                    },
                    sortable: true,
                    columnMenu: true,
                    reorderable: true,
                    resizable: true,
                    columns: [
                        {
                            field: "ItSystem.Name", title: "IT System",
                            template: "<a data-ui-sref='it-system.usage.interfaces({id: #: data.Id#})' data-ng-bind='dataItem.ItSystem.Name'></a>"
                        },
                        {
                            field: "MainContract", title: "Aktiv", width: 70,
                            template: "<span data-ng-bind='dataItem.MainContract.ItContract.IsActive ? \"Ja\" : \"Nej\"'></span>"
                        },
                        {
                            field: "ResponsibleUsage", title: "Ansv. organisationsenhed",
                            template: "<span data-ng-bind='dataItem.ResponsibleUsage.OrganizationUnit.Name'></span>"
                        },
                        {
                            field: "ItSystem.AppTypeOption", title: "Applikationstype",
                            template: "<span data-ng-bind='dataItem.ItSystem.AppTypeOption.Name'></span>"
                        },
                        {
                            field: "ItSystem.BusinessType", title: "Forretningstype",
                            template: "<span data-ng-bind='dataItem.ItSystem.BusinessType.Name'></span>"
                        },
                        {
                            field: "ItSystem.Usages", title: "Anvender", width: 95,
                            template: "<a data-ng-click=\"showUsageDetails(#: data.ItSystem.Id#,'#: data.ItSystem.Name#')\">{{dataItem.ItSystem.Usages.length || 0}}</a>"
                        },
                        {
                            field: "ItSystem.ItInterfaceExhibits", title: "Udstiller", width: 95,
                            template: "<a data-ng-click=\"showExposureDetails(#: data.ItSystem.Id#,'#: data.ItSystem.Name#')\">{{dataItem.ItSystem.ItInterfaceExhibits.length || 0}}</a>"
                        },
                        {
                            field: "Overview", title: "Overblik",
                            template: "<span data-ng-bind='dataItem.Overview.ItSystem.Name'></span>"
                        },
                    ],
                    error: function(e) {
                        console.log(e);
                    }
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

                // show exposureDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
                $scope.showExposureDetails = function (usageId, systemName) {
                    // filter by usageId
                    $scope.exhibitGrid.dataSource.filter({ field: "ItSystemId", operator: "eq", value: usageId });
                    // set title
                    $scope.exhibitModal.setOptions({ title: "Udstilles af " + systemName });
                    // open modal
                    $scope.exhibitModal.center().open();
                };

                $scope.exhibitDetailsGrid = {
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: "/odata/ItInterfaceExhibits?$expand=ItInterface",
                                dataType: "json",
                                cache: false
                            }
                        },
                        pageSize: 10,
                        serverPaging: true,
                        serverSorting: true,
                        serverFiltering: true
                    },
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

                // show usageDetailsGrid - takes a itSystemUsageId for data and systemName for modal title
                $scope.showUsageDetails = function(systemId, systemName) {
                    // filter by usageId
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
                                url: "/odata/ItSystemUsages?$expand=Organization",
                                dataType: "json",
                                cache: false
                            },
                        },
                        pageSize: 10,
                        serverPaging: true,
                        serverSorting: true,
                        serverFiltering: true
                    },
                    columns: [
                        {
                            field: "Organization.Name",
                            title: "Organisation"
                        }
                    ],
                    dataBound: detailsBound
                };

                // shows a detailGrid containg userroles for a given itsystem
                $scope.detailGridOptions = function (dataItem) {
                    return {
                        dataSource: {
                            type: "odata-v4",
                            transport: {
                                read: "/odata/Organizations(" + user.currentOrganizationId + ")/ItSystemUsages(" + dataItem.Id + ")/Rights?$expand=Role,User,ObjectOwner"
                            },
                            serverPaging: true,
                            serverSorting: true,
                            serverFiltering: true,
                            pageSize: 5,
                        },
                        scrollable: false,
                        sortable: true,
                        pageable: true,
                        columns: [
                            {
                                field: "Role.Name", title: "Rolle",
                                template: "<span data-ng-bind='dataItem.Role.Name'></span>"
                            },
                            {
                                field: "User.Name", title: "Bruger",
                                template: "<span>{{dataItem.User.Name}} {{dataItem.User.LastName}}</span>"
                            },
                            {
                                field: "ObjectOwner.Name", title: "Oprettet af",
                                template: "<span>{{dataItem.ObjectOwner.Name}} {{dataItem.ObjectOwner.LastName}}</span>"
                            }
                        ]
                    };
                };
            }
        ]
    );
})(angular, app);

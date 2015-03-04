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

                $scope.$watchCollection('pagination', function() {
                    //var url = 'api/itSystemUsage?csv&organizationId=' + user.currentOrganizationId + '&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take;

                    //if ($scope.pagination.orderBy) {
                    //    url += '&orderBy=' + $scope.pagination.orderBy;
                    //    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                    //}

                    //if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                    //else url += "&q=";

                    //$scope.csvUrl = url;
                    loadUsages();
                });

                // clear lists 
                $scope.activeContracts = [];
                $scope.inactiveContracts = [];

                function loadUsages() {
                    $scope.activeSystemUsages = [];
                    $scope.inactiveSystemUsages = [];

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

                            if (usage.mainContractIsActive) {
                                $scope.activeSystemUsages.push(usage);
                            } else {
                                $scope.inactiveSystemUsages.push(usage);
                            }

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

                //KENDO
                $scope.mainGridOptions = {
                    dataSource: {
                        type: "odata-v4", // IMPORTANT!! "odata" != "odata-v4" https://github.com/telerik/ui-for-aspnet-mvc-examples/blob/master/grid/odata-v4-web-api-binding-wrappers/KendoUIMVC5/Views/Home/Index.cshtml
                        transport: {
                            read: {
                                url: "/odata/ItSystems?$select=Id,Name,Description"
                            }
                        },
                        pageSize: 5,
                        serverPaging: true,
                        serverSorting: true
                    },
                    sortable: true,
                    groupable: {
                        messages: {
                            empty: "Drag a column header and drop it here to group by that column"
                        }
                    },
                    pageable: true,
                    reorderable: true,
                    resizable: true,
                    columns: [
                        { field: "Id", title: "ID", width: "40px" },
                        { field: "Name", title: "Navn" },
                        { field: "Description", title: "Beskrivelse" }
                    ],
                    error: function(e) {
                        console.log(e);
                    }
                };
                //KENDO SLUT
            }
        ]
    );
})(angular, app);

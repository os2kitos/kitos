(function (ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('it-system.interfaceCatalog', {
                url: '/interface-catalog',
                templateUrl: 'partials/it-system/it-interface-catalog.html',
                controller: 'system.interfaceCatalogCtrl',
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

    app.controller('system.interfaceCatalogCtrl',
    [
        '$rootScope', '$scope', '$http', 'notify', '$state', 'user',
        function ($rootScope, $scope, $http, notify, $state, user) {
            $rootScope.page.title = 'Snitflade - Katalog';

            $scope.pagination = {
                search: '',
                skip: 0,
                take: 20
            };

            $scope.csvUrl = 'api/itInterface/?csv&organizationId=' + user.currentOrganizationId;

            $scope.$watchCollection('pagination', function () {
                //var url = 'api/itInterface/?csv&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

                //if ($scope.pagination.orderBy) {
                //    url += '&orderBy=' + $scope.pagination.orderBy;
                //    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                //}

                //if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                //else url += '&q=';

                //$scope.csvUrl = url;
                loadSystems();
            });

            function loadSystems() {
                var url = 'api/itInterface/?skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += '&q=';

                $http.get(url).success(function (result, status, headers) {
                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;

                    $scope.systems = [];
                    _.each(result.response, function (system) {
                        $scope.systems.push(system);
                    });
                });
            }
        }
    ]);
})(angular, app);

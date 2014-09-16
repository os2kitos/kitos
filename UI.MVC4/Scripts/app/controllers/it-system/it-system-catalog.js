(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-system/it-system-catalog.html',
            controller: 'system.CatalogCtrl',
            resolve: {
                businessTypes: [
                    '$http', function ($http) {
                        return $http.get('api/businesstype');
                    }
                ],
                organizations: [
                    '$http', function ($http) {
                        return $http.get('api/organization');
                    }
                ],
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('system.CatalogCtrl',
    [
        '$rootScope', '$scope', '$http', 'notify', '$state', 'businessTypes', 'organizations', 'user',
        function($rootScope, $scope, $http, notify, $state, businessTypesHttp, organizationsHttp, user) {
            $rootScope.page.title = 'IT System - Katalog';

            $scope.pagination = {
                skip: 0,
                take: 20
            };

            //$scope.showType = 'appType.name';

            var businessTypes = businessTypesHttp.data.response;
            var organizations = organizationsHttp.data.response;

            function loadUser(system) {
                return $http.get('api/user/' + system.objectOwnerId, { cache: true })
                    .success(function(result) {
                        system.user = result.response;
                    });
            }

            function loadOrganization(system) {
                return $http.get('api/organization/' + system.organizationId, { cache: true })
                    .success(function(result) {
                        system.organization = result.response;
                    });
            }

            function loadTaskRef(system) {
                if (system.taskRefIds.length == 0) return null;

                return $http.get('api/taskref/' + system.taskRefIds[0])
                    .success(function(result) {
                        system.taskId = result.response.taskKey;
                        system.taskName = result.response.description;
                    });
            }

            function loadUsage(system) {
                return $http.get(system.usageUrl)
                    .success(function(result) {
                        system.hasUsage = true;
                        system.usage = result.response;
                    });
            }

            function addUsage(system) {
                return $http.post('api/itsystemusage', {
                    itSystemId: system.id,
                    organizationId: user.currentOrganizationId
                }).success(function(result) {
                    notify.addSuccessMessage('Systemet er taget i anvendelse');
                    system.hasUsage = true;
                    system.usage = result.response;
                }).error(function(result) {
                    notify.addErrorMessage('Systemet kunne ikke tages i anvendelse!');
                });
            }

            function deleteUsage(system) {

                return $http.delete(system.usageUrl).success(function(result) {
                    notify.addSuccessMessage('Anvendelse af systemet er fjernet');
                    system.hasUsage = false;
                }).error(function(result) {
                    notify.addErrorMessage('Anvendelse af systemet kunne ikke fjernes!');
                });
            }

            $scope.$watchCollection('pagination', function() {
                var url = 'api/itSystem/?csv&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += '&q=';

                $scope.csvUrl = url;
                loadSystems();
            });

            function loadSystems() {
                var url = 'api/itSystem/?skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take + '&organizationId=' + user.currentOrganizationId;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += '&q=';

                $http.get(url).success(function(result, status, headers) {

                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.pagination.count = paginationHeader.TotalCount;

                    $scope.systems = [];
                    _.each(result.response, function(system) {
                        system.businessType = _.findWhere(businessTypes, { id: system.businessTypeId });

                        system.belongsTo = _.findWhere(organizations, { id: system.belongsToId });

                        system.usageUrl = 'api/itsystemusage?itSystemId=' + system.id + '&organizationId=' + user.currentOrganizationId;

                        loadUser(system);
                        loadOrganization(system);
                        loadTaskRef(system);
                        loadUsage(system);

                        system.addUsage = function() {
                            addUsage(system);
                        };

                        system.deleteUsage = function() {
                            deleteUsage(system);
                        };

                        $scope.systems.push(system);
                    });
                });
            }
        }
    ]);
})(angular, app);

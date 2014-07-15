(function (ng, app) {
    app.config([
        '$stateProvider', '$urlRouterProvider', function($stateProvider) {
            $stateProvider.state('it-system.overview', {
                url: '/overview',
                templateUrl: 'partials/it-system/overview-it-system.html',
                controller: 'system.OverviewCtrl',
                resolve: {
                    appTypes: [
                        '$http', function($http) {
                            return $http.get("api/apptype");
                        }
                    ],
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
        ['$rootScope', '$scope', '$http', 'notify', 'appTypes', 'businessTypes', 'organizations', 'user', 'itSystemRoles',
            function ($rootScope, $scope, $http, notify, appTypesHttp, businessTypesHttp, organizationsHttp, user, itSystemRoles) {
                $rootScope.page.title = 'IT System - Overblik';
                $scope.itSystemRoles = itSystemRoles;
                $scope.pagination = {
                    skip: 0,
                    take: 20
                };
                
                var appTypes = appTypesHttp.data.response;
                var businessTypes = businessTypesHttp.data.response;

                $scope.showSystemId = 'localSystemId';
                $scope.showType = 'itSystem.appType.name';

                $scope.$watchCollection('pagination', loadUsages);

                function loadUsages() {
                    $scope.systemUsages = [];

                    var url = 'api/itSystemUsage?organizationId=' + user.currentOrganizationId + '&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take;
                    
                    if ($scope.pagination.orderBy) {
                        url += '&orderBy=' + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                    }
                    
                    $http.get(url).success(function(result, status, headers) {
                        $scope.systemUsages = result.response;
                        
                        var paginationHeader = JSON.parse(headers('X-Pagination'));
                        $scope.pagination.count = paginationHeader.TotalCount;

                        _.each(result.response, function(usage) {

                            usage.itSystem.appType = _.findWhere(appTypes, { id: usage.itSystem.appTypeId });
                            usage.itSystem.businessType = _.findWhere(businessTypes, { id: usage.itSystem.businessTypeId });
                            
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
            }]);
})(angular, app);
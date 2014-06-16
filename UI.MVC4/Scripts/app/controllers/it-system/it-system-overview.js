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
                    ]
                }
            });
        }
    ]);

    app.controller('system.OverviewCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            'appTypes', 'businessTypes', 'organizations', 'user',
            function ($rootScope, $scope, $http, notify,
             appTypesHttp, businessTypesHttp, organizationsHttp, user) {
                $rootScope.page.title = 'IT System - Overblik';

                var appTypes = appTypesHttp.data.response;
                var businessTypes = businessTypesHttp.data.response;
                var organizations = organizationsHttp.data.response;

                $scope.showSystemId = 'global';
                $scope.showType = 'appType';

                $scope.systemUsages = {};

                $http.get("api/itSystemUsage?organizationId=" + user.currentOrganizationId).success(function (result) {
                    $scope.systemUsages = result.response;

                    _.each(result.response, function (usage) {

                        usage.itSystem.appType = _.findWhere(appTypes, { id: usage.itSystem.appTypeId });
                        usage.itSystem.businessType = _.findWhere(businessTypes, { id: usage.itSystem.businessTypeId });

                        loadOverviewSystem(usage);

                    });

                    function loadOverviewSystem(usage) {
                        if (!usage.overviewItSystem) return null;

                        return $http.get("api/itsystem/" + usage.overviewItSystemId).success(function (result) {
                            usage.overviewItSystem = result.response;
                        });
                    }

                });
            }]);
})(angular, app);
(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.org', {
            url: '/org',
            templateUrl: 'partials/local-config/tab-org.html',
            controller: 'local-config.EditOrgCtrl',
            resolve: {
                organization: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function(user) {
                        return $http.get('api/organization/' + user.currentOrganizationId).then(function(result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });
    }]);

    app.controller('local-config.EditOrgCtrl', ['$scope', '$http', 'notify', 'config', 'organization',
            function ($scope, $http, notify, config, organization) {
                $scope.orgName = organization.name;
                $scope.orgCvr = organization.cvr;
                $scope.orgType = organization.type;
                $scope.orgAutosaveUrl = 'api/organization/' + organization.id;
            }
        ]
    );
})(angular, app);
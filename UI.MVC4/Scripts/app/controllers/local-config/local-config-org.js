(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.org', {
            url: '/org',
            templateUrl: 'partials/local-config/tab-org.html',
            controller: 'local-config.EditOrgCtrl',
            resolve: {
                organization: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/organization/' + $stateParams.id).then(function(result) {
                        return result.data.response;
                    });
                }],
            }
        });
    }]);

    app.controller('local-config.EditOrgCtrl', ['$scope', '$http', '$stateParams', 'notify', 'config', 'organization',
            function ($scope, $http, $stateParams, notify, config, organization) {
                $scope.orgName = organization.name;
                $scope.orgCvr = organization.cvr;
                $scope.orgType = organization.type;
                $scope.orgAutosaveUrl = 'api/organization/' + $stateParams.id;
                

                $scope.support = {
                    chosenNameId: config.itSupportModuleNameId,
                    guideUrl: config.itSupportGuide,
                    showTabOverview: config.showTabOverview,
                    showTechnology: config.showColumnTechnology,
                    showUsage: config.showColumnUsage,
                    showMandatory: config.showColumnMandatory
                };
            }
        ]
    );
})(angular, app);
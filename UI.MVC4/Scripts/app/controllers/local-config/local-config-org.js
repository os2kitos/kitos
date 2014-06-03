(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.org', {
            url: '/org',
            templateUrl: 'partials/local-config/tab-org.html',
            controller: 'local-config.EditOrgCtrl',
            resolve: {
                
            }
        });
    }]);

    app.controller('local-config.EditOrgCtrl', ['$scope', '$http', 'notify', 'config',
            function ($scope, $http, notify, config) {
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
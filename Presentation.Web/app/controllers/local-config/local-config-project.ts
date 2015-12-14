(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.project', {
            url: '/project',
            templateUrl: 'partials/local-config/tab-project.html',
            controller: 'local-config.EditProjectCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('local-config.EditProjectCtrl', ['$scope',
            function ($scope) {
            }
        ]
    );
})(angular, app);
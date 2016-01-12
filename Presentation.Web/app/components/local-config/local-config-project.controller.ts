(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.project', {
            url: '/project',
            templateUrl: 'app/components/local-config/local-config-project.view.html',
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
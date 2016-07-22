(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.system', {
            url: '/system',
            templateUrl: 'app/components/local-config/local-config-system.view.html',
            controller: 'local-config.EditSystemCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('local-config.EditSystemCtrl', ['$scope',
            function ($scope) {
            }
        ]
    );
})(angular, app);

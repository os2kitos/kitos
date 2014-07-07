(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.system', {
            url: '/system',
            templateUrl: 'partials/local-config/tab-system.html',
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
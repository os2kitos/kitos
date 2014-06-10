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

    app.controller('local-config.EditSystemCtrl', ['$scope', '$http', 'notify', 'config',
            function ($scope, $http, notify, config) {
            }
        ]
    );
})(angular, app);
(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.contract', {
            url: '/contract',
            templateUrl: 'partials/local-config/tab-contract.html',
            controller: 'local-config.EditContractCtrl',
            resolve: {
                
            }
        });
    }]);

    app.controller('local-config.EditContractCtrl', ['$scope', '$http', 'notify', 'config',
            function ($scope, $http, notify, config) {

            }
        ]
    );
})(angular, app);
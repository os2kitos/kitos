(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-contract/it-contract-edit.html',
            controller: 'contract.EditCtrl',
            resolve: {
                
            }
        });
    }]);

    app.controller('contract.EditCtrl', ['$scope', '$http', 'notify',
            function ($scope, $http, notify) {
                
            }]);
})(angular, app);
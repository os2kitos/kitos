(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.systems', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-contract/tab-systems.html',
            controller: 'contract.EditSystemsCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('contract.EditSystemsCtrl', ['$scope', '$http', 'notify',
            function ($scope, $http, notify) {

            }]);
})(angular, app);
(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.create', {
            url: '/create',
            controller: 'contract.CreateCtrl',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller('contract.CreateCtrl', ['$scope', '$http', '$state', 'user',
            function ($scope, $http, $state, user) {
                var orgId = user.currentOrganizationId;
                $http.post('api/itcontract', { organizationId: orgId }).success(function (result) {
                    var contract = result.response;
                    $state.go('it-contract.edit', { id: contract.id });
                });
            }]);
})(angular, app);
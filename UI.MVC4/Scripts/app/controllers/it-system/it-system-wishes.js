(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.wishes', {
            url: '/wishes',
            templateUrl: 'partials/it-system/tab-wishes.html',
            controller: 'system.EditWishes',
            resolve: {
                wishes: ['$rootScope', '$http', '$stateParams', function ($rootScope, $http, $stateParams) {
                    return $http.get('api/wish/?userId='+ $rootScope.user.id + '&usageId=' + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditWishes', ['$scope', 'wishes', function ($scope, wishes) {
        $scope.wishes = wishes;
    }]);
})(angular, app);
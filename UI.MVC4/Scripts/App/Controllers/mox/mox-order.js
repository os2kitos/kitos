(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('mox.order', {
            url: '/order',
            templateUrl: 'partials/mox/mox-order.html',
            controller: 'mox.OrderCtrl',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('mox.OrderCtrl', ['$scope', '$http', 'notify', 'user',
            function ($scope, $http, notify, user) {
                
            }]);
})(angular, app);

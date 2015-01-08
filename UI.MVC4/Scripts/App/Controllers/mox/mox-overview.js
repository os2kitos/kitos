(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('mox.overview', {
            url: '/overview',
            templateUrl: 'partials/mox/mox-overview.html',
            controller: 'mox.OverviewCtrl',
            resolve: {
                
            }
        });
    }]);

    app.controller('mox.OverviewCtrl', ['$scope', '$http', 'notify',
            function ($scope, $http, notify) {

            }]);
})(angular, app);

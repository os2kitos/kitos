(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.org', {
            url: '/risk',
            templateUrl: 'partials/it-project/tab-risk.html',
            controller: 'project.EditRiskCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('project.EditRiskCtrl', ['$scope', '$http', '$stateParams', 'notify',
        function($scope, $http, $stateParams, notify) {


        }
    ]);
})(angular, app);
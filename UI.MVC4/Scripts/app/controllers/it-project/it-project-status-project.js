(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.status-project', {
            url: '/status-project',
            templateUrl: 'partials/it-project/tab-status-project.html',
            controller: 'project.EditStatusProjectCtrl'
        });
    }]);

    app.controller('project.EditStatusProjectCtrl',
    ['$scope', 'itProject',
        function ($scope, itProject) {
        }]);
})(angular, app);

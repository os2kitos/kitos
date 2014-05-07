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
            $scope.project = itProject;

            $scope.project.phases = [itProject.phase1, itProject.phase2, itProject.phase3, itProject.phase4, itProject.phase5];

            console.log($scope.project);
        }]);
})(angular, app);

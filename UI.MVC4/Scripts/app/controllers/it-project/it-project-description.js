(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.description', {
            url: '/description',
            templateUrl: 'partials/it-project/tab-description.html',
            controller: 'project.EditDescriptionCtrl'
        });
    }]);

    app.controller('project.EditDescriptionCtrl',
    ['$scope', 'project',
        function ($scope, project) {
            $scope.itProjectId = project.id;
            $scope.description = project.description;
        }]);
})(angular, app);

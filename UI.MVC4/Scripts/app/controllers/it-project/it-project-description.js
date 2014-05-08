(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.description', {
            url: '/description',
            templateUrl: 'partials/it-project/tab-description.html',
            controller: 'project.EditDescriptionCtrl'
        });
    }]);

    app.controller('project.EditDescriptionCtrl',
    ['$scope', 'itProject',
        function ($scope, itProject) {
            $scope.itProjectId = itProject.id;
            $scope.description = itProject.description;
        }]);
})(angular, app);

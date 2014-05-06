(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.itsys', {
            url: '/itsys',
            templateUrl: 'partials/it-project/tab-itsys.html',
            controller: 'project.EditItsysCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('project.EditItsysCtrl',
    ['$rootScope', '$scope', 'itProject',
        function ($rootScope, $scope, itProject) {



        }]);
})(angular, app);

(function(ng, app) {

    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('it-project.edit.stakeholders', {
            url: '/stakeholders',
            templateUrl: 'partials/it-project/tab-stakeholders.html',
            controller: 'project.EditStakeholdersCtrl',
            resolve: {
                                
            }
        });
    }]);

    app.controller('project.EditStakeholdersCtrl',
    ['$rootScope', '$scope', 'itProject',
        function($rootScope, $scope, itProject) {



        }]);


})(angular, app);
    
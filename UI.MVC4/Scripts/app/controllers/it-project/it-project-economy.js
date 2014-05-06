(function(ng, app) {

    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('edit-it-project.economy', {
            url: '/economy',
            templateUrl: 'partials/it-project/tab-economy.html',
            controller: 'project.EditEconomyCtrl',
            resolve: {
                                
            }
        });
    }]);

    app.controller('project.EditEconomyCtrl',
    ['$rootScope', '$scope', 'itProject',
        function($rootScope, $scope, itProject) {



        }]);


})(angular, app);
    
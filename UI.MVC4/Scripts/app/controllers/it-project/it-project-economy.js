(function(ng, app) {

    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('edit-it-project.economy', {
            url: '/economy',
            templateUrl: 'partials/it-project/tab-economy.html',
            controller: 'project.EditEconomy',
            resolve: {
                                
            }
        });
    }]);

    app.controller('project.EditCtrl',
    ['$rootScope', '$scope', 'itProject',
        function($rootScope, $scope, itProject) {



        }]);


})(angular, app);
    
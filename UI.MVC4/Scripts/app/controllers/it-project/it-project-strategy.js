(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.strategy', {
            url: '/strategy',
            templateUrl: 'partials/it-project/tab-strategy.html',
            controller: 'project.EditStrategyCtrl'
        });
    }]);

    app.controller('project.EditStrategyCtrl',
    ['$scope', 'itProject',
        function ($scope, itProject) {
            
        }]);
})(angular, app);

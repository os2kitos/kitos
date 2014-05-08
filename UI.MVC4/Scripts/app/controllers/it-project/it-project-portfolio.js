(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.portfolio', {
            url: '/portfolio',
            templateUrl: 'partials/it-project/portfolio.html',
            controller: 'project.EditPortfolioCtrl'
        });
    }]);

    app.controller('project.EditPortfolioCtrl',
    ['$scope',
        function ($scope) {
            
        }]);
})(angular, app);

(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.contract', {
            url: '/contract',
            templateUrl: 'partials/local-config/tab-contract.html',
        });
    }]);
})(angular, app);

(function(ng, app) {

    app.config(['$stateProvider', $stateProvider => {
        $stateProvider.state('config.contract', {
            url: '/contract',
            templateUrl: 'app/components/global-config/global-config-contract.view.html',
            authRoles: ['GlobalAdmin']
        });
    }]);

})(angular, app);

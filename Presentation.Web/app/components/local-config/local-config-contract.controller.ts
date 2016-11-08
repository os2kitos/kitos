(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.contract', {
            url: '/contract',
            templateUrl: 'app/components/local-config/local-config-contract.view.html',
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin]
        });
    }]);
})(angular, app);

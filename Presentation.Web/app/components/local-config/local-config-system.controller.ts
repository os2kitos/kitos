    (function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.system', {
            url: '/system',
            templateUrl: 'app/components/local-config/local-config-system.view.html',
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin]
        });
    }]);
})(angular, app);

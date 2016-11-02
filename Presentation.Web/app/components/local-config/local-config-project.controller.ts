(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.project', {
            url: '/project',
            templateUrl: 'app/components/local-config/local-config-project.view.html',
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin]
        });
    }]);
})(angular, app);
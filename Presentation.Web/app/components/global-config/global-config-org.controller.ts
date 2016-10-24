(function (ng, app) {

    app.config(['$stateProvider', $stateProvider => {
        $stateProvider.state('config.org', {
            url: '/org',
            templateUrl: 'app/components/global-config/global-config-org.view.html',
            authRoles: ['GlobalAdmin']
        });
    }]);

})(angular, app);

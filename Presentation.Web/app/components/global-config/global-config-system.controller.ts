(function(ng, app) {

    app.config(['$stateProvider', $stateProvider => {
        $stateProvider.state('config.system', {
            url: '/system',
            templateUrl: 'app/components/global-config/global-config-system.view.html',
            authRoles: ['GlobalAdmin']
        });
    }]);

})(angular, app);

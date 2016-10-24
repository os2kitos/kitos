(function(ng, app) {

    app.config(['$stateProvider', $stateProvider => {
        $stateProvider.state('config.project', {
            url: '/project',
            templateUrl: 'app/components/global-config/global-config-project.view.html',
            authRoles: ['GlobalAdmin']
        });
    }]);

})(angular, app);

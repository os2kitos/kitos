(function(ng, app) {
    var subnav = [
        { state: 'config.org', text: 'Organisation' },
        { state: 'config.project', text: 'IT Projekt' },
        { state: 'config.system', text: 'IT System' },
        { state: 'config.contract', text: 'IT Kontrakt' }
    ];

    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('config.project', {
            url: '/project',
            templateUrl: 'app/components/global-config/global-config-project.view.html',
            controller: 'globalConfig.ProjectCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('globalConfig.ProjectCtrl', ['$rootScope', '$scope', function ($rootScope, $scope) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
    }]);
})(angular, app);

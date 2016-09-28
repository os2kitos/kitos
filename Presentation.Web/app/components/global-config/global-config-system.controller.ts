(function(ng, app) {
    var subnav = [
        { state: 'config.org', text: 'Organisation' },
        { state: 'config.project', text: 'IT Projekt' },
        { state: 'config.system', text: 'IT System' },
        { state: 'config.contract', text: 'IT Kontrakt' }
    ];

    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('config.system', {
            url: '/system',
            templateUrl: 'app/components/global-config/global-config-system.view.html',
            controller: 'globalConfig.SystemCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('globalConfig.SystemCtrl', ['$rootScope', '$scope', function ($rootScope, $scope) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
    }]);
})(angular, app);

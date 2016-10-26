(function(ng, app) {
    var subnav = [
        { state: 'global-admin.organizations', text: 'Organisation' },
        { state: 'global-admin.global-users', text: 'Globale administratorer' },
        { state: 'global-admin.local-users', text: 'Lokale administratorer' },
        { state: 'global-admin.org', text: 'Roller' },
        { state: 'global-admin.project', text: 'IT Projekt' },
        { state: 'global-admin.system', text: 'IT System' },
        { state: 'global-admin.contract', text: 'IT Kontrakt' },
        { state: 'global-admin.misc', text: 'Andet' }
    ];

    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('global-admin.system', {
            url: '/system',
            templateUrl: 'app/components/global-admin/global-admin-system.view.html',
            controller: 'global-admin.SystemCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('global-admin.SystemCtrl', ['$rootScope', '$scope', function ($rootScope, $scope) {
        $rootScope.page.title = 'Global Admin';
        $rootScope.page.subnav = subnav;
    }]);
})(angular, app);

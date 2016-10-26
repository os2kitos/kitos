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
        $stateProvider.state('global-admin.project', {
            url: '/project',
            templateUrl: 'app/components/global-admin/global-admin-project.view.html',
            controller: 'global-admin.ProjectCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('global-admin.ProjectCtrl', ['$rootScope', '$scope', function ($rootScope, $scope) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
    }]);
})(angular, app);

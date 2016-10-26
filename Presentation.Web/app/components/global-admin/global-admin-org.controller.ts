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
        $stateProvider.state('global-admin.org', {
            url: '/org',
            templateUrl: 'app/components/global-admin/global-admin-org.view.html',
            controller: 'globalAdmin.globalAdminsCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('globalAdmin.globalAdminsCtrl', ['$rootScope', '$scope', function ($rootScope, $scope) {
        $rootScope.page.title = 'Global admin';
        $rootScope.page.subnav = subnav;
    }]);
})(angular, app);

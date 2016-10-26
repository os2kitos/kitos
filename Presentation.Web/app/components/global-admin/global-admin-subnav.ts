(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('global-admin', {
            url: '/global-admin',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            authRoles: ['GlobalAdmin'],

            controller: ['$rootScope', '$state', function ($rootScope, $state) {
                $rootScope.page.title = 'Global admin';
                $rootScope.page.subnav = [
                    { state: 'global-admin.organizations', text: 'Organisation' },
                    { state: 'global-admin.global-users', text: 'Globale administratorer' },
                    { state: 'global-admin.local-users', text: 'Lokale administratorer' },
                    { state: 'global-admin.org', text: 'Roller' },
                    { state: 'global-admin.project', text: 'IT Projekt' },
                    { state: 'global-admin.system', text: 'IT System' },
                    { state: 'global-admin.contract', text: 'IT Kontrakt' }
                    { state: 'global-admin.local-users', text: 'Lokale administratorer' },
                    { state: 'global-admin.misc', text: 'Andet' }
                ];
            }]
        });
    }]);
})(angular, app);

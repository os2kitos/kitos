﻿(function(ng, app) {
    var subnav = [
        { state: 'config.org', text: 'Organisation' },
        { state: 'config.project', text: 'IT Projekt' },
        { state: 'config.system', text: 'IT System' },
        { state: 'config.contract', text: 'IT Kontrakt' }
    ];

    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('config.org', {
            url: '/org',
            templateUrl: 'app/components/global-config/global-config-org.view.html',
            controller: 'globalConfig.OrgCtrl',
            authRoles: ['GlobalAdmin'],
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('globalConfig.OrgCtrl', ['$rootScope', '$scope', 'user', function ($rootScope, $scope, user) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        $scope.orgId = user.currentOrganizationId;
    }]);
})(angular, app);

(function(ng, app) {
    var subnav = [
        { state: 'config.org', text: 'Organisation' },
        { state: 'config.project', text: 'IT Projekt' },
        { state: 'config.system', text: 'IT System' },
        { state: 'config.contract', text: 'IT Kontrakt' }
    ];

    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('config', {
            url: '/global-config',
            abstract: false,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        }).state('config.org', {
            url: '/org',
            templateUrl: 'partials/global-config/org.html',
            controller: 'globalConfig.OrgCtrl',
            authRoles: ['GlobalAdmin']
        }).state('config.project', {
            url: '/project',
            templateUrl: 'partials/global-config/project.html',
            controller: 'globalConfig.ProjectCtrl',
            authRoles: ['GlobalAdmin']
        }).state('config.system', {
            url: '/system',
            templateUrl: 'partials/global-config/system.html',
            controller: 'globalConfig.SystemCtrl',
            authRoles: ['GlobalAdmin']
        }).state('config.contract', {
            url: '/contract',
            templateUrl: 'partials/global-config/contract.html',
            controller: 'globalConfig.ContractCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('globalConfig.OrgCtrl', ['$rootScope', '$scope', 'user', function ($rootScope, $scope, user) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        $scope.orgId = user.currentOrganizationId;
    }]);

    app.controller('globalConfig.ProjectCtrl', ['$rootScope', '$scope', 'user', function ($rootScope, $scope, user) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        $scope.orgId = user.currentOrganizationId;
    }]);

    app.controller('globalConfig.SystemCtrl', ['$rootScope', '$scope', 'user', function ($rootScope, $scope, user) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        $scope.orgId = user.currentOrganizationId;
    }]);

    app.controller('globalConfig.ContractCtrl', ['$rootScope', '$scope', 'user', function ($rootScope, $scope, user) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        $scope.orgId = user.currentOrganizationId;
    }]);
})(angular, app);

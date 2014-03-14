(function(ng, app) {
    var subnav = [
        { state: "config-org", text: "Organisation" },
        { state: "config-project", text: "IT Projekt" },
        { state: "config-system", text: "IT System" },
        { state: "config-contract", text: "IT Kontrakt" }
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function($stateProvider, $urlRouterProvider) {

        $stateProvider.state('config-org', {
            url: '/global-config/org',
            templateUrl: 'partials/global-config/org.html',
            controller: 'globalConfig.OrgCtrl',
            authRoles: ['GlobalAdmin']
        }).state('config-project', {
            url: '/global-config/project',
            templateUrl: 'partials/global-config/project.html',
            controller: 'globalConfig.ProjectCtrl',
            authRoles: ['GlobalAdmin']
        }).state('config-system', {
            url: '/global-config/system',
            templateUrl: 'partials/global-config/system.html',
            controller: 'globalConfig.SystemCtrl',
            authRoles: ['GlobalAdmin']
        }).state('config-contract', {
            url: '/global-config/contract',
            templateUrl: 'partials/global-config/contract.html',
            controller: 'globalConfig.ContractCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('globalConfig.OrgCtrl', ['$rootScope', '$scope', 'Restangular', 'growl', function($rootScope, $scope, Restangular, growl) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;

        var config = Restangular.one('config', 1);
        config.get().then(function(result) {
            $scope.ShowItSupporttModule = result.ShowItSupporttModule;
        });

        $scope.patch = function(data) {
            return config.patch(data);
        };
    }]);

    app.controller('globalConfig.ProjectCtrl', ['$rootScope', '$scope', 'Restangular', 'growl', function ($rootScope, $scope, Restangular, growl) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        
        var baseCategories = Restangular.all('projectCategory');
        baseCategories.getList().then(function (categories) {
            console.log('asdf');
            $scope.categories = categories;
        });
    }]);
})(angular, app);
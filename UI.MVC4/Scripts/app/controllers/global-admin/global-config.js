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

        $scope.patchRole = function (data, i) {
            var item = $scope.roles[i];
            var result = Restangular.one('DepartmentRole', item.Id).patch({ IsActive: data }).then(function () {
                updateRoles();
            });
            return result;
        };

        $scope.patchSuggestion = function (data, i) {
            var item = $scope.roleSuggestions[i];
            var result = Restangular.one('DepartmentRole', item.Id).patch({ IsSuggestion: data }).then(function () {
                updateRoles();
            });
            return result;
        };

        var baseRoles = Restangular.all('DepartmentRole');
        var updateRoles = function () {
            baseRoles.getList({ nonsuggestions: true }).then(function (roles) {
                console.log('roles', roles);
                $scope.roles = roles;
            });

            baseRoles.getList({ suggestions: true }).then(function (roles) {
                console.log('suggestions', roles);
                $scope.roleSuggestions = roles;
            });
        };
        updateRoles();
    }]);

    app.controller('globalConfig.ProjectCtrl', ['$rootScope', '$scope', 'Restangular', 'growl', function ($rootScope, $scope, Restangular, growl) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        
        var baseCategories = Restangular.all('projectCategory');
        baseCategories.getList({nonsuggestions: true}).then(function (categories) {
            $scope.categories = categories;
        });
        
        $scope.patchProjectCategory = function (data, i) {
            var item = $scope.categories[i];
            return Restangular.one('projectCategory', item.Id).patch({IsActive: data});
        };
        
        var basePhases = Restangular.all('ProjectPhase');
        basePhases.getList({ nonsuggestions: true }).then(function (phases) {
            $scope.phases = phases;
        });
        
        $scope.patchPhase = function (data, i) {
            var item = $scope.phases[i];
            return Restangular.one('ProjectPhase', item.Id).patch({ Name: data });
        };

        var baseRefs = Restangular.all('extReferenceType');
        baseRefs.getList({ nonsuggestions: true }).then(function (refs) {
            $scope.refs = refs;
        });
        
        $scope.patchRef = function (data, i) {
            var item = $scope.refs[i];
            return Restangular.one('extReferenceType', item.Id).patch({ Name: data });
        };
               
        $scope.patchRole = function (data, i) {
            var item = $scope.roles[i];
            var result = Restangular.one('ItProjectRole', item.Id).patch({ IsActive: data }).then(function() {
                updateRoles();
            });
            return result;
        };
        
        $scope.patchSuggestion = function (data, i) {
            var item = $scope.roleSuggestions[i];
            var result = Restangular.one('ItProjectRole', item.Id).patch({ IsSuggestion: data }).then(function() {
                updateRoles();
            });
            return result;
        };

        var baseRoles = Restangular.all('ItProjectRole');
        var updateRoles = function () {
            baseRoles.getList({ nonsuggestions: true }).then(function(roles) {
                console.log('roles', roles);
                $scope.roles = roles;
            });
            
            baseRoles.getList({ suggestions: true }).then(function (roles) {
                console.log('suggestions', roles);
                $scope.roleSuggestions = roles;
            });
        };
        updateRoles();
    }]);
})(angular, app);
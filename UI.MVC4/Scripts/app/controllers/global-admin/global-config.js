﻿(function(ng, app) {
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

        handler($scope, Restangular, 'DepartmentRole', 'DepartmentRoles');
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
        function updateRoles() {
            baseRoles.getList({ nonsuggestions: true }).then(function(roles) {
                $scope.roles = roles;
            });
            
            baseRoles.getList({ suggestions: true }).then(function (roles) {
                $scope.roleSuggestions = roles;
            });
        };
        updateRoles();
    }]);

    app.controller('globalConfig.SystemCtrl', ['$rootScope', '$scope', 'Restangular', 'growl', function($rootScope, $scope, Restangular, growl) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        
        var baseRefs = Restangular.all('extReferenceType');
        baseRefs.getList({ nonsuggestions: true }).then(function (refs) {
            $scope.refs = refs;
        });

        $scope.patchRef = function (data, i) {
            var item = $scope.refs[i];
            return Restangular.one('extReferenceType', item.Id).patch({ Name: data });
        };

        handler($scope, Restangular, 'ItSystemRole', 'ItSystemRoles');
        handler($scope, Restangular, 'SystemType', 'SystemTypes');
        handler($scope, Restangular, 'InterfaceType', 'InterfaceTypes');
        handler($scope, Restangular, 'ProtocolType', 'ProtocolTypes');
        handler($scope, Restangular, 'Method', 'Methods');
        handler($scope, Restangular, 'DatabaseType', 'DatabaseTypes');
        handler($scope, Restangular, 'Environment', 'Environments');
    }]);

    function handler($scope, Restangular, nameSingular, namePlural) {
        var local = {};

        $scope['patch' + nameSingular] = function (data, i) {
            var item = $scope[namePlural][i];
            var result = Restangular.one(nameSingular, item.Id).patch({ IsActive: data }).then(function () {
                local['update' + namePlural]();
            });
            return result;
        };

        $scope['patch' + nameSingular + 'Suggestion'] = function (data, i) {
            var item = $scope[nameSingular + 'Suggestions'][i];
            var result = Restangular.one('Method', item.Id).patch({ IsSuggestion: data }).then(function () {
                local['update' + namePlural]();
            });
            return result;
        };

        local['update' + namePlural] = function () {
            Restangular.all(nameSingular).getList({ nonsuggestions: true }).then(function (list) {
                $scope[namePlural] = list;
            });

            Restangular.all(nameSingular).getList({ suggestions: true }).then(function (list) {
                $scope[nameSingular + 'Suggestions'] = list;
            });
        };
        local['update' + namePlural]();
    }
})(angular, app);
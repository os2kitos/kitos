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
            var result = Restangular.one(nameSingular, item.Id).patch({ IsSuggestion: data }).then(function () {
                local['update' + namePlural]();
            });
            return result;
        };
        
        $scope['patch' + nameSingular + 'Access'] = function (data, i) {
            var item = $scope[nameSingular][i];
            var result = Restangular.one(nameSingular, item.Id).patch({ HasWriteAccess: data });
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

        handler($scope, Restangular, 'ItProjectRole', 'ItProjectRoles');
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

    app.controller('globalConfig.ContractCtrl', ['$rootScope', '$scope', 'Restangular', 'growl', function ($rootScope, $scope, Restangular, growl) {
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
        
        handler($scope, Restangular, 'ContractType', 'ContractTypes');
        handler($scope, Restangular, 'ContractTemplate', 'ContractTemplates');
        handler($scope, Restangular, 'PurchaseForm', 'PurchaseForms');
        handler($scope, Restangular, 'ItContractRole', 'ItContractRoles');
        handler($scope, Restangular, 'HandoverTrial', 'HandoverTrials');
        handler($scope, Restangular, 'PaymentModel', 'PaymentModels');
        handler($scope, Restangular, 'AgreementElement', 'AgreementElement');
    }]);
})(angular, app);
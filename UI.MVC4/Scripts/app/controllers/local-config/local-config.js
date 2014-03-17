(function (ng, app) {

    var subnav = [
            { state: "local-config", text: "IT Kontrakt konfig" }
    ];


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('local-config', {
            url: '/local-config',
            templateUrl: 'partials/local-config/contract.html',
            controller: 'localConfig.ContractCtrl',
            authRoles: ['LocalAdmin', 'GlobalAdmin'],
            resolve: {
                moduleNames: ['$http', function ($http) {
                    return $http.get("api/itcontractnames");
                }],
                config: ['$http', '$rootScope', function ($http, $rootScope) {
                    var munId = $rootScope.user.municipality;
                    return $http.get("api/config/" + munId);
                }],
                extRefTypes: ['$http', function ($http) {
                    return $http.get("api/extreferencetype");
                }],
                extRefLocales: ['$http', '$rootScope', function ($http, $rootScope) {
                    var munId = $rootScope.user.municipality;
                    return $http.get("api/extreferencetypelocale/" + munId);
                }],
                contractTypes: ['$http', function ($http) {
                    return $http.get("api/contracttype");
                }],
                contractTemplates: ['$http', function ($http) {
                    return $http.get("api/contracttemplate");
                }],
                purchaseForms: ['$http', function ($http) {
                    return $http.get("api/purchaseForm");
                }],
                roles: ['$http', function ($http) {
                    return $http.get("api/itcontractrole");
                }],
                handoverTrials: ['$http', function ($http) {
                    return $http.get("api/paymentmodel"); //TODO
                }],
                paymentModels: ['$http', function ($http) {
                    return $http.get("api/paymentmodel");
                }],
                agreementElements: ['$http', function ($http) {
                    return $http.get("api/paymentmodel"); //TODO
                }],
            }
        });

    }]);

    app.controller('localConfig.ContractCtrl',
        ['$rootScope', '$scope', '$http', '$filter', 'growl', 'moduleNames', 'config', 'extRefTypes', 'extRefLocales', 'contractTypes', 'contractTemplates', 'purchaseForms', 'roles', 'handoverTrials', 'paymentModels', 'agreementElements',
            function ($rootScope, $scope, $http, $filter, growl, moduleNames, config, extRefTypes, extRefLocales, contractTypes, contractTemplates, purchaseForms, roles, handoverTrials, paymentModels, agreementElements) {
                $rootScope.page.title = 'IT Kontrakt konfiguration';
                $rootScope.page.subnav = subnav;

                $scope.moduleNames = moduleNames.data.Response;
                $scope.chosenNameId = config.data.Response.ItContractNameId;

                $scope.extRefs = [];
                _.each(extRefTypes.data.Response, function (v) {
                    
                    var locale = _.find(extRefLocales.data.Response, function(loc) {
                        return loc.Original_Id == v.Id;
                    });

                    $scope.extRefs.push({
                        original: v.Name,
                        locale: locale.Name,
                        note: v.Note
                    });
                });

                $scope.contractTypes = [];
                _.each(contractTypes.data.Response, function(v) {
                    $scope.contractTypes.push({
                        original: v.Name,
                        note: v.Note
                    });
                });

                $scope.contractTemplates = [];
                _.each(contractTemplates.data.Response, function (v) {
                    $scope.contractTemplates.push({
                        original: v.Name,
                        note: v.Note
                    });
                });
                
                $scope.purchaseForms = [];
                _.each(purchaseForms.data.Response, function (v) {
                    $scope.purchaseForms.push({
                        original: v.Name,
                        note: v.Note
                    });
                });

                $scope.roles = [];
                _.each(roles.data.Response, function (v) {
                    $scope.roles.push({
                        original: v.Name,
                        note: v.Note
                    });
                });
                
                $scope.handoverTrials = [];
                _.each(handoverTrials.data.Response, function (v) {
                    $scope.handoverTrials.push({
                        original: v.Name,
                        note: v.Note
                    });
                });

                $scope.paymentModels = [];
                _.each(paymentModels.data.Response, function (v) {
                    $scope.paymentModels.push({
                        original: v.Name,
                        note: v.Note
                    });
                });

                $scope.agreementElements = [];
                _.each(agreementElements.data.Response, function (v) {
                    $scope.agreementElements.push({
                        original: v.Name,
                        note: v.Note
                    });
                });

            }]);

})(angular, app);
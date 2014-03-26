(function (ng, app) {
    
    var subnav = [];

    /*app.directive('chooseAdminOrg', ['$rootScope', '$http', '$state', 'growl', function ($rootScope, $http, $state, growl) {
        return {
            scope: {
                url: '@',
                chosen: '='
            },
            templateUrl: 'partials/local-config/choose-org.html',
            link: function (scope, element, attrs) {
                $http.get("api/organization").success(function (data) {

                    scope.organizations = _.filter(data.Response, function (org) {
                        return _.contains($rootScope.user.isLocalAdminFor, org.Id);
                    });

                });

                scope.choose = function () {
                    $state.go("local-config.Edit", { chosenId: scope.chosen });
                };
            }
        };
    }]);*/

    app.directive('suggestNew', ['$http', 'growl', function ($http, growl) {
        return {
            scope: {
                url: '@'
            },
            templateUrl: 'partials/local-config/suggest-new.html',
            link: function (scope, element, attrs) {
                scope.suggest = function () {
                    if (scope.suggestForm.$invalid) return;

                    var data = {
                        'IsSuggestion': true,
                        'Name': scope.suggestion
                    };

                    $http.post(scope.url, data).success(function (result) {
                        growl.addSuccessMessage('Foreslag sendt!');
                        scope.suggestion = "";
                    }).error(function (result) {
                        growl.addErrorMessage('Kunne ikke sende foreslag!');
                    });
                };
            }
        };
    }]);

    app.directive('optionList', ['$http', function ($http) {
        return {
            scope: {
                optionsUrl: '@',
                title: '@',
            },
            templateUrl: 'partials/local-config/optionlist.html',
            link: function (scope, element, attrs) {

                scope.list = [];

                var optionsData = $http.get(scope.optionsUrl).success(function (result) {
                    _.each(result.Response, function (v) {
                        scope.list.push({
                            id: v.Id,
                            name: v.Name,
                            note: v.Note
                        });
                    });
                });
            }
        };
    }]);

    app.directive('optionLocaleList', ['$rootScope', '$q', '$http', 'growl', function ($rootScope, $q, $http, growl) {
        return {
            scope: {
                optionsUrl: '@',
                localesUrl: '@',
                title: '@',
                orgId: '='
            },
            templateUrl: 'partials/local-config/optionlocalelist.html',
            link: function (scope, element, attrs) {

                var orgId = parseInt(scope.orgId);

                scope.list = [];
                
                $q.all([
                    $http.get(scope.optionsUrl),
                    $http.get(scope.localesUrl + '/' + orgId)
                ]).then(function (result) {

                    var options = result[0].data.Response;
                    var locales = result[1].data.Response;

                    _.each(options, function (v) {

                        var locale = _.find(locales, function (loc) {
                            return loc.Original_Id == v.Id;
                        });

                        var isNew = _.isUndefined(locale);
                        var localeName = isNew ? '' : locale.Name;

                        scope.list.push({
                            id: v.Id,
                            name: v.Name,
                            note: v.Note,
                            localeName: localeName,
                            isNew: isNew
                        });
                    });
                });


                scope.updateLocale = function (value, option) {

                    var oId = option.id;

                    if (_.isEmpty(value)) {

                        return $http({ method: 'DELETE', url: scope.url + '?mId=' + orgId + '&oId=' + oId });

                    } else {

                        var method = option.isNew ? 'POST' : 'PUT';

                        var data = {
                            'Name': value,
                            'Original_Id': oId,
                            'Municipality_Id': orgId
                        };

                        return $http({ method: method, url: scope.localesUrl, data: data })
                            .success(function (result) {
                                growl.addSuccessMessage('Felt opdateret');
                            }).error(function (result) {
                                growl.addErrorMessage('Kunne ikke opdatere feltet med værdien: ' + value + '!');
                            });
                    }
                };
            }
        };
    }]);


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('local-config', {
            url: '/local-config',
            templateUrl: 'partials/local-config/index.html',
            adminRoles: ['LocalAdmin'],
            controller: 'localConfig.InitCtrl',
            
            resolve: {
                organizationsHttp: ['$http', function($http) {
                    return $http.get("api/organization");
                }]
            }
            
        }).state('local-config.Edit', {
            templateUrl: 'partials/local-config/config.html',
            url: '/:chosenId',
            adminRoles: ['LocalAdmin'],
            controller: 'localConfig.EditCtrl',
            resolve: {
                
                organizationHttp: ['$http', '$stateParams', function($http, $stateParams) {
                    return $http.get('api/organization/' + $stateParams.chosenId);
                }],
                
                supportNamesHttp: ['$http', function($http) {
                    return $http.get('api/itsupportnames');
                }],
                projectNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itprojectnames');
                }],
                systemNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itsystemnames');
                }],
                contractNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itcontractnames');
                }],

                configHttp: ['$http', '$stateParams', function($http, $stateParams) {
                    return $http.get('api/config/' + $stateParams.chosenId);
                }]
            }
        });


        /* .state('local-config-support', {
            url: '/local-config/:chosenId',
            templateUrl: 'partials/local-config/support.html',
            controller: 'localConfig.SupportCtrl',
            authRoles: ['LocalAdmin'],
            resolve: {
                moduleNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itsupportnames');
                }],
                
                configHttp: ['$http', '$stateParams', '$rootScope', function ($http, $stateParams) {
                    if (!$stateParams.chosenId) return true;
                    
                    return $http.get('api/config/' + $stateParams.chosenId);
                }]
            }
        });.state('local-config-edt.project', {
            url: '/local-config/project',
            templateUrl: 'partials/local-config/project.html',
            controller: 'localConfig.ProjectCtrl',
            authRoles: ['LocalAdmin', 'GlobalAdmin'],
            resolve: {
                moduleNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itprojectnames');
                }],
                configHttp: ['$http', '$rootScope', function ($http, $rootScope) {
                    var munId = $rootScope.user.municipality;
                    return $http.get('api/config/' + chosenId);
                }]
            }
        }).state('local-config.system', {
            url: '/local-config/system',
            templateUrl: 'partials/local-config/system.html',
            controller: 'localConfig.SystemCtrl'
        }).state('local-config-contract', {
            url: '/local-config/contract',
            templateUrl: 'partials/local-config/contract.html',
            controller: 'localConfig.ContractCtrl'
        });
        */
    }]);

    var patch = function ($http, growl, url, value, fieldName) {
        var data = {};
        data[fieldName] = value;

        $http({ method: 'PATCH', url: url, data: data }).success(function (result) {
            growl.addSuccessMessage("Feltet er opdateret!");
        }).error(function (result) {
            growl.addErrorMessage("Feltet blev ikke opdateret!");
        });
    };

    app.controller('localConfig.InitCtrl',
        ['$rootScope', '$scope', '$http', '$state', 'organizationsHttp',
            function($rootScope, $scope, $http, $state, organizationsHttp) {
                $rootScope.page.title = 'Konfiguration';

                $scope.orgChooser = {
                    options: _.filter(organizationsHttp.data.Response,
                        function(org) {
                            return _.contains($rootScope.user.isLocalAdminFor,
                                org.Id);
                        })
                };

            $scope.choose = function () {
                $state.go("local-config.Edit", { chosenId: $scope.orgChooser.chosen });
            };
        }]);

    app.controller('localConfig.EditCtrl',
        ['$rootScope', '$scope', '$http', '$stateParams', 'growl', 'organizationHttp', 'supportNamesHttp', 'projectNamesHttp', 'systemNamesHttp', 'contractNamesHttp', 'configHttp',
            function ($rootScope, $scope, $http, $stateParams, growl, organizationHttp, supportNamesHttp, projectNamesHttp, systemNamesHttp, contractNamesHttp, configHttp) {
                
                $scope.orgId = $stateParams.chosenId;
                $scope.orgChooser.chosen = $stateParams.chosenId;

                $scope.organization = organizationHttp.data.Response;

                var config = configHttp.data.Response;
                $scope.updateConfig = function (value, fieldName) {
                    return patch($http, growl, 'api/config/' + config.Id, value, fieldName);
                };
                
                $scope.support = {
                    show: true,
                    moduleNames: supportNamesHttp.data.Response,
                    chosenNameId: config.ItSupportModuleName_Id,
                    guideUrl: config.ItSupportGuide,
                    showTabOverview: config.ShowTabOverview,
                    showTechnology: config.ShowColumnTechnology,
                    showUsage: config.ShowColumnUsage,
                    showMandatory: config.ShowColumnMandatory
                };

                $scope.project = {
                    show: false,
                    moduleNames: projectNamesHttp.data.Response,
                    chosenNameId: config.ItProjectModuleName_Id,
                    guideUrl: config.ItProjectGuide,
                    showPortfolio: config.ShowPortfolio,
                    showBC: config.ShowBC
                };

                $scope.system = {
                    show: false,
                    moduleNames: systemNamesHttp.data.Response,
                    chosenNameId: config.ItSystemModuleName_Id,
                    guideUrl: config.ItSystemGuide
                };

                $scope.contract = {
                    show: false,
                    moduleNames: contractNamesHttp.data.Response,
                    chosenNameId: config.ItContractModuleName_Id,
                    guideUrl: config.ItContractGuide
                };

                $scope.toggle = function (object, s) {
                    console.log(s);
                    object.show = !object.show;
                };

            }]);

})(angular, app);

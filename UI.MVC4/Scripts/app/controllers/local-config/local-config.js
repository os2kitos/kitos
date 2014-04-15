(function (ng, app) {
    
    var subnav = [];
    
    app.directive('suggestNew', ['$http', 'notify', function ($http, notify) {
        return {
            scope: {
                url: '@'
            },
            templateUrl: 'partials/local-config/suggest-new.html',
            link: function (scope, element, attrs) {
                scope.suggest = function () {
                    if (scope.suggestForm.$invalid) return;

                    var data = {
                        "isSuggestion": true,
                        "name": scope.suggestion
                    };

                    $http.post(scope.url, data).success(function (result) {
                        notify.addSuccessMessage('Foreslag sendt!');
                        scope.suggestion = "";
                    }).error(function (result) {
                        notify.addErrorMessage('Kunne ikke sende foreslag!');
                    });
                };
            }
        };
    }]);
    
    app.directive('suggestNewRole', ['$http', 'notify', function ($http, notify) {
        return {
            scope: {
                url: '@'
            },
            templateUrl: 'partials/local-config/suggest-new-role.html',
            link: function (scope, element, attrs) {
                scope.suggest = function () {
                    if (scope.suggestForm.$invalid) return;

                    var data = {
                        "isSuggestion": true,
                        "name": scope.suggestion,
                        "hasReadAccess": true,
                        "hasWriteAccess": scope.writeAccess
                    };

                    $http.post(scope.url, data).success(function (result) {
                        notify.addSuccessMessage('Foreslag sendt!');
                        scope.suggestion = "";
                    }).error(function (result) {
                        notify.addErrorMessage('Kunne ikke sende foreslag!');
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
                    _.each(result.response, function (v) {
                        scope.list.push({
                            id: v.id,
                            name: v.name,
                            note: v.note
                        });
                    });
                });
            }
        };
    }]);

    app.directive('optionLocaleList', ['$rootScope', '$q', '$http', 'notify', function ($rootScope, $q, $http, notify) {
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

                    var options = result[0].data.response;
                    var locales = result[1].data.response;

                    _.each(options, function (v) {

                        var locale = _.find(locales, function (loc) {
                            return loc.originalId == v.id;
                        });

                        var isNew = _.isUndefined(locale);
                        var localeName = isNew ? '' : locale.name;

                        scope.list.push({
                            id: v.id,
                            name: v.name,
                            note: v.note,
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
                            "name": value,
                            "originalId": oId,
                            "municipalityId": orgId
                        };

                        return $http({ method: method, url: scope.localesUrl, data: data })
                            .success(function (result) {
                                notify.addSuccessMessage('Felt opdateret');
                            }).error(function (result) {
                                notify.addErrorMessage('Kunne ikke opdatere feltet med værdien: ' + value + '!');
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

    }]);

    var patch = function ($http, notify, url, value, fieldName) {
        var data = {};
        data[fieldName] = value;

        $http({ method: 'PATCH', url: url, data: data }).success(function (result) {
            notify.addSuccessMessage("Feltet er opdateret!");
        }).error(function (result) {
            notify.addErrorMessage("Feltet blev ikke opdateret!");
        });
    };

    app.controller('localConfig.InitCtrl',
        ['$rootScope', '$scope', '$http', '$state', 'organizationsHttp',
            function($rootScope, $scope, $http, $state, organizationsHttp) {
                $rootScope.page.title = 'Konfiguration';
                $rootScope.page.subnav = [];

                $scope.orgChooser = {
                    options: _.filter(organizationsHttp.data.response,
                        function(org) {
                            return _.contains($rootScope.user.isLocalAdminFor,
                                org.id);
                        })
                };

            $scope.choose = function () {
                $state.go("local-config.Edit", { chosenId: $scope.orgChooser.chosen });
            };
        }]);

    app.controller('localConfig.EditCtrl',
        ['$rootScope', '$scope', '$http', '$stateParams', 'notify', 'organizationHttp', 'supportNamesHttp', 'projectNamesHttp', 'systemNamesHttp', 'contractNamesHttp', 'configHttp',
            function ($rootScope, $scope, $http, $stateParams, notify, organizationHttp, supportNamesHttp, projectNamesHttp, systemNamesHttp, contractNamesHttp, configHttp) {
                $rootScope.page.title = 'Konfiguration';
                $rootScope.page.subnav = [];
                
                $scope.orgId = $stateParams.chosenId;
                $scope.orgChooser.chosen = $stateParams.chosenId;

                $scope.organization = organizationHttp.data.response;

                var config = configHttp.data.response;
                $scope.updateConfig = function (value, fieldName) {
                    return patch($http, notify, 'api/config/' + config.id, value, fieldName);
                };
                
                $scope.support = {
                    show: true,
                    moduleNames: supportNamesHttp.data.response,
                    chosenNameId: config.itSupportModuleNameId,
                    guideUrl: config.itSupportGuide,
                    showTabOverview: config.showTabOverview,
                    showTechnology: config.showColumnTechnology,
                    showUsage: config.showColumnUsage,
                    showMandatory: config.showColumnMandatory
                };

                $scope.project = {
                    show: false,
                    moduleNames: projectNamesHttp.data.response,
                    chosenNameId: config.itProjectModuleNameId,
                    guideUrl: config.itProjectGuide,
                    showPortfolio: config.showPortfolio,
                    showBC: config.showBC
                };

                $scope.system = {
                    show: false,
                    moduleNames: systemNamesHttp.data.response,
                    chosenNameId: config.itSystemModuleNameId,
                    guideUrl: config.itSystemGuide
                };

                $scope.contract = {
                    show: false,
                    moduleNames: contractNamesHttp.data.response,
                    chosenNameId: config.itContractModuleNameId,
                    guideUrl: config.itContractGuide
                };

                $scope.toggle = function (object, s) {
                    console.log(s);
                    object.show = !object.show;
                };

            }]);

})(angular, app);

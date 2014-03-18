(function (ng, app) {

    var subnav = [
            { state: 'local-config-support', text: 'IT Support' },
            { state: 'local-config-project', text: 'IT Projekter' },
            { state: 'local-config-system', text: 'IT Systemer' },
            { state: 'local-config-contract', text: 'IT Kontrakter' }
    ];

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
                title: '@'
            },
            templateUrl: 'partials/local-config/optionlocalelist.html',
            link: function (scope, element, attrs) {
                var mId = $rootScope.user.municipality;

                scope.list = [];

                $q.all([
                    $http.get(scope.optionsUrl),
                    $http.get(scope.localesUrl + '/' + mId)
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

                        return $http({ method: 'DELETE', url: scope.url + '?mId=' + mId + '&oId=' + oId });

                    } else {

                        var method = option.isNew ? 'POST' : 'PUT';

                        var data = {
                            'Name': value,
                            'Original_Id': oId,
                            'Municipality_Id': mId
                        };

                        console.log(data);

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

        $stateProvider.state('local-config-support', {
            url: '/local-config',
            templateUrl: 'partials/local-config/support.html',
            controller: 'localConfig.SupportCtrl',
            authRoles: ['LocalAdmin', 'GlobalAdmin'],
            resolve: {
                moduleNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itsupportnames');
                }],
                configHttp: ['$http', '$rootScope', function ($http, $rootScope) {
                    var munId = $rootScope.user.municipality;
                    return $http.get('api/config/' + munId);
                }]
            }
        }).state('local-config-project', {
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
                    return $http.get('api/config/' + munId);
                }]
            }
        }).state('local-config-system', {
            url: '/local-config/system',
            templateUrl: 'partials/local-config/system.html',
            controller: 'localConfig.SystemCtrl',
            authRoles: ['LocalAdmin', 'GlobalAdmin'],
            resolve: {
                moduleNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itsystemnames');
                }],
                configHttp: ['$http', '$rootScope', function ($http, $rootScope) {
                    var munId = $rootScope.user.municipality;
                    return $http.get('api/config/' + munId);
                }]
            }
        }).state('local-config-contract', {
            url: '/local-config/contract',
            templateUrl: 'partials/local-config/contract.html',
            controller: 'localConfig.ContractCtrl',
            authRoles: ['LocalAdmin', 'GlobalAdmin'],
            resolve: {
                moduleNamesHttp: ['$http', function ($http) {
                    return $http.get('api/itcontractnames');
                }],
                configHttp: ['$http', '$rootScope', function ($http, $rootScope) {
                    var munId = $rootScope.user.municipality;
                    return $http.get('api/config/' + munId);
                }]
            }
        });

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

    app.controller('localConfig.ContractCtrl',
        ['$rootScope', '$scope', '$http', '$filter', 'growl', 'moduleNamesHttp', 'configHttp',
            function ($rootScope, $scope, $http, $filter, growl, moduleNamesHttp, configHttp) {

                $rootScope.page.title = 'IT Kontrakt konfiguration';
                $rootScope.page.subnav = subnav;

                $scope.moduleNames = moduleNamesHttp.data.Response;

                var config = configHttp.data.Response;

                $scope.chosenNameId = config.ItContractModuleName_Id;
                $scope.guideUrl = config.ItContractGuide;

                $scope.updateConfig = function (value, fieldName) {
                    return patch($http, growl, 'api/config/' + config.Id, value, fieldName);
                };

            }]);

    app.controller('localConfig.ProjectCtrl',
        ['$rootScope', '$scope', '$http', '$filter', 'growl', 'moduleNamesHttp', 'configHttp',
            function ($rootScope, $scope, $http, $filter, growl, moduleNamesHttp, configHttp) {

                $rootScope.page.title = 'IT Project konfiguration';
                $rootScope.page.subnav = subnav;
                $scope.updateConfig = function (value, fieldName) {
                    return patch($http, growl, 'api/config/' + config.Id, value, fieldName);
                };

                $scope.moduleNames = moduleNamesHttp.data.Response;

                var config = configHttp.data.Response;

                $scope.chosenNameId = config.ItProjectModuleName_Id;
                $scope.guideUrl = config.ItProjectGuide;
                $scope.showPortfolio = config.ShowPortfolio;
                $scope.showBC = config.ShowBC;


            }]);

    app.controller('localConfig.SystemCtrl',
        ['$rootScope', '$scope', '$http', '$filter', 'growl', 'moduleNamesHttp', 'configHttp',
            function ($rootScope, $scope, $http, $filter, growl, moduleNamesHttp, configHttp) {

                $rootScope.page.title = 'IT System konfiguration';
                $rootScope.page.subnav = subnav;
                $scope.updateConfig = function (value, fieldName) {
                    return patch($http, growl, 'api/config/' + config.Id, value, fieldName);
                };

                $scope.moduleNames = moduleNamesHttp.data.Response;

                var config = configHttp.data.Response;

                $scope.chosenNameId = config.ItSystemModuleName_Id;
                $scope.guideUrl = config.ItSystemGuide;


            }]);

    app.controller('localConfig.SupportCtrl',
        ['$rootScope', '$scope', '$http', '$filter', 'growl', 'moduleNamesHttp', 'configHttp',
            function ($rootScope, $scope, $http, $filter, growl, moduleNamesHttp, configHttp) {

                $rootScope.page.title = 'IT System konfiguration';
                $rootScope.page.subnav = subnav;
                $scope.updateConfig = function (value, fieldName) {
                    return patch($http, growl, 'api/config/' + config.Id, value, fieldName);
                };

                $scope.moduleNames = moduleNamesHttp.data.Response;

                var config = configHttp.data.Response;

                $scope.chosenNameId = config.ItSupportModuleName_Id;
                $scope.guideUrl = config.ItSupportGuide;

                $scope.showTabOverview = config.ShowTabOverview;
                $scope.showTechnology = config.ShowColumnTechnology;
                $scope.showUsage = config.ShowColumnUsage;
                $scope.showMandatory = config.ShowColumnMandatory;


            }]);


})(angular, app);

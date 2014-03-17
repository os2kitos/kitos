(function (ng, app) {

    var subnav = [
            { state: "local-config", text: "IT Kontrakt konfig" }
    ];

    app.directive('suggestNew', ['$http', 'growl', function ($http, growl) {
        return {
            scope: {
                url: '@'
            },
            templateUrl: 'partials/local-config/suggest-new.html',
            link: function (scope, element, attrs) {
                scope.suggest = function() {
                    var data = {
                        "IsSuggestion": true,
                        "Name": scope.suggestion
                    };

                    $http.post(scope.url, data).success(function(result) {
                        growl.addSuccessMessage("Foreslag sendt!");
                    }).error(function(result) {
                        growl.addErrorMessage("Kunne ikke sende foreslag!");
                    });
                };
            }
        };
    }]);
    
    app.directive('optionList', ['$http', function($http) {
        return {
           scope: {
               optionsUrl: '@',
               title: '@',
           },
           templateUrl: 'partials/local-config/optionlist.html',
           link: function (scope, element, attrs) {
               scope.list = [];
               
               var optionsData = $http.get(scope.optionsUrl).success(function(result) {
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
                    $http.get(scope.localesUrl + "/" + mId)
                ]).then(function (result) {
                    
                    var options = result[0].data.Response;
                    var locales = result[1].data.Response;
                    
                    _.each(options, function (v) {

                        var locale = _.find(locales, function (loc) {
                            return loc.Original_Id == v.Id;
                        });

                        var isNew = _.isUndefined(locale);
                        var localeName = isNew ? "" : locale.Name;

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

                    var oId = option.Id;

                    if (_.isEmpty(value)) {

                        return $http({ method: 'DELETE', url: scope.url + "?mId=" + mId + "&oId=" + oId});

                    } else {

                        var method = option.New ? 'POST' : 'PUT';

                        var data = {
                            "Name": value,
                            "Original_Id": oId,
                            "Municipality_Id": mId
                        };
                        
                        return $http({ method: method, url: scope.localesUrl, data: data })
                        .success(function(result) {
                            growl.addSuccessMessage("Felt opdateret");
                        }).error(function(result) {
                            growl.addErrorMessage("Kunne ikke opdatere feltet med værdien: " + value + "!");
                        });
                    }
                };
            }
        };
    }]);


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('local-config', {
            url: '/local-config',
            templateUrl: 'partials/local-config/contract.html',
            controller: 'localConfig.ContractCtrl',
            authRoles: ['LocalAdmin', 'GlobalAdmin'],
            resolve: {
                moduleNamesHttp: ['$http', function($http) {
                    return $http.get("api/itcontractnames");
                }],
                configHttp: ['$http', '$rootScope', function($http, $rootScope) {
                    var munId = $rootScope.user.municipality;
                    return $http.get("api/config/" + munId);
                }]
            }
        });

    }]);

    app.controller('localConfig.ContractCtrl',
        ['$rootScope', '$scope', '$http', '$filter', 'growl', 'moduleNamesHttp', 'configHttp',
            function ($rootScope, $scope, $http, $filter, growl, moduleNamesHttp, configHttp) {
                
                $rootScope.page.title = 'IT Kontrakt konfiguration';
                $rootScope.page.subnav = subnav;
                
                $scope.moduleNames = moduleNamesHttp.data.Response;

                var config = configHttp.data.Response;
                
                $scope.chosenNameId = config.ItContractNameId;
                $scope.guideUrl = config.ItContractGuide;

                $scope.updateGuide = function(url) {
                    var data = {
                        "ItContractGuide": url
                    };

                    $http.patch("api/config/" + config.Id, data);
                };

            }]);

})(angular, App);
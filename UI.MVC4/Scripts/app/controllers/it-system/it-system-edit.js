(function (ng, app) {


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('it-system.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-system/edit-it-system.html',
            controller: 'system.EditCtrl',
            resolve: {
                itSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itsystem/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                hasWriteAccess: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itsystem/" + $stateParams.id + "?hasWriteAccess")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                appTypes: [
                    '$http', function($http) {
                        return $http.get("api/apptype");
                    }
                ],
                interfaceAppType: [
                    '$http', function($http) {
                        return $http.get("api/apptype?interfaceAppType");
                    }
                ],
                businessTypes: [
                    '$http', function($http) {
                        return $http.get("api/businesstype");
                    }
                ],
                user: [
                    'userService', function(userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('system.EditCtrl',
        ['$rootScope', '$scope', '$http', '$state', 'notify', 'itSystem', 'hasWriteAccess',
            'appTypes', 'interfaceAppType', 'businessTypes', 'user', 'autofocus',
            function ($rootScope, $scope, $http, $state, notify, itSystem, hasWriteAccess,
            appTypes, interfaceAppType, businessTypes, user, autofocus) {
                $rootScope.page.title = 'IT System - Rediger system';
                autofocus();

                itSystem.updateUrl = 'api/itSystem/' + itSystem.id;
                itSystem.belongsTo = (!itSystem.belongsToId) ? null : { id: itSystem.belongsToId, text: itSystem.belongsToName};
                itSystem.parent = (!itSystem.parentId) ? null : { id: itSystem.parentId, text: itSystem.parentName };

                $scope.select2AllowClearOpt = {
                    allowClear: true
                };

                $scope.system = itSystem;
                $scope.hasWriteAccess = hasWriteAccess;

                $scope.appTypes = appTypes.data.response;
                $scope.interfaceAppType = interfaceAppType.data.response;
                $scope.businessTypes = businessTypes.data.response;

                $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem?nonInterfaces', true);
                $scope.organizationSelectOptions = selectLazyLoading('api/organization?', true, ['orgId=' + user.currentOrganizationId]);
                
                function selectLazyLoading(url, allowClear, paramAry) {
                    return {
                        minimumInputLength: 1,
                        initSelection: function (elem, callback) {
                        },
                        allowClear: allowClear,
                        ajax: {
                            data: function (term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function (queryParams) {
                                var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                                var res = $http.get(url + '&q=' + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (obj) {

                                    results.push({
                                        id: obj.id,
                                        text: obj.name
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }

                //when appType == interface, we only want to show the interface specific tab
                $scope.$watch('system.appTypeId', function(newVal, oldVal) {
                    if (newVal == oldVal || !newVal) return;
                    
                    if (newVal == $scope.interfaceAppType.id) {
                        $state.go('it-system.edit.interface-details');
                    } else {
                        $state.go('it-system.edit.interfaces');
                    }
                });

            }]);

})(angular, app);
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
                tsas: [
                    '$http', function($http) {
                        return $http.get("api/tsa");
                    }
                ],
                interfaces: [
                    '$http', function($http) {
                        return $http.get("api/interface");
                    }
                ],
                interfaceTypes: [
                    '$http', function($http) {
                        return $http.get("api/interfacetype");
                    }
                ],
                methods: [
                    '$http', function($http) {
                        return $http.get("api/method");
                    }
                ],
                dataTypes: [
                    '$http', function($http) {
                        return $http.get("api/datatype");
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
        ['$rootScope', '$scope', '$http', 'notify', 'itSystem',
            'appTypes', 'interfaceAppType', 'businessTypes', 'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'user',
            function ($rootScope, $scope, $http, notify, itSystem,
            appTypes, interfaceAppType, businessTypes, tsas, interfaces, interfaceTypes, methods, dataTypes, user) {
                $rootScope.page.title = 'IT System - Rediger system';

                itSystem.updateUrl = 'api/itSystem/' + itSystem.id;
                itSystem.belongsTo = !itSystem.belongsToId ? null : { id: itSystem.belongsToId, text: itSystem.belongsToName};
                itSystem.parent = !itSystem.parentId ? null : { id: itSystem.parentId, text: itSystem.parentName };

                $scope.system = itSystem;

                $scope.appTypes = appTypes.data.response;
                $scope.interfaceAppType = interfaceAppType.data.response;
                $scope.businessTypes = businessTypes.data.response;

                $scope.tsas = tsas.data.response;
                $scope.interfaces = interfaces.data.response;
                $scope.interfaceTypes = interfaceTypes.data.response;
                $scope.methods = methods.data.response;

                $scope.dataTypes = dataTypes.data.response;

                $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem?nonInterfaces', true);
                $scope.interfacesSelectOptions = selectLazyLoading('api/itsystem?interfaces');
                $scope.organizationSelectOptions = selectLazyLoading('api/organization?');


                $scope.newDataRow = {};

                // submit system
                $scope.addDataRow = function (newDataRow) {
                    if (!newDataRow.data || !newDataRow.dataTypeId) return;

                    $scope.system.dataRows.push({ data: newDataRow.data, dataTypeId: newDataRow.dataTypeId });

                    $scope.newDataRow.data = "";
                    $scope.newDataRow.dataTypeId = "";
                };

                function selectLazyLoading(url, allowClear) {
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
                                var res = $http.get(url + '&q=' + queryParams.data.query).then(queryParams.success);
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

                $http.get('api/taskref').success(function (result) {
                    $scope.kleFilter = { type: 'KLE-Emne' };
                    $scope.allTasksFlat = result.response;
                });

                $scope.cleanKleFilter = function () {
                    if ($scope.kleFilter.parentId === null) {
                        delete $scope.kleFilter.parentId;
                    }
                };
            }]);

})(angular, app);
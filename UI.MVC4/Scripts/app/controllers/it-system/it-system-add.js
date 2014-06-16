(function (ng, app) {


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('it-system.add', {
            url: '/add',
            templateUrl: 'partials/it-system/edit-it-system.html',
            controller: 'system.AddCtrl',
            resolve: {
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

    app.controller('system.AddCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            'appTypes', 'interfaceAppType', 'businessTypes', 'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'user',
            function ($rootScope, $scope, $http, notify,
            appTypes, interfaceAppType, businessTypes, tsas, interfaces, interfaceTypes, methods, dataTypes, user) {
                $rootScope.page.title = 'IT System - Opret nyt system';

                $scope.appTypes = appTypes.data.response;
                $scope.interfaceAppType = interfaceAppType.data.response;
                $scope.businessTypes = businessTypes.data.response;

                $scope.tsas = tsas.data.response;
                $scope.interfaces = interfaces.data.response;
                $scope.interfaceTypes = interfaceTypes.data.response;
                $scope.methods = methods.data.response;

                $scope.dataTypes = dataTypes.data.response;

                $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem?nonInterfaces');
                $scope.interfacesSelectOptions = selectLazyLoading('api/itsystem?interfaces');
                $scope.organizationSelectOptions = selectLazyLoading('api/organization?');

                //default values of a system
                $scope.system =
                {
                    accessModifier: 0,
                    dataRows: []
                };

                $scope.newDataRow = {};

                // submit system
                $scope.saveSystem = function () {

                    var system = $scope.system;

                    var checkedTasks = _.filter($scope.allTasksFlat, function (task) {
                        return task.isChecked;
                    });

                    var data = {
                        parentId: system.parent ? system.parent.id : null,
                        exposedById: system.exposedBy ? system.exposedBy.id : null,
                        canUseInterfaceIds: _.pluck(system.canUseInterfaces, 'id'),
                        belongsToId: system.belongsTo ? system.belongsTo.id : null,
                        organizationId: user.currentOrganizationId,

                        version: system.version,
                        name: system.name,
                        systemId: system.systemId,
                        userId: user.id,
                        accessModifier: system.accessModifier,
                        description: system.description,
                        url: system.url,
                        taskRefIds: _.pluck(checkedTasks, 'id'),

                        appTypeId: system.appTypeId,
                        businessTypeId: system.businessTypeId,

                        interfaceId: system.interfaceId,
                        interfaceTypeId: system.interfaceTypeId,
                        tsaId: system.tsaId,
                        methodId: system.methodId,
                        dataRows: system.dataRows,
                    };

                    var msg = notify.addInfoMessage("Gemmer... ");
                    $http.post('api/itsystem', data, { handleBusy: true }).success(function () {
                        msg.toSuccessMessage("Systemet er gemt!");
                    }).error(function () {
                        msg.toErrorMessage("Fejl! Systemet kunne ikke gemmes!");
                    });
                };

                $scope.addDataRow = function (newDataRow) {
                    if (!newDataRow.data || !newDataRow.dataTypeId) return;

                    $scope.system.dataRows.push({ data: newDataRow.data, dataTypeId: newDataRow.dataTypeId });

                    $scope.newDataRow.data = "";
                    $scope.newDataRow.dataTypeId = "";
                };

                function selectLazyLoading(url) {
                    return {
                        minimumInputLength: 1,
                        initSelection: function (elem, callback) {
                        },
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
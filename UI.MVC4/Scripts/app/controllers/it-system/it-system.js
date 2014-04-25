(function (ng, app) {

    var subnav = [
            { state: 'index', text: 'Overblik' },
            { state: 'index', text: 'Tilknyt IT system' },
            { state: 'add-it-system', text: 'Opret IT system' },
            { state: 'index', text: 'Rapport' }
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('add-it-system', {
            url: '/system/add',
            templateUrl: 'partials/it-system/edit-system.html',
            controller: 'system.AddCtrl',
            resolve: {
                appTypes: ['$http', function ($http) {
                    return $http.get("api/apptype");
                }],
                interfaceAppType: ['$http', function ($http) {
                    return $http.get("api/apptype?interfaceAppType");
                }],
                businessTypes: ['$http', function ($http) {
                    return $http.get("api/businesstype");
                }],
                tsas: ['$http', function ($http) {
                    return $http.get("api/tsa");
                }],
                interfaces: ['$http', function ($http) {
                    return $http.get("api/interface");
                }],
                interfaceTypes: ['$http', function ($http) {
                    return $http.get("api/interfacetype");
                }],
                methods: ['$http', function ($http) {
                    return $http.get("api/method");
                }],
                dataTypes: ['$http', function ($http) {
                    return $http.get("api/datatype");
                }]
            }
        });

    }]);


    app.controller('system.AddCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            'appTypes', 'interfaceAppType', 'businessTypes', 'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes',
            function ($rootScope, $scope, $http, notify,
            appTypes, interfaceAppType, businessTypes, tsas, interfaces, interfaceTypes, methods, dataTypes) {
                $rootScope.page.title = 'Opret IT system';
                $rootScope.page.subnav = subnav;

                $scope.appTypes = appTypes.data.response;
                $scope.interfaceAppType = interfaceAppType.data.response;
                $scope.businessTypes = businessTypes.data.response;

                $scope.tsas = tsas.data.response;
                $scope.interfaces = interfaces.data.response;
                $scope.interfaceTypes = interfaceTypes.data.response;
                $scope.methods = methods.data.response;

                $scope.dataTypes = dataTypes.data.response;

                $scope.itSystemsSelectOptions = systemLazyLoading('nonInterfaces');
                $scope.itSystemsInterfacesOptions = systemLazyLoading('interfaces');

                $scope.system = { dataRows: [] };

                $scope.newDataRow = {};

                $scope.formData = {};

                // submit function
                $scope.saveSystem = function () {
                    var checkedTasks = _.filter($scope.allTasksFlat, function(task) {
                        return task.isChecked;
                    });
                    var checkedTaskIds = _.map(checkedTasks, function(task) {
                        return task.id;
                    });
                    $scope.formData.taskRefIds = checkedTaskIds;

                    $http.post('api/itsystem', $scope.formData).success(function() {
                        console.log('success');
                    });
                };

                $scope.addDataRow = function (newDataRow) {
                    if (!newDataRow.data || !newDataRow.dataType) return;

                    $scope.system.dataRows.push({ data: newDataRow.data, dataType: newDataRow.dataType });

                    $scope.newDataRow.data = "";
                    $scope.newDataRow.dataType = "";
                };

                function systemLazyLoading(urlExtra) {
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
                                var res = $http.get('api/itsystem?q=' + queryParams.data.query + "&" + urlExtra).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (system) {

                                    results.push({
                                        id: system.id,
                                        text: system.name
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }

                $http.get('api/taskref').success(function (result) {
                    $scope.kleFilter = {type:'KLE-Emne'}
                    $scope.allTasksFlat = result.response;
                });

                $scope.cleanKleFilter = function () {
                    if ($scope.kleFilter.parentId === null) {
                        delete $scope.kleFilter.parentId;
                    }
                };
            }]);
})(angular, app);
(function (ng, app) {

    var subnav = [
            { state: 'index', text: 'Overblik' },
            { state: 'index', text: 'Tilknyt IT system' },
            { state: 'add-it-system', text: 'Opret IT system' },
        { state: 'index', text: 'Rapport' },
        { state: 'it-system-usage', text: 'IT System' }
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $stateProvider
            .state('add-it-system', {
            url: '/system/add',
            templateUrl: 'partials/it-system/edit-system.html',
            controller: 'system.AddCtrl',
            resolve: {
                    appTypes: ['$http', function($http) {
                    return $http.get("api/apptype");
                }],
                    interfaceAppType: ['$http', function($http) {
                    return $http.get("api/apptype?interfaceAppType");
                }],
                    businessTypes: ['$http', function($http) {
                    return $http.get("api/businesstype");
                }],
                    tsas: ['$http', function($http) {
                    return $http.get("api/tsa");
                }],
                    interfaces: ['$http', function($http) {
                    return $http.get("api/interface");
                }],
                    interfaceTypes: ['$http', function($http) {
                    return $http.get("api/interfacetype");
                }],
                    methods: ['$http', function($http) {
                    return $http.get("api/method");
                }],
                    dataTypes: ['$http', function($http) {
                    return $http.get("api/datatype");
                }]
            }
            })
            .state('it-system-usage', {
                url: '/system/usage/{id:[0-9]+}',
                templateUrl: 'partials/it-system/edit-system-usage.html',
                controller: 'system.EditUsage',
                resolve: {
                    itSystemUsage: ['$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itsystemusage/' + $stateParams.id)
                            .then(function(result) {
                                return result.data.response;
                            });
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

                // submit function
                $scope.saveSystem = function () {

                    if (!$rootScope.user.currentOrganizationId) {
                        notify.addErrorMessage("Du har ikke valgt en organisation! Vælg en organisation i øverste højre hjørne");
                        return;
                    }

                    var system = $scope.system;

                    var checkedTasks = _.filter($scope.allTasksFlat, function(task) {
                        return task.isChecked;
                    });

                    /*var dataRows = _.map(system.dataRows, function(row) {
                        return { data: row.data, dataTypeId: row.dataType.id };
                    });*/
                    
                    var data = {
                        parentId: system.parent ? system.parent.id : null,
                        exposedById: system.exposedBy ? system.exposedBy.id : null,
                        canUseInterfaceIds: _.pluck(system.canUseInterfaces, 'id'),
                        belongsToId: system.belongsTo.id,
                        organizationId: $rootScope.user.currentOrganizationId,

                        version: system.version,
                        name: system.name,
                        userId: $rootScope.user.id,
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


                    $http.post('api/itsystem', data, {handleBusy: true}).success(function() {
                        console.log('success');
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

    app.controller('system.EditUsage', ['$rootScope', '$scope', '$http', 'notify', 'itSystemUsage', function ($rootScope, $scope, $http, notify, itSystemUsage) {
        $scope.usage = itSystemUsage;
    }]);
})(angular, app);
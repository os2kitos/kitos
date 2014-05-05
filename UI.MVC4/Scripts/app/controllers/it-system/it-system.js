(function (ng, app) {

    var subnav = [
            { state: 'overview-it-system', text: 'Overblik' },
            { state: 'assign-it-system', text: 'Tilknyt IT system' },
            { state: 'add-it-system', text: 'Opret IT system' },
            { state: 'index', text: 'Rapport' },
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function($stateProvider, $urlRouterProvider) {

        $stateProvider.state('overview-it-system', {
            url: '/system/overview',
            templateUrl: 'partials/it-system/overview-it-system.html',
            controller: 'system.OverviewCtrl',
            resolve: {
                appTypes: ['$http', function($http) {
                    return $http.get("api/apptype");
                }],
                businessTypes: ['$http', function($http) {
                    return $http.get("api/businesstype");
                }],
                organizations: ['$http', function($http) {
                    return $http.get("api/organization");
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        }).state('add-it-system', {
            url: '/system/add',
            templateUrl: 'partials/it-system/edit-it-system.html',
            controller: 'system.AddCtrl',
            resolve: {
                //resolve drop down datas
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
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        }).state('assign-it-system', {
            url: '/system/assign',
            templateUrl: 'partials/it-system/assign-it-system.html',
            controller: 'system.AssignCtrl',
            resolve: {
                appTypes: ['$http', function($http) {
                    return $http.get("api/apptype");
                }],
                businessTypes: ['$http', function($http) {
                    return $http.get("api/businesstype");
                }],
                organizations: ['$http', function($http) {
                    return $http.get("api/organization");
                }],
                systems: ['$http', function($http) {
                    return $http.get("api/itsystem");
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        }).state('it-system-usage', {
            url: '/system/usage/{id:[0-9]+}',
            templateUrl: 'partials/it-system/usage-it-system.html',
            controller: 'system.EditUsage',
            resolve: {
                appTypes: ['$http', function($http) {
                    return $http.get("api/apptype")
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                businessTypes: ['$http', function($http) {
                    return $http.get("api/businesstype")
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                archiveTypes: ['$http', function($http) {
                    return $http.get("api/archivetype")
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                sensitiveDataTypes: ['$http', function($http) {
                    return $http.get("api/sensitivedatatype")
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                itSystems: ['$http', function($http) {
                    return $http.get("api/itsystem/")
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                itSystemUsage: ['$http', '$stateParams', function($http, $stateParams) {
                    return $http.get('api/itsystemusage/' + $stateParams.id)
                        .then(function(result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.OverviewCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            'appTypes', 'businessTypes', 'organizations', 'user',
            function ($rootScope, $scope, $http, notify,
             appTypesHttp, businessTypesHttp, organizationsHttp, user) {
                $rootScope.page.title = 'IT Systemer overblik';
                $rootScope.page.subnav = subnav;

                var appTypes = appTypesHttp.data.response;
                var businessTypes = businessTypesHttp.data.response;
                var organizations = organizationsHttp.data.response;

                $scope.showSystemId = 'global';
                $scope.showType = 'appType';

                $scope.systemUsages = {};

                $http.get("api/itSystemUsage?organizationId=" + user.currentOrganizationId).success(function(result) {
                    $scope.systemUsages = result.response;

                    _.each(result.response, function(usage) {

                        usage.itSystem.appType = _.findWhere(appTypes, { id: usage.itSystem.appTypeId });
                        usage.itSystem.businessType = _.findWhere(businessTypes, { id: usage.itSystem.businessTypeId });

                        loadOverviewSystem(usage);

                    });
                    
                    function loadOverviewSystem(usage) {
                        if (!usage.overviewItSystem) return null;

                        return $http.get("api/itsystem/" + usage.overviewItSystemId).success(function (result) {
                            usage.overviewItSystem = result.response;
                        });
                    }

                });
            }]);

    app.controller('system.AddCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            'appTypes', 'interfaceAppType', 'businessTypes', 'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'user',
            function ($rootScope, $scope, $http, notify,
            appTypes, interfaceAppType, businessTypes, tsas, interfaces, interfaceTypes, methods, dataTypes, user) {
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
                        belongsToId: system.belongsTo.id,
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
                    }).error(function() {
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


    app.controller('system.AssignCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            'appTypes', 'businessTypes', 'systems', 'organizations', 'user',
            function ($rootScope, $scope, $http, notify,
             appTypesHttp, businessTypesHttp, systems, organizationsHttp, user) {
                $rootScope.page.title = 'Tilknyt IT system';
                $rootScope.page.subnav = subnav;

                $scope.showType = 'appType';
                
                var appTypes = appTypesHttp.data.response;
                var businessTypes = businessTypesHttp.data.response;
                var organizations = organizationsHttp.data.response;
                
                function loadUser(system) {
                    return $http.get("api/user/" + system.userId, { cache: true })
                        .success(function(result) {
                            system.user = result.response;
                        });
                }

                function loadOrganization(system) {
                    return $http.get("api/organization/" + system.organizationId, { cache: true })
                        .success(function (result) {
                            system.organization = result.response;
                        });
                }
                
                function loadTaskRef(system) {
                    if (system.taskRefIds.length == 0) return null;

                    return $http.get("api/taskref/" + system.taskRefIds[0])
                        .success(function(result) {
                            system.taskId = result.response.taskKey;
                            system.taskName = result.response.description;
                        });
                }
                
                function loadUsage(system) {
                    return $http.get(system.usageUrl)
                        .success(function(result) {
                            system.selected = true;
                        });
                }
                
                function addUsage(system) {
                    return $http.post("api/itsystemusage", {
                        itSystemId: system.id,
                        organizationId: user.currentOrganizationId
                    }).success(function(result) {
                        notify.addSuccessMessage("Systemet er tilknyttet");
                        system.selected = true;
                    }).error(function(result) {
                        notify.addErrorMessage("Systemet kunne ikke tilknyttes!");
                    });
                }
                
                function deleteUsage(system) {
                    
                    return $http.delete(system.usageUrl).success(function (result) {
                            notify.addSuccessMessage("Systemet er fjernet");
                            system.selected = false;
                    }).error(function (result) {
                        notify.addErrorMessage("Systemet kunne ikke fjernes!");
                    });
                }
                
                $scope.systems = [];
                _.each(systems.data.response, function (system) {
                    
                    system.appType = _.findWhere(appTypes, { id: system.appTypeId });
                    system.businessType = _.findWhere(businessTypes, { id: system.businessTypeId });

                    system.belongsTo = _.findWhere(organizations, { id: system.belongsToId });
                    
                    system.usageUrl = "api/itsystemusage?itSystemId=" + system.id + "&organizationId=" + user.currentOrganizationId;

                    loadUser(system);
                    loadOrganization(system);
                    loadTaskRef(system);
                    loadUsage(system);

                    system.toggle = function() {
                        if (system.selected) {
                            return deleteUsage(system);
                        } else {
                            return addUsage(system);
                        }
                    };

                    $scope.systems.push(system);
                });


            }]);

    app.controller('system.EditUsage', ['$rootScope', '$scope', '$http', '$stateParams', 'notify', 'itSystemUsage', 'appTypes',
        'businessTypes', 'archiveTypes', 'sensitiveDataTypes', 'itSystems',
        function ($rootScope, $scope, $http, $stateParams, notify, itSystemUsage, appTypes, businessTypes, archiveTypes, sensitiveDataTypes, itSystems) {
            $rootScope.page.title = 'Opret IT system';
            $rootScope.page.subnav = subnav.slice();
            $rootScope.page.subnav.push({ state: 'it-system-usage', text: 'IT System' });

            $scope.usageId = $stateParams.id;
            $scope.status = [{ id: true, name: 'Aktiv' }, { id: false, name: 'Inaktiv' }];
            $scope.appTypes = appTypes;
            $scope.businessTypes = businessTypes;
            $scope.archiveTypes = archiveTypes;
            $scope.sensitiveDataTypes = sensitiveDataTypes;
            $scope.itSystems = itSystems;
            $scope.usage = itSystemUsage;

            if (itSystemUsage.itSystem.parentId) {
                $scope.parentSystem = $http.get('api/itsystem/' + itSystemUsage.itSystem.parentId).then(function(result) {
                    return result.data.response;
                });
            }

            $scope.orgUnits = itSystemUsage.usedBy;
        }
    ]);
})(angular, app);
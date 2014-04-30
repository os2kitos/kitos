(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.interfaces', {
            url: '/interfaces',
            templateUrl: 'partials/it-system/tab-interfaces.html',
            controller: 'system.EditInterfaces',
            resolve: {
                tsas: ['$http', function ($http) {
                    return $http.get("api/tsa").then(function (result) {
                        return result.data.response;
                    });
                }],
                interfaces: ['$http', function ($http) {
                    return $http.get("api/interface").then(function (result) {
                        return result.data.response;
                    });
                }],
                interfaceTypes: ['$http', function ($http) {
                    return $http.get("api/interfacetype").then(function (result) {
                        return result.data.response;
                    });
                }],
                methods: ['$http', function ($http) {
                    return $http.get("api/method").then(function (result) {
                        return result.data.response;
                    });
                }],
                dataTypes: ['$http', function ($http) {
                    return $http.get("api/datatype").then(function (result) {
                        return result.data.response;
                    });
                }],
                interfaceCategories: ['$http', function ($http) {
                    return $http.get("api/interfaceCategory").then(function (result) {
                        return result.data.response;
                    });
                }],
                interfaceSystems: ['$http', function ($http) {
                    return $http.get("api/itsystem?interfaces").then(function (result) {
                        return result.data.response;
                    });
                }]
                
            }
        });
    }]);

    app.controller('system.EditInterfaces',
        ['$rootScope', '$scope', '$http', 'notify',
            'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes',
            'interfaceCategories', 'interfaceSystems', 'itSystemUsage',
            function ($rootScope, $scope, $http, notify,                 
                tsas, interfaces, interfaceTypes, methods, dataTypes,
                interfaceCategories, interfaceSystems, itSystemUsage) {

                $scope.exposedInterfaces = [];

                //EXPOSED INTERFACES
                $http.get("api/itsystem?itSystemId=" + itSystemUsage.itSystemId + "&getExposedInterfaces").success(function (result) {

                    var exposedInterfaces = result.response;

                    _.each(exposedInterfaces, function (system) {
                        system.interfaceType = _.findWhere(interfaceTypes, { id: system.interfaceTypeId });
                        system.interface = _.findWhere(interfaces, { id: system.interfaceId });
                        system.method = _.findWhere(methods, { id: system.methodId });
                        system.tsa = _.findWhere(tsas, { id: system.tsaId });

                        _.each(system.dataRows, function (dataRow) {
                            dataRow.dataType = _.findWhere(dataTypes, { id: dataRow.dataTypeId });
                        });

                    });

                    $scope.exposedInterfaces = exposedInterfaces;
                });


                //(CAN-BE) USED INTERFACES
                $scope.interfaces = interfaceSystems;
                _.each($scope.interfaces, function (interfaceSystem) {
                    
                    interfaceSystem.interfaceType = _.findWhere(interfaceTypes, { id: interfaceSystem.interfaceTypeId });
                    interfaceSystem.interface = _.findWhere(interfaces, { id: interfaceSystem.interfaceId });
                    interfaceSystem.method = _.findWhere(methods, { id: interfaceSystem.methodId });
                    interfaceSystem.tsa = _.findWhere(tsas, { id: interfaceSystem.tsaId });

                    _.each(interfaceSystem.dataRows, function (dataRow) {
                        dataRow.dataType = _.findWhere(dataTypes, { id: dataRow.dataTypeId });
                    });
                    
                    //can the interface be used by the selected IT system
                    interfaceSystem.canBeUsed = _.contains(interfaceSystem.canBeUsedByIds, itSystemUsage.itSystemId);
                    //is the interface currently used?
                    interfaceSystem.usage = _.findWhere(itSystemUsage.interfaceUsages, { interfaceId: interfaceSystem.id });

                    var rank = 3;
                    if (interfaceSystem.usage) rank -= 2;
                    if (interfaceSystem.canBeUsed) rank -= 1;

                    interfaceSystem.rank = rank;
                });

                $scope.toggleInterface = function(interfaceSystem) {

                    if (interfaceSystem.usage) {
                        $http.delete("api/interfaceUsage/" + interfaceSystem.usage.id).success(function(result) {
                            notify.addSuccessMessage("Snitfladen anvendes ikke længere");
                            delete interfaceSystem.usage;

                            var dataRowUsages = result.response.dataRows;
                            _.each(interfaceSystem.dataRows, function (dataRow) {
                                delete dataRow.usage;
                            });
                            
                        }).error(function(result) {
                            notify.addSuccessMessage("Fejl!");
                        });
                        
                    } else {
                        var data = {
                            itSystemusageId: itSystemUsage.id,
                            interfaceId: interfaceSystem.id
                        };

                        $http.post("api/interfaceUsage", data).success(function (result) {
                            notify.addSuccessMessage("Snitfladen er taget i lokal anvendelse");
                            interfaceSystem.usage = result.response;

                            var dataRowUsages = result.response.dataRowUsages;
                            _.each(interfaceSystem.dataRows, function(dataRow) {
                                dataRow.usage = _.findWhere(dataRowUsages, { 'dataRowId': dataRow.id });
                            });

                            console.log(interfaceSystem);

                        }).error(function (result) {
                            notify.addSuccessMessage("Fejl!");
                        });
                    }

                };


                $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem?nonInterfaces');
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
                

            }]);
})(angular, app);
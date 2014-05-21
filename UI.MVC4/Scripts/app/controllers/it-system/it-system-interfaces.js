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
                frequencies: ['$http', function ($http) {
                    return $http.get("api/frequency").then(function (result) {
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
            'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'frequencies',
            'interfaceCategories', 'interfaceSystems', 'itSystemUsage',
            function ($rootScope, $scope, $http, notify,                 
                tsas, interfaces, interfaceTypes, methods, dataTypes, frequencies,
                interfaceCategories, interfaceSystems, itSystemUsage) {

                $scope.interfaceCategories = interfaceCategories;
                $scope.frequencies = frequencies;
                
                $scope.filterInterfaces = function(interfaceSystem) {
                    return interfaceSystem.canBeUsed || interfaceSystem.usage || $scope.showAllInterfaces;
                };
                
                function interfaceFindTypes(theInterface) {
                    theInterface.interfaceType = _.findWhere(interfaceTypes, { id: theInterface.interfaceTypeId });
                    theInterface.interface = _.findWhere(interfaces, { id: theInterface.interfaceId });
                    theInterface.method = _.findWhere(methods, { id: theInterface.methodId });
                    theInterface.tsa = _.findWhere(tsas, { id: theInterface.tsaId });
                    
                    _.each(theInterface.dataRows, function (dataRow) {
                        dataRow.dataType = _.findWhere(dataTypes, { id: dataRow.dataTypeId });
                    });
                }
                
                function attachInterface(localUsageOrExposure) {
                    localUsageOrExposure.interface = _.findWhere(interfaceSystems, { id: localUsageOrExposure.interfaceId });

                    interfaceFindTypes(localUsageOrExposure.interface);
                }
                

                //Interface exposures
                _.each(itSystemUsage.interfaceExposures, function (interfaceExposure) {
                    interfaceExposure.updateUrl = "api/interfaceExposure/" + interfaceExposure.id;
                    attachInterface(interfaceExposure);
                });

                $scope.interfaceExposures = itSystemUsage.interfaceExposures;

                
                function attachUsage(interfaceSystem, usage) {
                    if (usage == null) return;

                    interfaceSystem.usage = usage;

                    if (usage.infrastructureId) {
                        interfaceSystem.usage.infrastructure = {
                            id: usage.infrastructureId,
                            text: usage.infrastructureName
                        };
                    }

                    var dataRowUsages = usage.dataRowUsages;
                    _.each(interfaceSystem.dataRows, function (dataRow) {
                        dataRow.usage = _.findWhere(dataRowUsages, { 'dataRowId': dataRow.id });
                    });
                }

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
                    var usage = _.findWhere(itSystemUsage.interfaceUsages, { interfaceId: interfaceSystem.id });
                    attachUsage(interfaceSystem, usage);

                    var rank = 3;
                    if (interfaceSystem.usage) rank -= 2;
                    if (interfaceSystem.canBeUsed) rank -= 1;

                    interfaceSystem.rank = rank;
                });

                $scope.toggleInterface = function(interfaceSystem) {

                    if (interfaceSystem.usage) {
                        $http.delete("api/interfaceUsage/" + interfaceSystem.usage.id).success(function(result) {
                            
                            delete interfaceSystem.usage;

                            _.each(interfaceSystem.dataRows, function (dataRow) {
                                delete dataRow.usage;
                            });

                            notify.addSuccessMessage("Snitfladen anvendes ikke længere");
                            
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

                            attachUsage(interfaceSystem, result.response);


                        }).error(function (result) {
                            notify.addSuccessMessage("Fejl!");
                        });
                    }

                };
                
                function patch(url, field, value) {

                    var data = {};
                    data[field] = value;
                    
                    return $http({ method: 'PATCH', url: url, data: data }).success(function(result) {
                        notify.addSuccessMessage("Feltet er opdateret");
                    }).error(function(result) {
                        notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!");
                    });
                }
                
                function patchUsage(usage, field, value) {

                    var url = "api/interfaceusage/" + usage.id;

                    return patch(url, field, value);
                }

                $scope.updateInfrastructure = function(usage) {
                    return patchUsage(usage, "infrastructureId", usage.infrastructure.id);
                };
                
                $scope.updateCategory = function (usage) {
                    return patchUsage(usage, "interfaceCategoryId", usage.interfaceCategoryId);
                };

                function patchDataRowUsage(dataRowUsage, field, value) {
                    var url = "api/dataRowUsage/" + dataRowUsage.id;

                    return patch(url, field, value);
                }
                
                $scope.updateFrequency = function(dataRowUsage) {
                    return patchDataRowUsage(dataRowUsage, "frequencyId", dataRowUsage.frequencyId);
                };
                $scope.updatePrice = function (dataRowUsage) {
                    return patchDataRowUsage(dataRowUsage, "price", parseInt(dataRowUsage.price));
                };
                $scope.updateAmount = function (dataRowUsage) {
                    return patchDataRowUsage(dataRowUsage, "amount", parseInt(dataRowUsage.amount));
                };
                $scope.updateEconomy = function (dataRowUsage) {
                    return patchDataRowUsage(dataRowUsage, "economy", parseInt(dataRowUsage.economy));
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
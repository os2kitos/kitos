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
                
                //resolving complex types from ids
                function resolveTypes(theInterface) {
                    theInterface.interfaceType = _.findWhere(interfaceTypes, { id: theInterface.interfaceTypeId });
                    theInterface.interface = _.findWhere(interfaces, { id: theInterface.interfaceId });
                    theInterface.method = _.findWhere(methods, { id: theInterface.methodId });
                    theInterface.tsa = _.findWhere(tsas, { id: theInterface.tsaId });
                    
                    _.each(theInterface.dataRows, function (dataRow) {
                        dataRow.dataType = _.findWhere(dataTypes, { id: dataRow.dataTypeId });
                    });
                }
                
                //Interface exposures
                _.each(itSystemUsage.interfaceExposures, function (interfaceExposure) {
                    interfaceExposure.updateUrl = "api/interfaceExposure/" + interfaceExposure.id;
                    resolveTypes(interfaceExposure.interface);
                });
                $scope.interfaceExposures = itSystemUsage.interfaceExposures;

                //Interface usages
                _.each(itSystemUsage.interfaceUsages, function(interfaceUsage) {
                    interfaceUsage.updateUrl = "api/interfaceUsage/" + interfaceUsage.id;
                    
                    //for the select2
                    if (interfaceUsage.infrastructureId) {
                        interfaceUsage.infrastructure = {
                            id: usage.infrastructureId,
                            text: usage.infrastructureName
                        };
                    }

                    _.each(interfaceUsage.dataRowUsages, function(dataRowUsage) {
                        dataRowUsage.updateUrl = "api/dataRowUsage/" + dataRowUsage.id;
                    });

                    resolveTypes(interfaceUsage.interface);
                });
                $scope.interfaceUsages = itSystemUsage.interfaceUsages;
               
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
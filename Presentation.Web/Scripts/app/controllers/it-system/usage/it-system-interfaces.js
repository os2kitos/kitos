(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.interfaces', {
            url: '/interfaces',
            templateUrl: 'partials/it-system/tab-interfaces.html',
            controller: 'system.EditInterfaces',
            resolve: {
                tsas: [
                    '$http', function($http) {
                        return $http.get('api/tsa').then(function(result) {
                            return result.data.response;
                        });
                    }
                ],
                interfaces: [
                    '$http', function($http) {
                        return $http.get('api/interface').then(function(result) {
                            return result.data.response;
                        });
                    }
                ],
                interfaceTypes: [
                    '$http', function($http) {
                        return $http.get('api/interfacetype').then(function(result) {
                            return result.data.response;
                        });
                    }
                ],
                methods: [
                    '$http', function($http) {
                        return $http.get('api/method').then(function(result) {
                            return result.data.response;
                        });
                    }
                ],
                dataTypes: [
                    '$http', function($http) {
                        return $http.get('api/datatype').then(function(result) {
                            return result.data.response;
                        });
                    }
                ],
                frequencies: [
                    '$http', function($http) {
                        return $http.get('api/frequency').then(function(result) {
                            return result.data.response;
                        });
                    }
                ],
                canUseInterfaces: [
                    '$http', 'itSystemUsage', function($http, itSystemUsage) {
                        return $http.get('api/itInterfaceUse/?interfaces&sysid=' + itSystemUsage.itSystem.id).then(function(result) {
                            var interfaces = result.data.response;
                            return interfaces;
                        });
                    }
                ],
                exhibits: [
                    '$http', 'itSystemUsage', 'user', function ($http, itSystemUsage, user) {
                        return $http.get('api/exhibit/?interfaces&sysid=' + itSystemUsage.itSystem.id + '&orgId=' + user.currentOrganizationId).then(function(result) {
                            var interfaces = result.data.response;
                            _.each(interfaces, function(data) {
                                $http.get('api/itInterfaceExhibitUsage/?usageId=' + itSystemUsage.id + '&exhibitId=' + data.exhibitedById).success(function(usageResult) {
                                    data.usage = usageResult.response;
                                });
                            });
                            return interfaces;
                        });
                    }
                ],
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('system.EditInterfaces',
    [
        '$rootScope', '$scope', '$http', 'notify',
        'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'frequencies', 'itSystemUsage', 'canUseInterfaces', 'exhibits',
        function($rootScope, $scope, $http, notify,
            tsas, interfaces, interfaceTypes, methods, dataTypes, frequencies, itSystemUsage, canUseInterfaces, exhibits) {

            $scope.frequencies = frequencies;

            $scope.filterInterfaces = function(interfaceSystem) {
                return interfaceSystem.canBeUsed || interfaceSystem.usage || $scope.showAllInterfaces;
            };

            // resolving complex types from ids
            function resolveTypes(theInterface) {
                theInterface.interfaceType = _.findWhere(interfaceTypes, { id: theInterface.interfaceTypeId });
                theInterface.interface = _.findWhere(interfaces, { id: theInterface.interfaceId });
                theInterface.method = _.findWhere(methods, { id: theInterface.methodId });
                theInterface.tsa = _.findWhere(tsas, { id: theInterface.tsaId });

                _.each(theInterface.dataRows, function(dataRow) {
                    dataRow.dataType = _.findWhere(dataTypes, { id: dataRow.dataTypeId });
                });

                theInterface.numRows = theInterface.dataRows.length;
            }

            // Interface exposures
            _.each(exhibits, function(interfaceExposure) {
                interfaceExposure.updateUrl = 'api/itInterfaceExhibitUsage/';
                interfaceExposure.urlParams = '&usageId=' + itSystemUsage.id + '&exhibitId=' + interfaceExposure.id;
                resolveTypes(interfaceExposure);
            });
            $scope.interfaceExposures = exhibits;

            // Interface usages
            _.each(canUseInterfaces, function(canUseInterface) {
                canUseInterface.updateUrl = 'api/interfaceUsage/';
                canUseInterface.urlParams = '&usageId=' + itSystemUsage.id + '&sysId=' + itSystemUsage.itSystem.id + '&interfaceId=' + canUseInterface.id;

                $http.get('api/interfaceUsage/?usageId=' + itSystemUsage.id + '&sysId=' + itSystemUsage.itSystem.id + '&interfaceId=' + canUseInterface.id).success(function (usageResult) {
                    var usage = usageResult.response;
                    canUseInterface.usage = usage;

                    // for the select2
                    if (usage.infrastructureId) {
                        canUseInterface.infrastructure = {
                            id: usage.infrastructureId,
                            text: usage.infrastructureItSystemName
                        };
                    }
                }).finally(function() {
                    _.each(canUseInterface.dataRows, function (dataRow) {
                        dataRow.updateUrl = 'api/dataRowUsage/';
                        dataRow.urlParams = '&rowId=' + dataRow.id + '&usageId=' + itSystemUsage.id + '&sysId=' + itSystemUsage.itSystem.id + '&interfaceId=' + canUseInterface.id;
                        dataRow.dataType = _.findWhere(dataTypes, { id: dataRow.dataTypeId });
                        if (canUseInterface.usage)
                            dataRow.usage = _.findWhere(canUseInterface.usage.dataRowUsages, { dataRowId: dataRow.id });
                    });
                });

                resolveTypes(canUseInterface);
            });
            $scope.interfaceUsages = canUseInterfaces;

            $scope.itSystemUsageSelectOptions = selectLazyLoading('api/itsystemusage', ['organizationId=' + itSystemUsage.organizationId]);
            function selectLazyLoading(url, paramAry) {
                return {
                    allowClear: true,
                    minimumInputLength: 1,
                    initSelection: function (elem, callback) {
                    },
                    ajax: {
                        data: function(term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function (queryParams) {
                            var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                            var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                            res.abort = function() {
                                return null;
                            };

                            return res;
                        },

                        results: function(data, page) {
                            var results = [];

                            _.each(data.data.response, function(obj) {
                                results.push({
                                    id: obj.id,
                                    text: obj.itSystem.name
                                });
                            });

                            return { results: results };
                        }
                    }
                };
            }
        }
    ]);
})(angular, app);

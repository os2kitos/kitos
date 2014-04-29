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
                exposedInterfaces: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itsystem?itSystemId=" + $stateParams.id + "&getExposedInterfaces")
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditInterfaces',
        ['$rootScope', '$scope', '$http', 'notify',
            'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'exposedInterfaces',
            function ($rootScope, $scope, $http, notify,                 
            tsas, interfaces, interfaceTypes, methods, dataTypes, exposedInterfaces) {

                $scope.exposedInterfaces = exposedInterfaces;
                _.each(exposedInterfaces, function (system) {
                    system.interfaceType = _.findWhere(interfaceTypes, { id: system.interfaceTypeId });
                    system.interface = _.findWhere(interfaces, { id: system.interfaceId });
                    system.method = _.findWhere(methods, { id: system.methodId });
                    system.tsa = _.findWhere(tsas, { id: system.tsaId });

                    _.each(system.dataRows, function(dataRow) {
                        dataRow.dataType = _.findWhere(dataTypes, { id: dataRow.dataTypeId });
                    });
                });


            }]);
})(angular, app);
(function (ng, app) {


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('it-system.edit.interface-details', {
            url: '/interface-details',
            templateUrl: 'partials/it-system/tab-edit-interface-details.html',
            controller: 'system.SystemInterfaceDetailsCtrl',
            resolve: {
                tsas: [
                    '$http', function ($http) {
                        return $http.get("api/tsa");
                    }
                ],
                interfaces: [
                    '$http', function ($http) {
                        return $http.get("api/interface");
                    }
                ],
                interfaceTypes: [
                    '$http', function ($http) {
                        return $http.get("api/interfacetype");
                    }
                ],
                methods: [
                    '$http', function ($http) {
                        return $http.get("api/method");
                    }
                ],
                dataTypes: [
                    '$http', function ($http) {
                        return $http.get("api/datatype");
                    }
                ]
            }
        });
    }]);

    app.controller('system.SystemInterfaceDetailsCtrl', ['$scope', '$http', 'notify', 'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'itSystem',
        function ($scope, $http, notify, tsas, interfaces, interfaceTypes, methods, dataTypes, itSystem) {

            $scope.tsas = tsas.data.response;
            $scope.interfaces = interfaces.data.response;
            $scope.interfaceTypes = interfaceTypes.data.response;
            $scope.methods = methods.data.response;

            $scope.dataTypes = dataTypes.data.response;

            itSystem.exposedByObj = !itSystem.exposedById ? null : { id: itSystem.exposedById, text: itSystem.exposedBy.name };
            
            //TODO wuff
            function selectLazyLoading(url, allowClear, filterObj) {
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

                                //optionally filter the obj
                                if (filterObj) {
                                    var newObj = filterObj(obj);
                                    
                                    //if that didn't filter it completely
                                    if (newObj) {
                                        results.push({
                                            id: newObj.id,
                                            text: newObj.name
                                        });
                                    }
                                } else {
                                    //or just push it directly
                                    results.push({
                                        id: obj.id,
                                        text: obj.name
                                    });
                                }
                            });

                            return { results: results };
                        }
                    }
                };
            }

        }]);

})(angular, app);
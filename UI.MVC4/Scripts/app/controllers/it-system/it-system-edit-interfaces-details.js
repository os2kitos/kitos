(function (ng, app) {
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $stateProvider.state('it-system.edit.interface-details', {
            url: '/interface-details',
            templateUrl: 'partials/it-system/tab-edit-interface-details.html',
            controller: 'system.SystemInterfaceDetailsCtrl',
            resolve: {
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

            $scope.select2AllowClearOpt = {
                allowClear: true
            };

            $scope.dataRows = [];
            _.each(itSystem.dataRows, pushDataRow);

            function pushDataRow(dataRow) {
                dataRow.show = true;
                $scope.dataRows.push(dataRow);

                dataRow.updateUrl = "api/dataRow/" + dataRow.id;
                dataRow.delete = function () {
                    var msg = notify.addInfoMessage("Fjerne rækken...", false);
                    $http.delete(dataRow.updateUrl).success(function(result) {
                        dataRow.show = false;
                        msg.toSuccessMessage("Rækken er fjernet!");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke fjerne rækken!");
                    });
                };
            }

            $scope.newDataRow = function() {

                var payload = { itSystemId: itSystem.id };

                var msg = notify.addInfoMessage("Tilføjer række...", false);
                $http.post("api/dataRow", payload).success(function(result) {
                    pushDataRow(result.response);
                    msg.toSuccessMessage("Rækken er tilføjet!");
                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke tilføje rækken!");
                });
            };
            
            $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem?nonInterfaces');
            function selectLazyLoading(url) {
                return {
                    allowClear: true,
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
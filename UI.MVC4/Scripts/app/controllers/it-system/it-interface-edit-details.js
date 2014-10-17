(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit.interface-details', {
            url: '/interface-details',
            templateUrl: 'partials/it-system/tab-edit-interface-details.html',
            controller: 'system.SystemInterfaceDetailsCtrl',
            resolve: {
                tsas: [
                    '$http', function($http) {
                        return $http.get('api/tsa').then(function (result) {
                            return result.data.response;
                        });
                    }
                ],
                interfaces: [
                    '$http', function($http) {
                        return $http.get('api/interface').then(function (result) {
                            return result.data.response;
                        });
                    }
                ],
                interfaceTypes: [
                    '$http', function($http) {
                        return $http.get('api/interfacetype').then(function (result) {
                            return result.data.response;
                        });
                    }
                ],
                methods: [
                    '$http', function($http) {
                        return $http.get('api/method').then(function (result) {
                            return result.data.response;
                        });
                    }
                ],
                dataTypes: [
                    '$http', function($http) {
                        return $http.get('api/datatype').then(function (result) {
                            return result.data.response;
                        });
                    }
                ],
                user: [
                        'userService', function (userService) {
                            return userService.getUser();
                        }
                ],
                dataRows: [
                    '$http', 'itInterface', function ($http, itInterface) {
                        return $http.get('api/datarow/?interfaceid=' + itInterface.id)
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ]
            }
        });
    }]);

    app.controller('system.SystemInterfaceDetailsCtrl', [
        '$scope', '$http', '$timeout', 'notify', 'tsas', 'interfaces', 'interfaceTypes', 'methods', 'dataTypes', 'user', 'itInterface', 'dataRows',
        function ($scope, $http, $timeout, notify, tsas, interfaces, interfaceTypes, methods, dataTypes, user, itInterface, dataRows) {

            $scope.tsas = tsas;
            $scope.interfaces = interfaces;
            $scope.interfaceTypes = interfaceTypes;
            $scope.methods = methods;
            $scope.dataTypes = dataTypes;
            $scope.exposedByObj = !itInterface.exhibitedByItSystemId ? null : { id: itInterface.exhibitedByItSystemId, text: itInterface.exhibitedByItSystemName };
            
            itInterface.updateUrl = 'api/itInterface/' + itInterface.id;
            $scope.system = itInterface;

            $scope.select2AllowClearOpt = {
                allowClear: true
            };

            $scope.dataRows = [];
            _.each(dataRows, pushDataRow);

            function pushDataRow(dataRow) {
                dataRow.show = true;
                $scope.dataRows.push(dataRow);

                dataRow.updateUrl = 'api/dataRow/' + dataRow.id;
                dataRow.delete = function() {
                    var msg = notify.addInfoMessage('Fjerner rækken...', false);
                    $http.delete(dataRow.updateUrl).success(function(result) {
                        dataRow.show = false;
                        msg.toSuccessMessage('Rækken er fjernet!');
                    }).error(function() {
                        msg.toErrorMessage('Fejl! Kunne ikke fjerne rækken!');
                    });
                };
            }

            $scope.newDataRow = function() {

                var payload = { itInterfaceId: itInterface.id };

                var msg = notify.addInfoMessage('Tilføjer række...', false);
                $http.post('api/dataRow', payload).success(function(result) {
                    pushDataRow(result.response);
                    msg.toSuccessMessage('Rækken er tilføjet!');
                }).error(function() {
                    msg.toErrorMessage('Fejl! Kunne ikke tilføje rækken!');
                });
            };

            $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem?', ['organizationId=' + user.currentOrganizationId]);

            function selectLazyLoading(url, paramAry) {
                return {
                    allowClear: true,
                    minimumInputLength: 1,
                    initSelection: function(elem, callback) {
                    },
                    ajax: {
                        data: function(term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function(queryParams) {
                            var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                            var res = $http.get(url + '&q=' + queryParams.data.query + extraParams).then(queryParams.success);
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
                                    text: obj.name
                                });
                            });

                            return { results: results };
                        }
                    }
                };
            }

            $scope.save = function () {
                // select2 is calling save twice,
                // checking for object will discard the incorrect call
                if (typeof $scope.exposedByObj !== 'object')
                    return;

                // check if this interface is exhibited by any system that is in use (itsystemusage)
                if (itInterface.isUsed) { // BUG this value only updates on reload
                    // clearing or changing the value must result in a dialog prompt
                    if (!$scope.exposedByObj) {
                        if (!confirm('Der er IT Systemer, som er i Lokal anvendelse som har denne snitfladerelation tilknyttet. Er du sikker på at du vil fjerne relationen?'))
                            return;
                    }
                    // TODO need previous value to prompt when value is changed (not cleared)
                }
                
                var msg = notify.addInfoMessage("Gemmer...", false);
                if ($scope.exposedByObj) {
                    // POST
                    var payload = {
                        id: itInterface.id,
                        itSystemId: $scope.exposedByObj.id
                    };
                    $http.post('api/exhibit', payload).success(function() {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });
                } else {
                    // DELETE
                    $http.delete('api/exhibit/' + itInterface.id).success(function() {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });
                }
            }
        }
    ]);
})(angular, app);

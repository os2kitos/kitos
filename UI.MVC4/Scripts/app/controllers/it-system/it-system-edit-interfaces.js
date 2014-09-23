(function(ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('it-system.edit.interfaces', {
                url: '/interfaces',
                templateUrl: 'partials/it-system/tab-edit-interfaces.html',
                controller: 'system.SystemInterfacesCtrl',
                resolve: {
                    interfaces: [
                        '$http', 'itSystem', function($http, itSystem) {
                            return $http.get('api/itInterfaceUse/?interfaces&sysId=' + itSystem.id).then(function(result) {
                                return result.data.response;
                            });
                        }
                    ],
                    user: [
                        'userService', function(userService) {
                            return userService.getUser();
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('system.SystemInterfacesCtrl', [
        '$scope', '$http', 'notify', 'itSystem', 'userService', 'interfaces', 'user',
        function ($scope, $http, notify, itSystem, userService, interfaces, user) {
            $scope.new = {};
            
            $scope.canUseInterfaces = [];
            _.each(interfaces, pushInterface);

            function pushInterface(theInterface) {
                if (!theInterface.deleted) theInterface.show = true;

                //method for removing the interface
                theInterface.delete = function() {
                    var msg = notify.addInfoMessage("Fjerner opmærkning...", false);
                    $http.delete('api/itInterfaceUse/?sysId=' + itSystem.id + '&interfaceId=' + theInterface.id).success(function() {
                        theInterface.show = false;
                        theInterface.deleted = true;
                        msg.toSuccessMessage("IT systemet er ikke længere opmærket med kan-anvende denne snitflade.");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke fjerne opmærkning!");
                    });
                };
                $scope.canUseInterfaces.push(theInterface);
            }

            $scope.addNewInterface = function() {
                if (!$scope.new.interface) return;

                var msg = notify.addInfoMessage("Gemmer opmærkning...", false);
                $http.post('api/itInterfaceUse/?sysId=' + itSystem.id + '&interfaceId=' + $scope.new.interface.id).success(function() {
                    pushInterface({ id: $scope.new.interface.id, name: $scope.new.interface.text });

                    msg.toSuccessMessage("IT systemet er opmærket med kan-anvende snitfladen");
                    $scope.new.interface = null;

                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke gemme opmærkning!");
                });
            };

            $scope.interfacesSelectOptions = selectLazyLoading('api/itInterface', false, ['sysId=' + itSystem.id, 'orgId=' + user.currentOrganizationId]);

            function selectLazyLoading(url, allowClear, paramAry) {
                return {
                    minimumInputLength: 1,
                    initSelection: function(elem, callback) {
                    },
                    allowClear: allowClear,
                    ajax: {
                        data: function(term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function(queryParams) {
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
                                    text: obj.name
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

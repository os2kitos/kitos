(function (ng, app) {


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('it-system.edit.interfaces', {
            url: '/interfaces',
            templateUrl: 'partials/it-system/tab-edit-interfaces.html',
            controller: 'system.SystemInterfacesCtrl',
            resolve: {
            }
        });
    }]);

    app.controller('system.SystemInterfacesCtrl', ['$scope', '$http', 'notify', 'itSystem',
        function ($scope, $http, notify, itSystem) {

            $scope.canUseInterfaces = itSystem.canUseInterfaces;

            _.each(itSystem.canUseInterfaces, pushInterface);
            
            function pushInterface(theInterface) {
                if (!theInterface.deleted) theInterface.show = true;

                //method for removing the interface
                theInterface.delete = function () {
                    var msg = notify.addInfoMessage("Fjerner opmærkning...", false);
                    $http.delete(itSystem.updateUrl + '?interfaceId=' + theInterface.id).success(function () {
                        theInterface.show = false;
                        theInterface.deleted = true;
                        msg.toSuccessMessage("IT systemet er ikke længere opmærket med kan-anvende denne snitflade.");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke fjerne opmærkning!");
                    });
                };

            }

            $scope.addNewInterface = function() {
                if (!$scope.newInterface) return;

                var msg = notify.addInfoMessage("Gemmer opmærkning...", false);
                $http.post(itSystem.updateUrl + '?interfaceId=' + $scope.newInterface.id).success(function (result) {
                    itSystem.canUseInterfaces.push(result.response);
                    pushInterface(result.response);
                    
                    msg.toSuccessMessage("IT systemet er opmærket med kan-anvende snitfladen");
                    $scope.newInterface = null;
                    
                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke gemme opmærkning!");
                });
            };
            
            $scope.interfacesSelectOptions = selectLazyLoading('api/itsystem?interfaces', false, function (obj) {
                //filter interfaces that's already assigned to the system
                if (_.findWhere($scope.canUseInterfaces, { id: obj.id, show: true })) return false;

                return obj;
            });
            
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
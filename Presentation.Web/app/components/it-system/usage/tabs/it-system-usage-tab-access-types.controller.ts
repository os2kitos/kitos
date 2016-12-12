(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.access-types', {
            url: '/contracts',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-access-types.view.html',
            controller: 'system.UsageAccessTypes',
            resolve: {
                accessTypes: [
                    '$http', 'itSystemUsage', function ($http, itSystemUsage) {
                        return $http.get("odata/ItSystems(" + itSystemUsage.itSystemId + ")?$select=Id&$expand=AccessTypes").then(function (result) {
                            return result.data.AccessTypes;
                        });
                    }
                ],
                activeAccessTypes: [
                    '$http', 'itSystemUsage', function ($http, itSystemUsage) {
                        return $http.get("odata/ItSystemUsages(" + itSystemUsage.id + ")?$select=Id&$expand=AccessTypes").then(function (result) {
                            return result.data.AccessTypes;
                        });
                    }
                ]
            }
        });
    }]);

    app.controller('system.UsageAccessTypes', ['$scope', '$http', '$state', '$stateParams', 'itSystemUsage', 'notify', 'accessTypes', 'activeAccessTypes',
        function ($scope, $http, $state, $stateParams, itSystemUsage, notify, accessTypes, activeAccessTypes) {
            var usageId = itSystemUsage.id;

            $scope.usage = itSystemUsage;

            $scope.accessTypes = accessTypes;

            console.log(activeAccessTypes);

            $scope.isActive = function (accessTypeId) {
                return _.find(activeAccessTypes, { Id: accessTypeId });
            }

            $scope.add = function (accessType) {
                var payload: any = {};
                payload["@odata.id"] = "https://localhost:44300/odata/AccessTypes(" + accessType.Id + ")";

                var msg = notify.addInfoMessage("Opdaterer ...", false);

                return $http.post("odata/ItSystemUsages(" + itSystemUsage.id  + ")/AccessTypes/$ref", payload).success(function (result) {
                    msg.toSuccessMessage("Feltet er opdateret!");
                }).error(function () {
                    msg.toErrorMessage("Fejl!");
                });
            }

            $scope.remove = function (id) {
                var msg = notify.addInfoMessage("Opdaterer ...", false);

                return $http.delete("odata/AccessTypes(" + id + ")").success(function (result) {
                    msg.toSuccessMessage("Feltet er slettet!");

                    _.remove($scope.accessTypes, function (o: any) {
                        return o.Id === id;
                    });
                }).error(function () {
                    msg.toErrorMessage("Fejl!");
                });
            }
        }]);
})(angular, app);

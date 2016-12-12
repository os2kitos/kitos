(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.access-types', {
            url: '/contracts',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-access-types.view.html',
            controller: 'system.UsageAccessTypes',
            resolve: {
                accessTypes: [
                    '$http', 'itSystem', function ($http, itSystem) {
                        return $http.get("odata/ItSystems(" + itSystem.id + ")?$select=Id&$expand=AccessTypes").then(function (result) {
                            return result.data.AccessTypes;
                        });
                    }
                ]
            }
        });
    }]);

    app.controller('system.UsageAccessTypes', ['$scope', '$http', '$state', '$stateParams', 'itSystemUsage', 'notify', 'accessTypes',
        function ($scope, $http, $state, $stateParams, itSystemUsage, notify, accessTypes) {
            var usageId = itSystemUsage.id;

            $scope.usage = itSystemUsage;

            $scope.accessTypes = accessTypes;

            console.log(accessTypes);

            $scope.add = function () {
                if ($scope.accessTypeName) {
                    var payload: any = {};
                    payload.Name = $scope.accessTypeName;
                    payload.ItSystemId = usageId;

                    var msg = notify.addInfoMessage("Opdaterer ...", false);

                    return $http.post("odata/AccessTypes", payload).success(function (result) {
                        msg.toSuccessMessage("Feltet er oprettet!");
                        $scope.accessTypes.push(result);
                        $scope.accessTypeName = "";
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });
                }
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

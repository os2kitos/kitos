(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-system.edit.access-types', {
                url: '/access-types',
                templateUrl: 'app/components/it-system/edit/tabs/it-system-edit-tab-access-types.view.html',
                controller: 'system.SystemAccessTypesCtrl',
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
        }
    ]);

    app.controller('system.SystemAccessTypesCtrl', [
        '$scope', '$http', '$state', 'notify', 'itSystem', 'user', "hasWriteAccess", "accessTypes",
        function ($scope, $http, $state, notify, itSystem, user, hasWriteAccess, accessTypes) {
            var systemId = itSystem.id;

            $scope.accessTypes = accessTypes;

            $scope.hasWriteAccess = hasWriteAccess;

            $scope.add = function () {
                if ($scope.accessTypeName) {
                    var payload: any = {};
                    payload.Name = $scope.accessTypeName;
                    payload.ItSystemId = systemId;

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

            $scope.remove = function(id) {
                var msg = notify.addInfoMessage("Opdaterer ...", false);

                return $http.delete("odata/AccessTypes(" + id + ")").success(function (result) {
                    msg.toSuccessMessage("Feltet er slettet!");

                    _.remove($scope.accessTypes, function (o:any) {
                        return o.Id === id;
                    });
                }).error(function (data, status) {
                    if (status === 409)
                        msg.toErrorMessage("Fejl! Kan ikke slettes, da den er i brug.");
                    else
                        msg.toErrorMessage('Fejl! Kunne ikke slette!');
                });
            }
        }
    ]);
})(angular, app);

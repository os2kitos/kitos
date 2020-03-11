//TODO: Not to be deleted yet KITOSUDV-526
//(function (ng, app) {
//    app.config(['$stateProvider', function ($stateProvider) {
//        $stateProvider.state('it-system.usage.access-control', {
//            url: '/access-control',
//            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-access-control.view.html',
//            controller: 'system.UsageAccessControl',
//            resolve: {
//                accessTypes: [
//                    '$http', 'itSystemUsage', function ($http, itSystemUsage) {
//                        return $http.get("odata/ItSystems(" + itSystemUsage.itSystemId + ")?$select=Id&$expand=AccessTypes").then(function (result) {
//                            return result.data.AccessTypes;
//                        });
//                    }
//                ],
//                activeAccessTypes: [
//                    '$http', 'itSystemUsage', function ($http, itSystemUsage) {
//                        return $http.get("odata/ItSystemUsages(" + itSystemUsage.id + ")?$select=Id&$expand=AccessTypes").then(function (result) {
//                            return result.data.AccessTypes;
//                        });
//                    }
//                ]
//            }
//        });
//    }]);

//    app.controller('system.UsageAccessControl', ['$scope', '$http', '$state', '$stateParams', 'itSystemUsage', 'notify', 'accessTypes', 'activeAccessTypes', 'hasWriteAccess',
//        function ($scope, $http, $state, $stateParams, itSystemUsage, notify, accessTypes, activeAccessTypes, hasWriteAccess) {
//            var usageId = itSystemUsage.id;

//            $scope.usage = itSystemUsage;
//            $scope.hasWriteAccess = hasWriteAccess;
//            $scope.accessTypes = accessTypes;

//            $scope.isActive = function (accessTypeId) {
//                return _.find(activeAccessTypes, { Id: accessTypeId });
//            }

//            $scope.toggle = function (accessType, e) {
//                if (e.currentTarget.checked) {
//                    var payload: any = {};
//                    payload["@odata.id"] = window.location.origin + "/odata/AccessTypes(" + accessType.Id + ")";

//                    var msg = notify.addInfoMessage("Opdaterer ...", false);

//                    return $http.post("odata/ItSystemUsages(" + itSystemUsage.id + ")/AccessTypes/$ref", payload).success(function (result) {
//                        msg.toSuccessMessage("Feltet er opdateret!");
//                    }).error(function () {
//                        msg.toErrorMessage("Fejl!");
//                    });
//                } else {
//                    var msg = notify.addInfoMessage("Opdaterer ...", false);

//                    return $http.delete("odata/ItSystemUsages(" + itSystemUsage.id + ")/AccessTypes/$ref?$id=" + window.location.origin + "/odata/AccessTypes(" + accessType.Id + ")").success(function (result) {
//                        msg.toSuccessMessage("Feltet er opdateret!");
//                    }).error(function () {
//                        msg.toErrorMessage("Fejl!");
//                    });
//                }
//            }
//        }]);
//})(angular, app);

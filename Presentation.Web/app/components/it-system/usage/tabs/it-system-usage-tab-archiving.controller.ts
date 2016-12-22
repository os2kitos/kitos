((ng, app) => {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.archiving', {
            url: '/archiving',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-archiving.view.html',
            controller: 'system.EditArchiving',
            resolve: {
                archiveTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalArchiveTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                archiveLocations: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalArchivelocations?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                systemUsage: [
                    '$http', '$stateParams', ($http, $stateParams) =>
                        $http.get(`odata/itSystemUsages(${$stateParams.id})`)
                            .then(result => result.data)
                ]
            }
        });
    }]);

    app.controller('system.EditArchiving', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'itSystemUsage', 'itSystemUsageService', 'archiveTypes', 'archiveLocations', 'systemUsage', 'notify',
        ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, itSystemUsageService, archiveTypes, archiveLocations, systemUsage, notify) => {
            var usageId = itSystemUsage.id;

            $scope.usage = itSystemUsage;
            $scope.archiveTypes = archiveTypes;
            $scope.archiveLocations = archiveLocations;
            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;

            $scope.patch = (field, value) => {
                var payload = {};
                payload[field] = value;
                itSystemUsageService.patchSystem($scope.usageId, payload);
            }

            $scope.patchDate = (field, value) => {
                var date = value.split("-").reverse().join("-");
                var payload = {};
                payload[field] = date;
                itSystemUsageService.patchSystem($scope.usageId, payload);
            }

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };
        }]);

})(angular, app);

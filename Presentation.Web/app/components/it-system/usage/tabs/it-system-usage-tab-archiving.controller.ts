((ng, app) => {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.archiving", {
            url: "/archiving",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-archiving.view.html",
            controller: "system.EditArchiving",
            resolve: {
                archiveTypes: [
                    "$http", function ($http) {
                        return $http.get("odata/LocalArchiveTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                archiveLocations: [
                    "$http", function ($http) {
                        return $http.get("odata/LocalArchivelocations?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                systemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                        $http.get(`odata/itSystemUsages(${$stateParams.id})`)
                            .then(result => result.data)
                ]
            }
        });
    }]);

    app.controller("system.EditArchiving", ["$scope", "$http", "$state", "$stateParams", "$timeout", "itSystemUsage", "itSystemUsageService", "archiveTypes", "archiveLocations", "systemUsage", "moment", "notify",
        ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, itSystemUsageService, archiveTypes, archiveLocations, systemUsage, moment, notify) => {
            $scope.usage = itSystemUsage;
            $scope.archiveTypes = archiveTypes;
            $scope.archiveLocations = archiveLocations;
            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;
            $scope.ArchivedDate = systemUsage.ArchivedDate;

            $scope.patch = (field, value) => {
                var payload = {};
                payload[field] = value;
                itSystemUsageService.patchSystem($scope.usageId, payload);
            }

            $scope.patchDate = (field, value) => {
                var date = moment(value);

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    $scope.ArchivedDate = systemUsage.ArchivedDate;
                } else {
                    date = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = date;
                    itSystemUsageService.patchSystem($scope.usageId, payload);
                    $scope.ArchivedDate = date;
                }
            }

            $scope.datepickerOptions = {
                format: "yyyy-MM-dd"
            };
        }]);

})(angular, app);

(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.interfaces", {
            url: "/interfaces",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-interfaces.view.html",
            controller: "system.EditInterfaces",
            resolve: {
                interfaces: [
                    "$http", function ($http) {
                        return $http.get("odata/LocalInterfaceTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(function (result) {
                            return result.data.value;
                        });
                    }
                ],
                dataTypes: [
                    "$http", function ($http) {
                        return $http.get("odata/LocalDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(function (result) {
                            return result.data.value;
                        });
                    }
                ],
                frequencies: [
                    "$http", function ($http) {
                        return $http.get("odata/LocalFrequencyTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(function (result) {
                            return result.data.value;
                        });
                    }
                ],
                exhibits: [
                    "$http", "itSystemUsage", "user", function ($http, itSystemUsage, user) {
                        return $http.get("api/exhibit/?interfaces=true&sysId=" + itSystemUsage.itSystem.id + "&orgId=" + user.currentOrganizationId).then(function (result) {
                            var interfaces = result.data.response;
                            _.each(interfaces, function (data: { exhibitedById; usage; }) {
                                $http.get("api/itInterfaceExhibitUsage/?usageId=" + itSystemUsage.id + "&exhibitId=" + data.exhibitedById).success(function (usageResult) {
                                    data.usage = usageResult.response;
                                });
                            });
                            return interfaces;
                        });
                    }
                ],
                user: [
                    "userService", function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller("system.EditInterfaces", ["$rootScope", "$scope", "$http", "notify","interfaces", "dataTypes", "frequencies", "itSystemUsage", "exhibits",
        function ($rootScope, $scope, $http, notify, interfaces, dataTypes, frequencies, itSystemUsage, exhibits) {

            $scope.frequencies = frequencies;

            $scope.filterInterfaces = function (interfaceSystem) {
                return interfaceSystem.canBeUsed || interfaceSystem.usage || $scope.showAllInterfaces;
            };

            // resolving complex types from ids
            function resolveTypes(theInterface) {
                theInterface.interface = _.find(interfaces, { Id: theInterface.interfaceId });

                _.each(theInterface.dataRows, function (dataRow: { dataTypeId; dataType; }) {
                    dataRow.dataType = _.find(dataTypes, { Id: dataRow.dataTypeId });
                });

                theInterface.numRows = theInterface.dataRows.length;
            }

            // interface exposures
            _.each(exhibits, function (interfaceExposure: { updateUrl; urlParams; id; }) {
                interfaceExposure.updateUrl = "api/itInterfaceExhibitUsage/";
                interfaceExposure.urlParams = "&usageId=" + itSystemUsage.id + "&exhibitId=" + interfaceExposure.id;
                resolveTypes(interfaceExposure);
            });
            $scope.interfaceExposures = exhibits;

            $scope.itSystemUsageSelectOptions = selectLazyLoading("api/itSystemUsage", ["organizationId=" + itSystemUsage.organizationId]);
            function selectLazyLoading(url, paramAry) {
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
                            var extraParams = paramAry ? "&" + paramAry.join("&") : "";
                            var res = $http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success);
                            res.abort = function () {
                                return null;
                            };

                            return res;
                        },

                        results: function (data, page) {
                            var results = [];

                            _.each(data.data.response, function (obj: { id; itSystem; }) {
                                results.push({
                                    id: obj.id,
                                    text: obj.itSystem.name
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

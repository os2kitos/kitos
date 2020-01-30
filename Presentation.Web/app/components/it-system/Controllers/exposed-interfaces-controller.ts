((ng, app) => {
    app.controller("system.ExposedInterfaces", ["$scope", "$http", "tsas", "interfaces", "interfaceTypes", "methods", "dataTypes", "frequencies", "systemUsageId", "exhibits", "user",
        ($scope, $http, tsas, interfaces, interfaceTypes, methods, dataTypes, frequencies, systemUsageId, exhibits, user) => {

            $scope.frequencies = frequencies;

            $scope.filterInterfaces = interfaceSystem => interfaceSystem.canBeUsed || interfaceSystem.usage || $scope.showAllInterfaces;

            // resolving complex types from ids
            function resolveTypes(theInterface) {
                theInterface.interfaceType = _.find(interfaceTypes, { Id: theInterface.interfaceTypeId });
                theInterface.interface = _.find(interfaces, { Id: theInterface.interfaceId });
                theInterface.method = _.find(methods, { Id: theInterface.methodId });
                theInterface.tsa = _.find(tsas, { Id: theInterface.tsaId });

                _.each(theInterface.dataRows, (dataRow: { dataTypeId; dataType; }) => {
                    dataRow.dataType = _.find(dataTypes, { Id: dataRow.dataTypeId });
                });

                theInterface.numRows = theInterface.dataRows.length;
            }

            // interface exposures
            _.each(exhibits, (interfaceExposure: { updateUrl; urlParams; id; }) => {
                interfaceExposure.updateUrl = "api/itInterfaceExhibitUsage/";
                interfaceExposure.urlParams = `&usageId=${systemUsageId}&exhibitId=${interfaceExposure.id}`;
                resolveTypes(interfaceExposure);
            });
            $scope.interfaceExposures = exhibits;

            $scope.itSystemUsageSelectOptions = selectLazyLoading("api/itSystemUsage", [`organizationId=${user.organizationId}`]);
            function selectLazyLoading(url, paramAry) {
                return {
                    allowClear: true,
                    minimumInputLength: 1,
                    initSelection: (elem, callback) => {
                    },
                    ajax: {
                        data: (term, page) => ({ query: term }),
                        quietMillis: 500,
                        transport: queryParams => {
                            var extraParams = paramAry ? "&" + paramAry.join("&") : "";
                            var res = $http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success);
                            res.abort = () => null;

                            return res;
                        },

                        results: (data, page) => {
                            var results = [];

                            _.each(data.data.response, (obj: { id; itSystem; }) => {
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

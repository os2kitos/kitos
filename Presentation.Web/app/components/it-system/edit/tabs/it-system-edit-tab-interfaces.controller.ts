((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.edit.interfaces", {
            url: "/interfaces",
            templateUrl: "app/components/it-system/edit/tabs/it-system-edit-tab-interfaces.view.html",
            controller: "system.ExposedInterfaces",
            resolve: {
                exhibits: [
                    "$http", "itSystem", "user", ($http, itSystem, user) =>
                        $http.get(`api/exhibit/?interfaces=true&sysId=${itSystem.id}&orgId=${user.currentOrganizationId}`).then(result => {
                            return result.data.response;
                        })
                ],
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);

    app.controller("system.ExposedInterfaces", ["$scope", "$http", "itSystem", "exhibits", "user",
        ($scope, $http, itSystem, exhibits, user) => {

            $scope.system = itSystem;
            
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
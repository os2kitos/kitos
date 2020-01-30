((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.interfaces", {
            url: "/interfaces",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-interfaces.view.html",
            controller: "systemUsage.ExposedInterfaces",
            resolve: {
                exhibits: [
                    "$http", "itSystemUsage", "user", ($http, itSystemUsage, user) =>
                        $http.get(`api/exhibit/?interfaces=true&sysId=${itSystemUsage.itSystem.id}&orgId=${user.currentOrganizationId}`).then(result => {
                            var interfaces = result.data.response;
                            _.each(interfaces, (data: { exhibitedById; usage; }) => {
                                $http.get(`api/itInterfaceExhibitUsage/?usageId=${itSystemUsage.id}&exhibitId=${data.exhibitedById}`).success(usageResult => {
                                    data.usage = usageResult.response;
                                });
                            });
                            return interfaces;
                        })
                ],
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);

    app.controller("systemUsage.ExposedInterfaces", ["$scope", "itSystemUsage", "exhibits",
        ($scope, itSystemUsage, exhibits) => {

            $scope.systemUsage = itSystemUsage;

            // Enables marking "wished" 
            _.each(exhibits, (interfaceExposure: { updateUrl; urlParams; id; }) => {
                interfaceExposure.updateUrl = "api/itInterfaceExhibitUsage/";
                interfaceExposure.urlParams = `&usageId=${itSystemUsage.id}&exhibitId=${interfaceExposure.id}`;
            });

            $scope.interfaceExposures = exhibits;
        }
    ]);

})(angular, app);

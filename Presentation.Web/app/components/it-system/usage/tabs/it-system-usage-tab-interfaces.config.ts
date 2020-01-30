((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.interfaces", {
            url: "/interfaces",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-interfaces.view.html",
            controller: "system.ExposedInterfaces",
            resolve: {
                tsas: [
                    "$http", $http => $http.get("odata/LocalTsaTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                ],
                interfaces: [
                    "$http", $http => $http.get("odata/LocalInterfaceTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                ],
                interfaceTypes: [
                    "$http", $http => $http.get("odata/LocalItInterfaceTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                ],
                methods: [
                    "$http", $http => $http.get("odata/LocalMethodTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                ],
                dataTypes: [
                    "$http", $http => $http.get("odata/LocalDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                ],
                frequencies: [
                    "$http", $http => $http.get("odata/LocalFrequencyTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                ],
                systemUsageId: [
                    "itSystemUsage", (itSystemUsage) => itSystemUsage.id
                ],
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

})(angular, app);

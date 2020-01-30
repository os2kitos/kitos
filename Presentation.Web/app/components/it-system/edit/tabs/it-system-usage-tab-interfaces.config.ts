((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.edit.interfaces", {
            url: "/interfaces",
            templateUrl: "app/components/it-system/edit/tabs/it-system-usage-tab-interfaces.view.html",
            controller: "system.ExposedInterfaces",
            resolve: {
                tsas: [
                    "$http",
                    $http => $http
                        .get(
                            "odata/LocalTsaTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(result => result.data.value)
                ],
                interfaces: [
                    "$http",
                    $http => $http
                        .get(
                            "odata/LocalInterfaceTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(result => result.data.value)
                ],
                interfaceTypes: [
                    "$http",
                    $http => $http
                        .get(
                            "odata/LocalItInterfaceTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(result => result.data.value)
                ],
                methods: [
                    "$http",
                    $http => $http
                        .get(
                            "odata/LocalMethodTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(result => result.data.value)
                ],
                dataTypes: [
                    "$http",
                    $http => $http
                        .get(
                            "odata/LocalDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(result => result.data.value)
                ],
                frequencies: [
                    "$http",
                    $http => $http
                        .get(
                            "odata/LocalFrequencyTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(result => result.data.value)
                ],
                systemUsageId: [
                    "$http", "itSystem", "user",
                    ($http, itSystem, user) => $http
                        .get(`api/itSystemUsage/?itSystemId=${itSystem.id}&organizationId=${user.currentOrganizationId}`)
                        .then(systemUsageResult => systemUsageResult.data.response.id)
                        .catch(response => {
                            if (response.status === 404)
                                return 0;
                            throw "unexpected error when trying to get specific it system";
                        })
                ],
                exhibits: [
                    "$http", "itSystem", "user", "systemUsageId", ($http, itSystem, user, systemUsageId) =>
                        $http.get(`api/exhibit/?interfaces=true&sysId=${itSystem.id}&orgId=${user.currentOrganizationId}`).then(result => {
                            var interfaces = result.data.response;
                            _.each(interfaces, (data: { exhibitedById; usage; }) => {
                                if (systemUsageId !== 0) {
                                    $http.get(`api/itInterfaceExhibitUsage/?usageId=${systemUsageId}&exhibitId=${data.exhibitedById}`).success(usageResult => {
                                        data.usage = usageResult.response;
                                    });
                                }
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

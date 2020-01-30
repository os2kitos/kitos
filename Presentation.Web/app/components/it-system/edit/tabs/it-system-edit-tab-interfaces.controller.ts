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

    app.controller("system.ExposedInterfaces", ["$scope", "itSystem", "exhibits", "user", "lazyLoadingService",
        ($scope, itSystem, exhibits, user, lazyLoadingService) => {

            $scope.system = itSystem;
            
            $scope.interfaceExposures = exhibits;

            $scope.itSystemUsageSelectOptions = lazyLoadingService.selectLazyLoading("api/itSystemUsage", [`organizationId=${user.organizationId}`]);
            
            
        }
    ]);
})(angular, app);
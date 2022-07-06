((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.dataprocessing", {
            url: "/dataprocessing",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-data-processing.view.html",
            controller: "systemUsage.DataProcessing",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);

    app.controller("systemUsage.DataProcessing", ["$scope",
        ($scope) => {
            const itSystemUsage= $scope.usage;
            $scope.dataProcessingRegistrations = itSystemUsage.associatedDataProcessingRegistrations;
        }
    ]);
})(angular, app);
((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.dataProcessorAgreement", {
            url: "/DataProcessorAgreement",
            templateUrl: "app/components/local-config/local-config-dataProcessorAgreement.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigDataProcessorAgreement",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);
    app.controller("localConfigDataProcessorAgreement", ["$scope", "user", ($scope, user) => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
        $scope.currentOrganizationId = user.currentOrganizationId;
    }]);
})(angular, app);

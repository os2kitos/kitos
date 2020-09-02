((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.dataProcessingAgreement", {
            url: "/DataProcessingAgreement",
            templateUrl: "app/components/local-config/local-config-dataProcessingAgreement.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigDataProcessingAgreement",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);
    app.controller("localConfigDataProcessingAgreement", ["$scope", "user", ($scope, user) => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
        $scope.currentOrganizationId = user.currentOrganizationId;
    }]);
})(angular, app);

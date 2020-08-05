((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.contract", {
            url: "/contract",
            templateUrl: "app/components/local-config/local-config-contract.view.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigContract",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);
    app.controller("localConfigContract", ["$scope", "user", ($scope, user) => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
        $scope.currentOrganizationId = user.currentOrganizationId;
    }]);
})(angular, app);

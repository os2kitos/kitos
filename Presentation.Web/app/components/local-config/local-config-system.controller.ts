((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.system", {
            url: "/system",
            templateUrl: "app/components/local-config/local-config-system.view.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigSystem",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);
    app.controller("localConfigSystem", ["$scope", "user", ($scope, user) => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
        $scope.currentOrganizationId = user.currentOrganizationId;
    }]);
})(angular, app);

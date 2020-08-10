((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.project", {
            url: "/project",
            templateUrl: "app/components/local-config/local-config-project.view.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigProject",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);
    app.controller("localConfigProject", ["$scope", "user", ($scope, user) => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
        $scope.currentOrganizationId = user.currentOrganizationId;
    }]);
})(angular, app);
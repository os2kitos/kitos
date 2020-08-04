((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.project", {
            url: "/project",
            templateUrl: "app/components/local-config/local-config-project.view.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigProject"
        });
    }]);
    app.controller("localConfigProject", ["$scope", $scope => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
    }]);
})(angular, app);
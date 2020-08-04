((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.system", {
            url: "/system",
            templateUrl: "app/components/local-config/local-config-system.view.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigSystem"
        });
    }]);
    app.controller("localConfigSystem", ["$scope", $scope => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
    }]);
})(angular, app);

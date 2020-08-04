((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.contract", {
            url: "/contract",
            templateUrl: "app/components/local-config/local-config-contract.view.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "localConfigContract"
        });
    }]);
    app.controller("localConfigContract", ["$scope", $scope => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
    }]);
})(angular, app);

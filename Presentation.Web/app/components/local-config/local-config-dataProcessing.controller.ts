((ng,app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.data-processing", {
            url: "/data-processing",
            templateUrl: "app/components/local-config/local-config-dataProcessing.view.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
            controller: "data-processing-local-config"
        });
    }]);
    app.controller("data-processing-local-config", ["$scope", "user", ($scope, user) => {
        $scope.localOptionType = Kitos.Services.LocalOptions.LocalOptionType;
        $scope.currentOrganizationId = user.currentOrganizationId;
    }]);
})(angular, app);

((ng,app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.dataProcessing", {
            url: "/DataProcessing",
            templateUrl: "app/components/local-config/local-config-dataProcessing.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
        });
    }]);
})(angular, app);

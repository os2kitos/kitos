((ng,app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("local-config.dataProcessingAgreement", {
            url: "/DataProcessingAgreement",
            templateUrl: "app/components/local-config/local-config-dataProcessingAgreement.html",
            authRoles: [Kitos.Models.OrganizationRole.LocalAdmin],
        });
    }]);
})(angular, app);

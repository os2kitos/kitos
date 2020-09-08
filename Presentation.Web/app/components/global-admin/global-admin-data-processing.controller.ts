((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.data-processing", {
            url: "/data-processing",
            templateUrl: "app/components/global-admin/global-admin-data-processing.view.html",
            authRoles: ["GlobalAdmin"]
        });
    }]);
})(angular, app);
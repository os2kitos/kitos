(function (ng, app) {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.report", {
            url: "/report",
            templateUrl: "app/components/global-admin/global-admin-report.view.html",
            authRoles: ["GlobalAdmin"]
        });
    }]);
})(angular, app);

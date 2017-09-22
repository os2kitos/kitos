(function (ng, app) {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.project", {
            url: "/project",
            templateUrl: "app/components/global-admin/global-admin-project.view.html",
            authRoles: ["GlobalAdmin"]
        });
    }]);
})(angular, app);

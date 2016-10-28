(function (ng, app) {

    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.org", {
            url: "/org",
            templateUrl: "app/components/global-admin/global-admin-org.view.html",
            authRoles: ["GlobalAdmin"]
        });
    }]);

})(angular, app);

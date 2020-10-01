((ng, app) => {

    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.system", {
            url: "/system",
            templateUrl: "app/components/global-admin/global-admin-system.view.html",
            authRoles: ["GlobalAdmin"]
        });
    }]);

})(angular, app);

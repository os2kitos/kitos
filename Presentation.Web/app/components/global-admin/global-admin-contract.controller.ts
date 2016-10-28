(function(ng, app) {

    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.contract", {
            url: "/contract",
            templateUrl: "app/components/global-admin/global-admin-contract.view.html",
            authRoles: ["GlobalAdmin"]
        });
    }]);

})(angular, app);

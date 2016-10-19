((ng, app) => {

    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("config", {
            url: "/global-config",
            abstract: true,
            controller: "globalConfig",
            template: "<ui-view autoscroll='false' />"
        });
    }]);

    app.controller("globalConfig", ["$rootScope", $rootScope => {
        var subnav = [
            { state: "config.org", text: "Organisation" },
            { state: "config.project", text: "IT Projekt" },
            { state: "config.system", text: "IT System" },
            { state: "config.contract", text: "IT Kontrakt" }
        ];

        $rootScope.page.title = "Global konfiguration";
        $rootScope.page.subnav = subnav;
    }]);

})(angular, app);

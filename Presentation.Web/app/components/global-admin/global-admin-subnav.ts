(function (ng, app) {

    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin", {
            url: "/global-admin",
            abstract: true,
            controller: "globalAdminConfig",
            template: "<ui-view autoscroll='false' />"
        });
    }]);

    app.controller("globalAdminConfig", ["$rootScope", "$scope", ($rootScope, $scope) => {
        var subnav = [
            { state: "global-admin.organizations", text: "Organisationer" },
            { state: "global-admin.global-users", text: "Globale administratorer" },
            { state: "global-admin.local-users", text: "Lokale administratorer" },
            { state: "global-admin.org", text: "Organisation" },
            { state: "global-admin.project", text: "IT Projekt" },
            { state: "global-admin.system", text: "IT System" },
            { state: "global-admin.contract", text: "IT Kontrakt" },
            { state: "global-admin.data-processing", text: "Databehandling" },
            { state: "global-admin.report", text: "Rapport" },
            { state: "global-admin.misc", text: "Andet" },
            { state: "global-admin.help-texts", text: "Hjælpetekster" }
        ];

        $rootScope.page.title = "Global admin";
        $rootScope.page.subnav = subnav;
        $rootScope.subnavPositionCenter = true;

        $scope.$on('$viewContentLoaded', function () {
            $rootScope.positionSubnav();
        });
    }]);

})(angular, app);

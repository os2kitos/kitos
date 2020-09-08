((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("data-processing", {
            url: "/data-processing",
            abstract: true,
            template: "<ui-view autoscroll=\"false\" />"
            ,
            controller: ["$rootScope", "$scope", ($rootScope, $scope) => {
                $rootScope.page.title = "Databehandling";
                $rootScope.page.subnav = [
                    { state: "data-processing.overview", text: "Databehandleraftaler" }
                ];

                $rootScope.subnavPositionCenter = false;

                $scope.$on("$viewContentLoaded", () => {
                    $rootScope.positionSubnav();
                });
            }]
        });
    }]);
})(angular, app);

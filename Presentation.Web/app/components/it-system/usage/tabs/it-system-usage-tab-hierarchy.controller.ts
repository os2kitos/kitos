((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.hierarchy", {
            url: "/hierarchy",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-hierarchy.view.html",
            controller: "system.EditHierarchy",
            resolve: {
                hierarchyFlat: ["$http", "itSystemUsage", ($http, itSystemUsage) => $http.get(`api/itsystem/${itSystemUsage.itSystemId}?hierarchy=true`).then(result => result.data.response)]
            }
        });
    }]);

    app.controller("system.EditHierarchy",
        ["$scope", "_", "hierarchyFlat", "itSystemUsage",
            ($scope, _, hierarchyFlat, itSystemUsage) => {
                $scope.systems = _.toHierarchy(hierarchyFlat, "id", "parentId", "children");
                $scope.usage = itSystemUsage;
                $scope.formatSystemName = Kitos.Helpers.SystemNameFormat.apply;
            }
        ]
    );
})(angular, app);

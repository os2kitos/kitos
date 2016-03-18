((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-project.edit.hierarchy", {
            url: "/hierarchy",
            templateUrl: "app/components/it-project/tabs/it-project-tab-hierarchy.view.html",
            controller: "project.EditHierarchyCtrl",
            resolve: {
                hierarchyFlat: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itproject/${$stateParams.id}?hierarchy`).then(result => result.data.response)]
            }
        });
    }]);

    app.controller("project.EditHierarchyCtrl",
    ["$scope", "_", "hierarchyFlat",
        ($scope, _, hierarchyFlat) => {
            $scope.hierarchy = _.toHierarchy(hierarchyFlat, "id", "parentId", "children");
        }]);
})(angular, app);

((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.hierarchy", {
            url: "/hierarchy",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-hierarchy.view.html",
            controller: "contract.EditHierarchyCtrl",
            resolve: {
                hierarchyFlat: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itcontract/${$stateParams.id}?hierarchy`).then(result => result.data.response)]
            }
        });
    }]);

    app.controller("contract.EditHierarchyCtrl",
        ["$scope", "_", "hierarchyFlat",
            ($scope, _, hierarchyFlat) => {
                $scope.hierarchy = _.toHierarchy(hierarchyFlat, "id", "parentId", "children");
            }
        ]
    );
})(angular, app);

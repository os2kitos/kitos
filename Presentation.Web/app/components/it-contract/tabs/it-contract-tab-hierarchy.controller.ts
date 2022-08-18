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
        ["$scope", "_", "$state", "$timeout", "hierarchyFlat", "$stateParams", "notify", "contract", "hasWriteAccess", "$http", "user", "select2LoadingService",
            ($scope, _, $state, $timeout, hierarchyFlat, $stateParams, notify, contract, hasWriteAccess, $http, user, select2LoadingService: Kitos.Services.ISelect2LoadingService) => {
                $scope.hierarchy = _.toHierarchy(hierarchyFlat, "id", "parentId", "children");
                $scope.autoSaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.contract = contract;
                $scope.hasWriteAccess = hasWriteAccess;

                $scope.itContractsSelectOptions = select2LoadingService.loadSelect2WithDataHandler('api/itcontract',
                    true,
                    ['orgId=' + user.currentOrganizationId],
                    (c, results) => {
                        if (c.id !== contract.id) {
                            results.push({ id: c.id, text: c.name });
                        }
                    },
                    "q");

                if (!!contract.parentId) {
                    $scope.contract.parent = {
                        id: contract.parentId,
                        text: contract.parentName
                    };
                }
                $scope.updateHierarchy = (field, value) => {
                    let id;
                    if (value == null) {
                        id = null;
                    } else {
                        id = value.id;
                    }
                    var payload = {};
                    payload[field] = id;
                    $http.patch($scope.autoSaveUrl + `?organizationId=${user.currentOrganizationId}`, payload)
                        .then(() => {
                            notify.addSuccessMessage("Feltet er opdateret!");
                            reload();
                        },
                            () => notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
                }
                function reload() {
                    return $state.transitionTo($state.current, $stateParams, {
                        reload: true
                    }).then(() => {
                        $scope.hideContent = true;
                        return $timeout(() => $scope.hideContent = false, 1);
                    });
                };
            }
        ]
    );
})(angular, app);

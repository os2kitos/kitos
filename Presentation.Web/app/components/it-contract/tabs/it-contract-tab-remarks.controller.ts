(function (ng, app) {

    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-contract.edit.remarks", {
            url: "/remarks",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-remarks.view.html",
            controller: "contract.RemarksCtrl",
            resolve: {
                getRemark: ["$http", "contract", function ($http, contract) {
                    $http.get(`odata/ItContractRemarks(${contract.id})`).then(result => {
                        return result.data.value;
                    });
                }]
            }
        });
    }
    ]);

    app.controller("contract.RemarksCtrl", ["$scope", "$http", "$stateParams", "notify", "contract", "hasWriteAccess", "user", "getRemark", function ($scope, $http, $stateParams, notify, contract, hasWriteAccess, user, getRemark) {
        $scope.autosaveUrl = `api/itcontract/${contract.id}`;
        $scope.hasWriteAccess = hasWriteAccess;
        $scope.remark = getRemark;
        $scope.userMayEdit = user.isGlobalAdmin || user.isLocalAdmin || user.isContractAdmin || hasWriteAccess;

        $scope.

            $scope.saveRemark = function () {
                $http.post();
            };

    }]);

})(angular, app);
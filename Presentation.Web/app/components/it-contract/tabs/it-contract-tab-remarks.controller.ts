(function (ng, app) {

    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-contract.edit.remarks", {
            url: "/remarks",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-remarks.view.html",
            controller: "contract.RemarksCtrl",
            resolve: {
                getRemark: ["$http", "contract", function ($http, contract) {
                    return $http.get(`odata/ItContractRemarks(${contract.id})`).then(function (result) {
                        return result.data;
                    }, function () {
                        //Contracts can have 0 or 1 remark and if there is no remark a 404 error is returned
                        //If the is no remark we POST otherwise PATCH
                        return null;
                    });
                }]
            }
        });
    }
    ]);

    app.controller("contract.RemarksCtrl", ["$scope", "$http", "$stateParams", "notify", "contract", "hasWriteAccess", "user", "getRemark",
        function ($scope, $http, $stateParams, notify, contract, hasWriteAccess, user, getRemark) {

            $scope.hasWriteAccess = hasWriteAccess;
            $scope.userMayEdit = user.isGlobalAdmin || user.isLocalAdmin || user.isContractAdmin || hasWriteAccess;
            $scope.itContractRemark = getRemark;
            $scope.remarkVisibility = "0";
            $scope.remark = "";

            if ($scope.itContractRemark !== null) {
                if ($scope.userMayEdit) {
                    $scope.remark = $scope.itContractRemark.Remark;
                } else {
                    $scope.remark = "Du har ikke lov til at se denne information.";
                }
            }

            $scope.saveRemark = function () {
                if ($scope.userMayEdit) {
                    if ($scope.itContractRemark === null) {
                        postRemark();
                    } else {
                        patchRemark();
                    }
                }
            };

            function postRemark() {
                console.log("POST");
                var msg = notify.addInfoMessage("Gemmer bemærkning...", false);

                const payload = {
                    "Id": `${contract.id}`,
                    "AccessModifier": `${$scope.remarkVisibility}`,
                    "Remark": $scope.remark
                };

                $http.post("odata/ItContractRemarks", payload).then(function (result) {
                    msg.toSuccessMessage("Bemærkningen er gemt");
                    console.log("POST Successful");
                    $scope.itContractRemark = result.data;
                }, function () {
                    console.log("POST Error");
                    msg.toErrorMessage("Bemærkningen kunne ikke gemmes");
                });
            }

            function patchRemark() {
                console.log("PATCH");
                var msg = notify.addInfoMessage("Opdaterer bemærkning...", false);

                const payload = {
                    "AccessModifier": `${$scope.remarkVisibility}`,
                    "Remark": $scope.remark
                };

                $http.patch(`odata/ItContractRemarks(${contract.id})`, payload).then(function (success) {
                    msg.toSuccessMessage("Bemærkningen er gemt");
                    $scope.remark = success.config.data.Remark;
                    console.log("PATCH Successful");
                }, function () {
                    console.log("PATCH Error");
                    msg.toErrorMessage("Bemærkningen kunne ikke gemmes");
                });
            }

        }]);

})(angular, app);
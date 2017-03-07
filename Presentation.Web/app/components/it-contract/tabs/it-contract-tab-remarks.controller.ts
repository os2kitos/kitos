(function (ng, app) {

    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-contract.edit.remarks", {
            url: "/remarks",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-remarks.view.html",
            controller: "contract.RemarksCtrl",
            resolve: {
                getRemark: ["$http", "contract", "notify", function ($http, contract, notify) {
                    return $http.get(`odata/ItContracts(${contract.id})/Remark`).then(function (result) {
                        return result.data;
                    }, function (error) {
                        //Contracts can have 0 or 1 remark and if there is no remark a 404 error is returned
                        //If the is no remark we POST otherwise PATCH

                        if (error.status === 401) {
                            notify.addInfoMessage("Du har ikke lov til at se disse informationer. Kontakt venligst din lokale administrator eller kontrakt administrator.");
                        }

                        return null;
                    });
                }]
            }
        });
    }
    ]);

    app.controller("contract.RemarksCtrl", ["$scope", "$http", "$stateParams", "notify", "contract", "hasWriteAccess", "user", "getRemark",
        function ($scope, $http, $stateParams, notify, contract, hasWriteAccess, user, getRemark) {

            $scope.itContractRemark = getRemark;

            if ($scope.itContractRemark !== null) {
                $scope.remark = $scope.itContractRemark.Remark;
                $scope.accessModifier = $scope.itContractRemark.AccessModifier;
            }

            $scope.saveRemark = function () {
                if (hasWriteAccess) {
                    if ($scope.itContractRemark === null) {
                        postRemark();
                    } else {
                        patchRemark();
                    }
                }
            };

            function postRemark() {
                var msg = notify.addInfoMessage("Gemmer bemærkning...", false);

                const payload = {
                    "Id": `${contract.id}`,
                    "AccessModifier": `${$scope.accessModifier}`,
                    "Remark": $scope.remark
                };

                $http.post(`odata/ItContractRemarks(${contract.id})`, payload).then(function (result) {
                    msg.toSuccessMessage("Bemærkningen er gemt");
                    $scope.itContractRemark = result.data;
                }, function () {
                    msg.toErrorMessage("Bemærkningen kunne ikke gemmes");
                });
            };

            function patchRemark() {
                var msg = notify.addInfoMessage("Opdaterer bemærkning...", false);

                const payload = {
                    "AccessModifier": `${$scope.accessModifier}`,
                    "Remark": $scope.remark
                };

                $http.patch(`odata/ItContractRemarks(${$scope.itContractRemark.Id})`, payload).then(function (success) {
                    msg.toSuccessMessage("Bemærkningen er gemt");
                    $scope.remark = success.config.data.Remark;
                }, function () {
                    msg.toErrorMessage("Bemærkningen kunne ikke gemmes");
                });
            };

        }]);

})(angular, app);
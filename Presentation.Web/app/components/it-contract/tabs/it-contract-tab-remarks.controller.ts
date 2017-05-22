(function (ng, app) {

    app.config(["$stateProvider", function ($stateProvider) {
            $stateProvider.state("it-contract.edit.remarks", {
                url: "/remarks",
                templateUrl: "app/components/it-contract/tabs/it-contract-tab-remarks.view.html",
                controller: "contract.RemarksCtrl",
                resolve: {
                    getRemark: ["$http", "contract", function ($http, contract) {
                        return $http.get(`odata/ItContracts(${contract.id})/Remark`).then(function (result) {
                            return result.data;
                        }, function (error) {
                            //Contracts can have 0 or 1 remark and if there is no remark a 404 error is returned
                            return error;
                        });
                    }]
                }
            });
        }
    ]);

    app.controller("contract.RemarksCtrl", ["$scope", "$http", "$stateParams", "notify", "contract", "user", "getRemark",
        function ($scope, $http, $stateParams, notify, contract, user, getRemark) {

            $scope.itContractRemark = getRemark;
            // in order to have write access the user and the contract must belong to the same organization and the user must either own the object or have a valid role in the organization
            $scope.hasWriteAccess = user.currentOrganizationId === contract.organizationId && (user.id === contract.objectOwnerId || user.isGlobalAdmin || user.isLocalAdmin || user.isContractAdmin);
            $scope.visibility = "Local";
            $scope.remark = "";

            if ($scope.itContractRemark.status === 401) {
                notify.addInfoMessage("Du har ikke lov til at se disse informationer. Kontakt venligst din lokale administrator eller kontrakt administrator.");
            } else if ($scope.itContractRemark !== null) {
                $scope.remark = $scope.itContractRemark.Remark;
                $scope.visibility = $scope.itContractRemark.AccessModifier;
            }

            $scope.saveRemark = function () {
                if ($scope.hasWriteAccess) {
                    //If the contract has no remark (404) we POST otherwise PATCH
                    if ($scope.itContractRemark.status === 404) {
                        postRemark();
                    } else {
                        patchRemark();
                    }
                }
            };

            $scope.changeVisibility = function () {
                if ($scope.hasWriteAccess) {
                    changeVisibility();
                } else {
                    notify.addInfoMessage("Du har ikke lov til at foretage denne handling.");
                }
            }

            function changeVisibility() {
                const payload = {
                    "AccessModifier": `${$scope.visibility}`
                };

                $http.patch(`odata/ItContractRemarks(${$scope.itContractRemark.Id})`, payload).then(function (success) {
                    notify.addSuccessMessage("Synligheden for bemærkninger blev opdateret");
                }, function (error) {
                    if (error.status === 403) {
                        notify.addInfoMessage("Du har ikke lov til at foretage denne handling.");
                    } else {
                        notify.addErrorMessage("Synligheden for bemærkninger blev ikke opdateret");
                    }
                });
            }

            function postRemark() {
                const payload = {
                    "Id": `${contract.id}`,
                    "Remark": $scope.remark
                };

                $http.post(`odata/ItContractRemarks(${contract.id})`, payload).then(function (result) {
                    notify.addSuccessMessage("Bemærkningen er gemt");
                    $scope.itContractRemark = result.data;
                }, function (error) {
                    if (error.status === 401) {
                        notify.addInfoMessage("Du har ikke lov til at foretage denne handling.");
                    } else {
                        notify.addErrorMessage("Bemærkningen kunne ikke gemmes");
                    }
                });
            };

            function patchRemark() {
                const payload = {
                    "Remark": $scope.remark
                };

                $http.patch(`odata/ItContractRemarks(${$scope.itContractRemark.Id})`, payload).then(function (success) {
                    notify.addSuccessMessage("Bemærkningen er gemt");
                    $scope.remark = success.config.data.Remark;
                }, function (error) {
                    if (error.status === 403) {
                        notify.addInfoMessage("Du har ikke lov til at foretage denne handling.");
                        $scope.remark = $scope.itContractRemark.Remark;
                    } else {
                        notify.addErrorMessage("Bemærkningen kunne ikke gemmes");
                    }
                });
            };

        }]);

})(angular, app);
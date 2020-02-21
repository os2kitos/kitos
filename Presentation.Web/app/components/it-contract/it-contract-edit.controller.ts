((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("it-contract.edit", {
                url: "/edit/{id:[0-9]+}",
                templateUrl: "app/components/it-contract/it-contract-edit.view.html",
                controller: "contract.EditCtrl",
                controllerAs: "contractEditVm",
                resolve: {
                    contract: ["$http", "$stateParams", ($http, $stateParams) => $http.get("api/itcontract/" + $stateParams.id).then(result => result.data.response)
                    ],
                    user: ["userService", userService => userService.getUser().then(user => user)
                    ],
                    userAccessRights: ["$http", "$stateParams",
                        ($http, $stateParams) => $http
                        .get("api/itcontract?id=" + $stateParams.id + "&getEntityAccessRights=true")
                        .then(result => result.data.response)
                    ],
                    hasWriteAccess: ["userAccessRights", (userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit]
                }
            });
        }
    ]);

    app.controller("contract.EditCtrl", ["$scope", "$rootScope", "contract", "hasWriteAccess","userAccessRights",
        ($scope, $rootScope, contract, hasWriteAccess, userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => {
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.allowClearOption = {
                allowClear: true
            };
            $scope.contract = contract;

            if (!userAccessRights.canDelete) {
                _.remove($rootScope.page.subnav.buttons, (o: any) => o.text === "Slet IT Kontrakt");
            }
        }]);
})(angular, app);

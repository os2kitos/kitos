(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-contract.edit', {
                url: '/edit/{id:[0-9]+}',
                templateUrl: 'app/components/it-contract/it-contract-edit.view.html',
                controller: 'contract.EditCtrl',
                controllerAs: "contractEditVm",
                resolve: {
                    contract: [
                        '$http', '$stateParams', function ($http, $stateParams) {
                            return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    user: [
                        'userService', function (userService) {
                            return userService.getUser().then(function (user) {
                                return user;
                            });
                        }
                    ],
                    hasWriteAccess: [
                        '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                            return $http.get("api/itcontract/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                                .then(function (result) {
                                    return result.data.response;
                                });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('contract.EditCtrl',
    [
        '$scope', '$rootScope', '$http', '$stateParams', 'notify', 'contract', 'user', 'hasWriteAccess',
        function ($scope, $rootScope, $http, $stateParams, notify, contract, user, hasWriteAccess) {
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.allowClearOption = {
                allowClear: true
            };

            if (!$scope.hasWriteAccess) {
                _.remove($rootScope.page.subnav.buttons, function (o: any) {
                    return o.text === "Slet IT Kontrakt";
                });
            }
        }]);
})(angular, app);

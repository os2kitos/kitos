(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-contract.edit', {
                url: '/edit/{id:[0-9]+}',
                templateUrl: 'app/components/it-contract/it-contract-edit.view.html',
                controller: 'contract.EditCtrl',
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
        '$scope', '$http', '$stateParams', 'notify', 'contract', 'user', 'hasWriteAccess',
        function ($scope, $http, $stateParams, notify, contract, user, hasWriteAccess) {
            $scope.hasWriteAccess = hasWriteAccess;

            function formatContractSigner(signer) {

                var userForSelect = null;

                $scope.contractSigner = {
                    edit: false,
                    signer: signer,
                    userForSelect: userForSelect,
                    update: function () {
                        var selectedUser = $scope.contractSigner.userForSelect;

                        if (selectedUser) {
                            var msg = notify.addInfoMessage("Gemmer...", false);

                            var signerId = selectedUser ? selectedUser.id : null;
                            var signerUser = selectedUser ? selectedUser.user : null;

                            $http({
                                method: 'PATCH',
                                url: 'api/itcontract/' + contract.id + '?organizationId=' + user.currentOrganizationId,
                                data: {
                                    contractSignerId: signerId
                                }
                            }).success(function (result) {
                                msg.toSuccessMessage("Kontraktunderskriveren er gemt");

                                formatContractSigner({ id: signerUser.Id, fullName: signerUser.Name + " " + signerUser.LastName });

                            }).error(function () {
                                msg.toErrorMessage("Fejl!");
                            });
                        }
                    }
                };
            }

            formatContractSigner(contract.contractSigner);

        }]);
})(angular, app);

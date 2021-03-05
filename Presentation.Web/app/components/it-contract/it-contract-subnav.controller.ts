(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract', {
            url: '/contract',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$http', '$state', '$uibModal', 'notify', 'user', '$scope', function ($rootScope, $http, $state, $modal, notify, user, $scope) {
                $rootScope.page.title = 'IT Kontrakt';
                $rootScope.page.subnav = [
                    { state: 'it-contract.overview', text: "Kontraktoverblik - Økonomi" },
                    { state: 'it-contract.plan', text: "Kontraktoverblik - Tid" }
                ];
                $rootScope.page.subnav.buttons = [
                    { func: remove, text: 'Slet IT Kontrakt', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-contract.edit' }
                ];
                $rootScope.subnavPositionCenter = false;

                function remove() {
                    if (!confirm("Er du sikker på du vil slette kontrakten?")) {
                        return;
                    }
                    var contractId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter IT Kontrakten...", false);
                    $http.delete('api/itcontract/' + contractId + '?organizationId=' + user.currentOrganizationId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("IT Kontrakten er slettet!");
                            $state.go('it-contract.overview');
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke slette IT Kontrakten!");
                        });
                }

                $scope.$on('$viewContentLoaded', function () {
                    $rootScope.positionSubnav();
                });
            }]
        });
    }]);
})(angular, app);

(function(ng, app) {
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
            controller: ['$rootScope', '$http', '$state', '$uibModal', 'notify', 'user', function ($rootScope, $http, $state, $modal, notify, user) {
                $rootScope.page.title = 'IT Kontrakt';
                $rootScope.page.subnav = [
                    { state: 'it-contract.overview', text: "IT kontrakter: økonomi" },
                    { state: 'it-contract.plan', text: "IT kontrakter: tid" },
                    { state: 'it-contract.edit', text: 'IT Kontrakt', showWhen: 'it-contract.edit' },
                ];
                $rootScope.page.subnav.buttons = [
                    { func: remove, text: 'Slet IT Kontrakt', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-contract.edit' }
                ];

                function remove() {
                    if (!confirm("Er du sikker på du vil slette kontrakten?")) {
                        return;
                    }
                    var contractId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter IT Kontrakten...", false);
                    $http.delete('api/itcontract/' + contractId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage("IT Kontrakten er slettet!");
                            $state.go('it-contract.overview');
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke slette IT Kontrakten!");
                        });
                }
            }]
        });
    }]);
})(angular, app);

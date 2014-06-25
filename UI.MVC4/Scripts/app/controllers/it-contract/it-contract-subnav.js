(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract', {
            url: '/contract',
            abstract: true,
            template: '<ui-view/>',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$http', '$state', 'notify', 'user', function ($rootScope, $http, $state, notify, user) {
                $rootScope.page.title = 'IT Kontrakt';
                $rootScope.page.subnav = [
                    { state: 'it-contract.overview', text: 'Overblik: økonomi' },
                    { state: 'it-contract.plan', text: 'Overblik: tid' },
                    { state: 'it-contract.edit', text: 'IT Kontrakt', showWhen: 'it-contract.edit' },
                ];
                $rootScope.page.subnav.buttons = [
                    { func: create, text: 'Opret IT Kontrakt', style: 'btn-success', icon: 'glyphicon-plus' },
                    { func: remove, text: 'Slet IT Kontrakt', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-contract.edit' }
                ];

                function create() {
                    var orgId = user.currentOrganizationId;
                    var msg = notify.addInfoMessage("Opretter kontrakt...", false);
                    $http.post('api/itcontract', { organizationId: orgId })
                        .success(function(result) {
                            msg.toSuccessMessage("En ny kontrakt er oprettet!");
                            var contract = result.response;
                            $state.go('it-contract.edit.systems', { id: contract.id });
                        })
                        .error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke oprette en ny kontrakt!");
                        });
                }

                function remove() {
                    var contractId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter IT Kontrakten...", false);
                    $http.delete('api/itcontract/' + contractId)
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
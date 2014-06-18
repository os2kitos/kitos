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
                $rootScope.page.buttons = [
                    { func: create, text: 'Opret IT Kontrakt' }
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
            }]
        });
    }]);
})(angular, app);
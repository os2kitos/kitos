(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system', {
            url: '/system',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            },
            controller: ['$rootScope', '$http', '$state', 'notify', 'user', function ($rootScope, $http, $state, notify, user) {
                $rootScope.page.title = 'IT System';
                $rootScope.page.subnav = [
                    { state: 'it-system.overview', text: 'Overblik' },
                    { state: 'it-system.catalog', text: 'IT System katalog' },
                    { state: 'it-system.edit', text: 'IT System', showWhen: 'it-system.edit' },
                    { state: 'it-system.usage', text: 'IT System anvendelse', showWhen: 'it-system.usage' }
                ];
                $rootScope.page.subnav.buttons = [
                    { func: create, text: 'Opret IT System', style: 'btn-success', icon: 'glyphicon-plus' },
                    { func: removeUsage, text: 'Fjern anvendelse', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.usage' },
                    { func: remove, text: 'Slet IT System', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.edit' }
                ];

                function create() {
                    var payload = {
                        name: 'Unavngivet system',
                        belongsToId: user.currentOrganizationId,
                        organizationId: user.currentOrganizationId,
                        userId: user.id,
                        dataRows: [],
                        taskRefIds: [],
                        canUseInterfaceIds: []
                    };

                    var msg = notify.addInfoMessage("Opretter system...", false);
                    $http.post('api/itsystem', payload)
                        .success(function(result) {
                            msg.toSuccessMessage("Et nyt system er oprettet!");
                            var systemId = result.response.id;
                            $state.go('it-system.edit.interfaces', { id: systemId });
                        }).error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke oprette et nyt system!");
                        });
                };

                function removeUsage() {
                    var usageId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter IT System anvendelsen...", false);
                    $http.delete('api/itsystemusage/' + usageId)
                        .success(function (result) {
                            msg.toSuccessMessage("IT System anvendelsen er slettet!");
                            $state.go('it-system.overview');
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke slette IT System anvendelsen!");
                        });
                }

                function remove() {
                    var systemId = $state.params.id;
                    var msg = notify.addInfoMessage("Sletter IT System...", false);
                    $http.delete('api/itsystem/' + systemId)
                        .success(function (result) {
                            msg.toSuccessMessage("IT System  er slettet!");
                            $state.go('it-system.catalog');
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke slette IT System!");
                        });
                }
            }]
        });
    }]);
})(angular, app);
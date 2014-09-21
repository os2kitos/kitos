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
                    { state: 'it-system.interfaceCatalog', text: 'Snitflade katalog' },
                    { state: 'it-system.edit', text: 'IT System', showWhen: 'it-system.edit' },
                    { state: 'it-system.usage', text: 'IT System anvendelse', showWhen: 'it-system.usage' },
                    { state: 'it-system.interface-edit', text: 'Snitflade', showWhen: 'it-system.interface-edit' }
                ];
                $rootScope.page.subnav.buttons = [
                    { func: createSystem, text: 'Opret IT System', style: 'btn-success', icon: 'glyphicon-plus' },
                    { func: createInterface, text: 'Opret Snitflade', style: 'btn-success', icon: 'glyphicon-plus' },
                    { func: removeUsage, text: 'Fjern anvendelse', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.usage' },
                    { func: removeSystem, text: 'Slet IT System', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.edit' },
                    { func: removeInterface, text: 'Slet Snitflade', style: 'btn-danger', icon: 'glyphicon-minus', showWhen: 'it-system.interface-edit' }
                ];

                function createSystem() {
                    var payload = {
                        name: 'Unavngivet system',
                        belongsToId: user.currentOrganizationId,
                        organizationId: user.currentOrganizationId,
                        userId: user.id,
                        taskRefIds: [],
                    };

                    var msg = notify.addInfoMessage('Opretter system...', false);
                    $http.post('api/itsystem', payload)
                        .success(function(result) {
                            msg.toSuccessMessage('Et nyt system er oprettet!');
                            var systemId = result.response.id;
                            $state.go('it-system.edit.interfaces', { id: systemId });
                        }).error(function() {
                            msg.toErrorMessage('Fejl! Kunne ikke oprette et nyt system!');
                        });
                };

                function removeSystem() {
                    if (!confirm('Er du sikker på du vil slette systemet?')) {
                        return;
                    }
                    var systemId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter IT System...', false);
                    $http.delete('api/itsystem/' + systemId)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System  er slettet!');
                            $state.go('it-system.catalog');
                        })
                        .error(function () {
                            msg.toErrorMessage('Fejl! Kunne ikke slette IT System!');
                        });
                }

                function removeUsage() {
                    if (!confirm('Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.')) {
                        return;
                    }
                    var usageId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter IT System anvendelsen...', false);
                    $http.delete('api/itsystemusage/' + usageId)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System anvendelsen er slettet!');
                            $state.go('it-system.overview');
                        })
                        .error(function () {
                            msg.toErrorMessage('Fejl! Kunne ikke slette IT System anvendelsen!');
                        });
                }

                function createInterface() {
                    var payload = {
                        name: 'Unavngivet snitflade',
                        organizationId: user.currentOrganizationId,
                    };

                    var msg = notify.addInfoMessage('Opretter snitflade...', false);
                    $http.post('api/itinterface', payload)
                        .success(function (result) {
                            msg.toSuccessMessage('En ny snitfalde er oprettet!');
                            var interfaceId = result.response.id;
                            $state.go('it-system.interface-edit.interface-details', { id: interfaceId });
                        }).error(function () {
                            msg.toErrorMessage('Fejl! Kunne ikke oprette snitfalde!');
                        });
                }

                function removeInterface() {
                    if (!confirm('Er du sikker på du vil slette snitfladen?')) {
                        return;
                    }
                    var interfaceId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter Snitflade...', false);
                    $http.delete('api/itinterface/' + interfaceId)
                        .success(function (result) {
                            msg.toSuccessMessage('Snitflade er slettet!');
                            $state.go('it-system.interfaceCatalog');
                        })
                        .error(function () {
                            msg.toErrorMessage('Fejl! Kunne ikke slette Snitfladen!');
                        });
                }
            }]
        });
    }]);
})(angular, app);

(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system', {
            url: '/system',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],

            },
            controller: ['$rootScope', '$http', '$state', '$uibModal', 'notify', 'user', '$scope', '$timeout', function ($rootScope, $http, $state, $modal, notify, user, $scope, $timeout) {
                $rootScope.page.title = 'IT System';
                $rootScope.page.subnav = [
                    { state: 'it-system.overview', text: "IT Systemer" },
                    { state: 'it-system.catalog', text: 'IT System katalog' },
                    { state: 'it-system.interfaceCatalog', text: 'Snitflade katalog' },
                    { state: 'it-system.edit', text: 'IT System', showWhen: 'it-system.edit' },
                    { state: 'it-system.usage', text: 'IT System anvendelse', showWhen: 'it-system.usage' },
                    { state: 'it-system.interface-edit', text: 'Snitflade', showWhen: 'it-system.interface-edit' }
                ];
                
                $rootScope.page.subnav.buttons = [
                    { func: removeUsage, text: 'Fjern anvendelse', style: 'btn-danger', showWhen: 'it-system.usage' },
                    { func: removeInterface, text: 'Slet Snitflade', style: 'btn-danger', showWhen: 'it-system.interface-edit' }
                ];

                $rootScope.subnavPositionCenter = false;

                function removeUsage() {
                    if (!confirm('Er du sikker på at du vil fjerne den lokale anvendelse af systemet? Dette sletter ikke systemet, men vil slette alle lokale detaljer vedrørende anvendelsen.')) {
                        return;
                    }
                    var usageId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter IT System anvendelsen...', false);
                    $http.delete('api/itSystemUsage/' + usageId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System anvendelsen er slettet!');
                            $state.go('it-system.overview');
                        })
                        .error(function (error, status) {
                            if (status === 401)
                                msg.toErrorMessage('Fejl! Du har ikke tilladelse!');
                            else
                                msg.toErrorMessage("Fejl! Kunne ikke slette IT System anvendelsen!");
                        });
                }

                function removeInterface() {
                    if (!confirm('Er du sikker på du vil slette snitfladen?')) {
                        return;
                    }
                    var interfaceId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter Snitflade...', false);
                    $http.delete('api/itinterface/' + interfaceId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage('Snitflade er slettet!');
                            $state.go('it-system.interfaceCatalog');
                        })
                        .error(function (data, status) {
                            if (status == 409)
                                msg.toErrorMessage('Fejl! Kan ikke slette snitflade, den er tilknyttet et IT System, som er i lokal anvendelse!');
                            else if (status === 401)
                                msg.toErrorMessage('Fejl! Du har ikke tilladelse!');
                            else
                                msg.toErrorMessage('Fejl! Kunne ikke slette Snitfladen!');
                        });
                }

                $scope.$on('$viewContentLoaded', function () {
                    $rootScope.positionSubnav();
                });
            }]
        });
    }]);
})(angular, app);

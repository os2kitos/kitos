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
                    { state: 'it-system.overview', substate: 'it-system.usage', text: "IT Systemer i " + user.currentOrganizationName },
                    { state: 'it-system.catalog', substate: 'it-system.edit', text: 'IT Systemkatalog' },
                    { state: 'it-system.interfaceCatalog', substate: 'it-system.interface-edit.main', text: 'Snitfladekatalog' }
                ];

                $rootScope.page.subnav.buttons = [
                    {
                        func: removeUsage, text: 'Fjern anvendelse', style: 'btn-danger', showWhen: 'it-system.usage', dataElementType: 'removeSystemUsageButton'
                    },
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
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage('IT System anvendelsen er slettet!');
                            $state.go('it-system.overview');
                        }, function onError(result) {
                            if (result.status === 403)
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
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage('Snitflade er slettet!');
                            $state.go('it-system.interfaceCatalog');
                        }, function onError(result) {
                            if (result.status == 409)
                                msg.toErrorMessage('Fejl! Kan ikke slette snitflade, den er tilknyttet et IT System, som er i lokal anvendelse!');
                            else if (result.status === 403)
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

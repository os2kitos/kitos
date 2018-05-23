(function (ng, app) {
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $stateProvider.state('it-system.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'app/components/it-system/edit/it-system-edit.view.html',
            controller: 'system.EditCtrl',
            resolve: {
                itSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itsystem/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ],
                hasWriteAccess: ['$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                    return $http.get("api/itsystem/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditCtrl',
        [
            '$rootScope', '$scope', 'itSystem', 'user', 'hasWriteAccess', '$state', 'notify', '$http', '_',
            function ($rootScope, $scope, itSystem, user, hasWriteAccess, $state, notify, $http, _) {
                
                $scope.hasWriteAccess = hasWriteAccess;

                if (user.isGlobalAdmin) {
                    if (!$rootScope.page.subnav.buttons.some(x => x.text === "Slet IT System")) {
                        $rootScope.page.subnav.buttons.push({ func: removeSystem, text: 'Slet IT System', style: 'btn-danger', showWhen: 'it-system.edit' });
                    }
                } else if ((hasWriteAccess && (user.currentOrganizationId === itSystem.organizationId) && itSystem.accessModifier === 0) || ($scope.hasWriteAccess && (user.id === itSystem.objectOwnerId) && itSystem.accessModifier === 0)) {
                    if (!$rootScope.page.subnav.buttons.some(x => x.text === "Slet IT System")) {
                    $rootScope.page.subnav.buttons.push({ func: removeSystem, text: 'Slet IT System', style: 'btn-danger', showWhen: 'it-system.edit' });
                    }
                } else {
                    _.remove($rootScope.page.subnav.buttons, function (o) {
                        return o.text === "Slet IT System";
                    });
                }
                if (itSystem.accessModifier === 1 || user.isGlobalAdmin || user.isLocalAdmin) {
                    _.remove($rootScope.page.subnav.buttons, function (o) {
                        return o.text === "Deaktivér IT System";
                    });
                    _.remove($rootScope.page.subnav.buttons, function (o) {
                        return o.text === "Aktivér IT System";
                    });

                    if (itSystem.accessModifier === 1 || user.isGlobalAdmin || user.isLocalAdmin) {
                        if (!itSystem.disabled) {
                            $rootScope.page.subnav.buttons.push(
                                { func: disableSystem, text: 'Deaktivér IT System', style: 'btn-danger', showWhen: 'it-system.edit' }
                            );
                        } else {
                            $rootScope.page.subnav.buttons.push(
                                { func: enableSystem, text: 'Aktivér IT System', style: 'btn-success', showWhen: 'it-system.edit' }
                            );
                        }
                    }
                }
                function disableSystem() {
                    if (!confirm('Er du sikker på du vil deaktivere systemet?')) {
                        return;
                    }

                    var payload: any = {};
                    payload.Disabled = true;

                    var msg = notify.addInfoMessage('Deaktiverer IT System...', false);
                    $http.patch('odata/ItSystems(' + itSystem.id + ')', payload)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System er deaktiveret!');
                            $state.reload();
                        })
                        .error(function (data, status) {
                            msg.toErrorMessage('Fejl! Kunne ikke deaktivere IT System!');
                        });
                }

                function enableSystem() {
                    if (!confirm('Er du sikker på du vil aktivere systemet?')) {
                        return;
                    }
                    var payload: any = {};
                    payload.Disabled = false;

                    var msg = notify.addInfoMessage('Aktiverer IT System...', false);
                    $http.patch('odata/ItSystems(' + itSystem.id + ')', payload)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System er aktiveret!');
                            $state.reload();
                        })
                        .error(function (data, status) {
                            msg.toErrorMessage('Fejl! Kunne ikke aktivere IT System!');
                        });
                }

                function removeSystem() {
                    if (!confirm('Er du sikker på du vil slette systemet?')) {
                        return;
                    }
                    var systemId = $state.params.id;
                    var msg = notify.addInfoMessage('Sletter IT System...', false);
                    $http.delete('api/itsystem/' + systemId + '?organizationId=' + user.currentOrganizationId)
                        .success(function (result) {
                            msg.toSuccessMessage('IT System  er slettet!');
                            $state.go('it-system.catalog');
                        })
                        .error(function (data, status) {
                            if (status === 409)
                                msg.toErrorMessage('Fejl! IT Systemet er i lokal anvendelse!');
                            else if (status === 401)
                                msg.toErrorMessage('Fejl! Du har ikke tilladelse!');
                            else
                                msg.toErrorMessage('Fejl! Kunne ikke slette IT System!');
                        });
                }
            }
        ]);
})(angular, app);

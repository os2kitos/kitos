((ng, app) => {
    app.config(['$stateProvider', '$urlRouterProvider', ($stateProvider, $urlRouterProvider) => {
        $stateProvider.state('it-system.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'app/components/it-system/edit/it-system-edit.view.html',
            controller: 'system.EditCtrl',
            resolve: {
                itSystem: ['$http', '$stateParams', ($http, $stateParams) => $http.get("api/itsystem/" + $stateParams.id)
                    .then(result => result.data.response)],
                user: [
                    'userService', userService => userService.getUser()
                ],
                userAccessRights: ["authorizationServiceFactory", "$stateParams",
                    (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                        authorizationServiceFactory
                            .createSystemAuthorization()
                            .getAuthorizationForItem($stateParams.id)
                ],
                hasWriteAccess: ["userAccessRights", userAccessRights => userAccessRights.canEdit],
            }
        });
    }]);

    app.controller("system.EditCtrl",
        [
            "$rootScope", "$scope", "itSystem", "user", "hasWriteAccess", "$state", "notify", "$http", "_", "userAccessRights", "SystemDeletedErrorResponseTranslationService",
            ($rootScope, $scope, itSystem, user, hasWriteAccess, $state, notify, $http, _, userAccessRights, systemDeletedErrorResponseTranslationService) => {

                const systemStateButtonTexts = {
                    activate: "Gør IT System tilgængeligt",
                    deactivate: "Gør IT System 'Ikke tilgængeligt'"
                }
                $scope.showKLE = user.isGlobalAdmin;
                $scope.showReference = user.isGlobalAdmin;

                $scope.systemNameHeader = Kitos.Helpers.SystemNameFormat.apply(itSystem.name + " - data i IT systemkatalog", itSystem.disabled);

                $scope.hasWriteAccess = hasWriteAccess;


                if (userAccessRights.canDelete) {
                    if (!$rootScope.page.subnav.buttons.some(x => x.text === "Slet IT System")) {
                        $rootScope.page.subnav.buttons.push({ func: removeSystem, text: "Slet IT System", style: "btn-danger", showWhen: "it-system.edit" });
                    }
                }
                else {
                    _.remove($rootScope.page.subnav.buttons, o => o.text === "Slet IT System");
                }
                if (userAccessRights.canEdit) {
                    _.remove($rootScope.page.subnav.buttons, o => o.text === systemStateButtonTexts.deactivate);
                    _.remove($rootScope.page.subnav.buttons, o => o.text === systemStateButtonTexts.activate);

                    if (!itSystem.disabled) {
                        $rootScope.page.subnav.buttons.push(
                            { func: disableSystem, text: systemStateButtonTexts.deactivate, style: "btn-danger", showWhen: "it-system.edit" }
                        );
                    } else {
                        $rootScope.page.subnav.buttons.push(
                            { func: enableSystem, text: systemStateButtonTexts.activate, style: "btn-success", showWhen: "it-system.edit" }
                        );
                    }
                }
                function disableSystem() {
                    if (!confirm("Er du sikker på, at du vil gøre IT Systemet 'ikke tilgængeligt'?")) {
                        return;
                    }

                    var payload: any = {};
                    payload.Disabled = true;

                    var msg = notify.addInfoMessage("Gør IT System 'ikke tilgængeligt'...", false);
                    $http.patch("odata/ItSystems(" + itSystem.id + ")", payload)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("IT Systemet er nu 'ikke tilgængeligt'!");
                            $state.reload();
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gøre IT Systemet 'ikke tilgængeligt'!");
                        });
                }

                function enableSystem() {
                    if (!confirm("Er du sikker på, at du vil gøre IT Systemet tilgængeligt?")) {
                        return;
                    }
                    var payload: any = {};
                    payload.Disabled = false;

                    var msg = notify.addInfoMessage("Gør IT Systemet tilgængeligt...", false);
                    $http.patch("odata/ItSystems(" + itSystem.id + ")", payload)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("IT Systemet er tilgængeligt!");
                            $state.reload();
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gøre IT Systemet tilgængeligt!");
                        });
                }

                function removeSystem() {
                    if (!confirm("Er du sikker på du vil slette systemet?")) {
                        return;
                    }
                    var systemId = $state.params.id;

                    var msg = notify.addInfoMessage("Sletter IT System...", false);
                    $http.delete("api/itsystem/" + systemId + "?organizationId=" + user.currentOrganizationId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("IT System  er slettet!");
                            $state.go(Kitos.Constants.ApplicationStateId.SystemCatalog);
                        }, function onError(result) {
                            msg.toErrorMessage(systemDeletedErrorResponseTranslationService.translateResponse(result.status, result.data.response));
                        });
                }
            }
        ]);
})(angular, app);

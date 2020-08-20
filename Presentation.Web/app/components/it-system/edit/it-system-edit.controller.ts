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
            "$rootScope", "$scope", "itSystem", "user", "hasWriteAccess", "$state", "notify", "$http", "_", "userAccessRights","SystemDeletedErrorResponseTranslationService",
            ($rootScope, $scope, itSystem, user, hasWriteAccess, $state, notify, $http, _, userAccessRights, systemDeletedErrorResponseTranslationService) => {

                $scope.showKLE = user.isGlobalAdmin;
                $scope.showReference = user.isGlobalAdmin;

                $scope.systemNameHeader = (itSystem.name + " - data i IT systemkatalog") + (itSystem.disabled ? " (Ikke aktiv)" : "");
				
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
                    _.remove($rootScope.page.subnav.buttons, o => o.text === "Deaktivér IT System");
                    _.remove($rootScope.page.subnav.buttons, o => o.text === "Aktivér IT System");

                    if (!itSystem.disabled) {
                        $rootScope.page.subnav.buttons.push(
                            { func: disableSystem, text: "Deaktivér IT System", style: "btn-danger", showWhen: "it-system.edit" }
                        );
                    } else {
                        $rootScope.page.subnav.buttons.push(
                            { func: enableSystem, text: "Aktivér IT System", style: "btn-success", showWhen: "it-system.edit" }
                        );
                    }
                }
                function disableSystem() {
                    if (!confirm("Er du sikker på du vil deaktivere systemet?")) {
                        return;
                    }

                    var payload: any = {};
                    payload.Disabled = true;

                    var msg = notify.addInfoMessage("Deaktiverer IT System...", false);
                    $http.patch("odata/ItSystems(" + itSystem.id + ")", payload)
                        .success(result => {
                            msg.toSuccessMessage("IT System er deaktiveret!");
                            $state.reload();
                        })
                        .error((data, status) => {
                            msg.toErrorMessage("Fejl! Kunne ikke deaktivere IT System!");
                        });
                }

                function enableSystem() {
                    if (!confirm("Er du sikker på du vil aktivere systemet?")) {
                        return;
                    }
                    var payload: any = {};
                    payload.Disabled = false;

                    var msg = notify.addInfoMessage("Aktiverer IT System...", false);
                    $http.patch("odata/ItSystems(" + itSystem.id + ")", payload)
                        .success(result => {
                            msg.toSuccessMessage("IT System er aktiveret!");
                            $state.reload();
                        })
                        .error((data, status) => {
                            msg.toErrorMessage("Fejl! Kunne ikke aktivere IT System!");
                        });
                }

                function removeSystem() {
                    if (!confirm("Er du sikker på du vil slette systemet?")) {
                        return;
                    }
                    var systemId = $state.params.id;

                    var msg = notify.addInfoMessage("Sletter IT System...", false);
                    $http.delete("api/itsystem/" + systemId + "?organizationId=" + user.currentOrganizationId)
                        .success(result => {
                            msg.toSuccessMessage("IT System  er slettet!");
                            $state.go("it-system.catalog");
                        })
                        .error((data, status) => {
                            msg.toErrorMessage(systemDeletedErrorResponseTranslationService.translateResponse(status , data.response));
                        });
                }
            }
        ]);
})(angular, app);

((ng, app) => {
    const activateInterfaceBtnText = "Aktivér snitflade";
    const deactivateInterfaceBtnText = "Deaktivér snitflade";
    const deleteInterfaceBtnText = "Slet Snitflade";

    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.interface-edit", {
            url: "/edit/{id:[0-9]+}/interface",
            templateUrl: "app/components/it-system/it-interface/it-interface-edit.view.html",
            controller: "system.interfaceEditCtrl",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ],
                userAccessRights: ["authorizationServiceFactory", "$stateParams",
                    (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                        authorizationServiceFactory
                            .createInterfaceAuthorization()
                            .getAuthorizationForItem($stateParams.id)
                ],
                hasWriteAccess: [
                    "userAccessRights", (userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                ],
                itInterface: [
                    "$http", "$stateParams", ($http, $stateParams) => {
                        var interfaceId = $stateParams.id;
                        return $http.get("api/itInterface/" + interfaceId)
                            .then(result => result.data.response);
                    }
                ]
            }
        });
    }]);

    app.controller('system.interfaceEditCtrl',
        [
            "$rootScope", "$scope", "$http", "$state", "notify", "itInterface", "hasWriteAccess", "autofocus", "$stateParams", "_", "userAccessRights",
            ($rootScope, $scope, $http, $state, notify, itInterface, hasWriteAccess, autofocus, $stateParams, _, userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => {
                $rootScope.page.title = "Snitflade - Rediger";
                autofocus();
                $scope.stateId = $stateParams.id;
                itInterface.belongsTo = (!itInterface.belongsToId) ? null : { id: itInterface.belongsToId, text: itInterface.belongsToName };
                itInterface.updateUrl = "api/itInterface/" + itInterface.id;
                $scope.interface = itInterface;
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.select2AllowClearOpt = {
                    allowClear: true
                };

                const removeButton = (text: string) => _.remove($rootScope.page.subnav.buttons, (o: any) => o.text === text);

                removeButton(deactivateInterfaceBtnText);
                removeButton(activateInterfaceBtnText);

                if (!userAccessRights.canDelete) {
                    removeButton(deleteInterfaceBtnText);
                }

                //Enabled/disabled is only interactive for shared interfaces
                if (userAccessRights.canEdit) {
                    const isShared = itInterface.accessModifier === 1;
                    if (isShared) {
                        if (!itInterface.disabled) {
                            $rootScope.page.subnav.buttons.push(
                                { func: disableInterface, text: deactivateInterfaceBtnText, style: "btn-danger", showWhen: "it-system.interface-edit" }
                            );
                        } else {
                            $rootScope.page.subnav.buttons.push(
                                { func: enableInterface, text: activateInterfaceBtnText, style: "btn-success", showWhen: "it-system.interface-edit" }
                            );
                        }
                    }
                }

                function disableInterface() {
                    if (!confirm("Er du sikker på du vil deaktivere snitfladen?")) {
                        return;
                    }

                    var payload: any = {};
                    payload.Disabled = true;

                    var msg = notify.addInfoMessage("Deaktiverer snitflade...", false);
                    $http.patch("odata/ItInterfaces(" + itInterface.id + ")", payload)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Snitflade er deaktiveret!");
                            $state.reload();
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke deaktivere snitflade!");
                        });
                }

                function enableInterface() {
                    if (!confirm("Er du sikker på du vil aktivere snitflade?")) {
                        return;
                    }
                    var payload: any = {};
                    payload.Disabled = false;

                    var msg = notify.addInfoMessage("Aktiverer snitflade...", false);
                    $http.patch("odata/ItInterfaces(" + itInterface.id + ")", payload)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Snitflade er aktiveret!");
                            $state.reload();
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke aktivere snitflade!");
                        });
                }
            }
        ]);
})(angular, app);

((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.interface-edit.main", {
            url: "/main",
            templateUrl: "app/components/it-system/it-interface/it-interface-edit-tab-main.view.html",
            controller: "system.SystemInterfaceMainCtrl",
            resolve: {
                interfaces: [
                    "optionsService", optionsService =>
                        optionsService.getLocalInterfaceTypes()
                ],
                dataTypes: [
                    "localOptionServiceFactory", (localOptionServiceFactory : Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.DataTypes).getAll()
                ],
                dataRows: [
                    "$http", "itInterface",
                    ($http, itInterface) =>
                        $http.get(`api/datarow/?interfaceid=${itInterface.id}`).then(result => result.data.response)
                ]
            }
        });
    }]);

    app.controller("system.SystemInterfaceMainCtrl",
        [
            "$scope", "$http", "$state", "notify", "itInterface", "user", "hasWriteAccess", "interfaces", "dataTypes", "dataRows", "select2LoadingService", "_",
            ($scope, $http, $state, notify, itInterface, user, hasWriteAccess, interfaces, dataTypes, dataRows, select2LoadingService, _) => {

                $scope.hasWriteAccess = hasWriteAccess;
                $scope.interfaces = interfaces;
                $scope.dataTypes = dataTypes;

                $scope.linkButtonDisabled = !Kitos.Utility.Validation.validateUrl(itInterface.url);

                const isDisabled = (itInterface.exhibitedByItSystemDisabled) ? " (Slettes)" : "";
                $scope.exposedByObj = !itInterface.exhibitedByItSystemId ? null : { id: itInterface.exhibitedByItSystemId, text: itInterface.exhibitedByItSystemName + isDisabled };

                itInterface.updateUrl = `api/itInterface/${itInterface.id}`;

                $scope.interface = itInterface;

                $scope.dataRows = [];
                _.each(dataRows, pushDataRow);

                $scope.saveLink = () => {
                    $http.patch(itInterface.updateUrl + "?organizationId=" + user.currentOrganizationId,
                        { url: itInterface.url })
                        .then(success => {
                            $scope.linkButtonDisabled = !Kitos.Utility.Validation.validateUrl(itInterface.url);
                            notify.addSuccessMessage("Feltet er opdateret");
                        },
                            error => notify.addErrorMessage("Fejl! Feltet kunne ikke ændres!"));
                }

                function pushDataRow(dataRow: any) {
                    dataRow.show = true;
                    dataRow.updateUrl = `api/dataRow/${dataRow.id}`;
                    dataRow.delete = () => {
                        var msg = notify.addInfoMessage("Fjerner rækken...", false);
                        $http.delete(dataRow.updateUrl + "?organizationId=" + user.currentOrganizationId).success(() => {
                            dataRow.show = false;
                            msg.toSuccessMessage("Rækken er fjernet!");
                        }).error(() => {
                            msg.toErrorMessage("Fejl! Kunne ikke fjerne rækken!");
                        });
                    };

                    $scope.dataRows.push(dataRow);
                }

                $scope.newDataRow = () => {

                    var payload = { itInterfaceId: itInterface.id };

                    var msg = notify.addInfoMessage("Tilføjer række...", false);
                    $http.post("api/dataRow", payload).success(result => {
                        pushDataRow(result.response);
                        msg.toSuccessMessage("Rækken er tilføjet!");
                    }).error(() => {
                        msg.toErrorMessage("Fejl! Kunne ikke tilføje rækken!");
                    });
                };

                $scope.itSystemsSelectOptions = select2LoadingService.loadSelect2("api/itsystem", true, [`organizationId=${user.currentOrganizationId}`, `take=25`], true);

                $scope.organizationSelectOptions = select2LoadingService.loadSelect2("api/organization", true, [`orgId=${user.currentOrganizationId}`, 'take=25'], false);


                function reload() {
                    $state.go(".", null, { reload: true });
                }

                function systemHasLocalUse(action: string): string {
                    return `Der er IT Systemer, som er i Lokal anvendelse som har denne snitfladerelation tilknyttet. Er du sikker på at du vil ${action} relationen?`;
                }

                $scope.save = () => {
                    // check if this interface is exhibited by any system that is in use (itsystemusage)
                    if (itInterface.isUsed) {
                        // clearing or changing the value must result in a dialog prompt
                        if ($scope.exposedByObj) {
                            if (!confirm(systemHasLocalUse("skifte"))) {
                                $scope.exposedByObj = { id: itInterface.exhibitedByItSystemId, text: itInterface.exhibitedByItSystemName };
                                return;
                            }
                        }
                        if (!confirm(systemHasLocalUse("fjerne"))) {
                            $scope.exposedByObj = { id: itInterface.exhibitedByItSystemId, text: itInterface.exhibitedByItSystemName };
                            return;
                        }
                    }

                    var msg = notify.addInfoMessage("Gemmer...", false);
                    if ($scope.exposedByObj) {
                        if (itInterface.exhibitedByItSystemId) {
                            // PATCH
                            var patchPayload = {
                                itSystemId: $scope.exposedByObj.id
                            };
                            var url = `api/exhibit/${itInterface.id}?organizationId=${user.currentOrganizationId}`;
                            $http({ method: "PATCH", url: url, data: patchPayload })
                                .success(() => {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                    reload();
                                })
                                .error(() => {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        } else {
                            // POST
                            var postPayload = {
                                itInterfaceId: itInterface.id,
                                itSystemId: $scope.exposedByObj.id
                            };
                            $http.post("api/exhibit", postPayload).success(() => {
                                msg.toSuccessMessage("Feltet er opdateret.");
                                reload();
                            }).error(() => {
                                msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                            });
                        }
                    } else {
                        // DELETE
                        $http.delete(`api/exhibit/${itInterface.id}?organizationId=${user.currentOrganizationId}`).success(() => {
                            msg.toSuccessMessage("Feltet er opdateret.");
                            reload();
                        }).error(() => {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                    }
                }
            }
        ]);
})(angular, app);

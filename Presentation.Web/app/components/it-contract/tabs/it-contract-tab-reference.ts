((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.references", {
            url: "/reference",
            templateUrl: "app/components/it-reference/it-reference.view.html",
            controller: "contract.EditReference",
            resolve: {
                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createContractReference()]
            }
        });
    }]);

    app.controller("contract.EditReference",
        ["$scope", "$state", "contract", "notify", "hasWriteAccess", "referenceService",
            ($scope, $state, contract, notify, hasWriteAccess, referenceService) => {
                
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.referenceName = contract.name;

                $scope.setChosenReference = id => {
                    var referenceId = (id === contract.referenceId) ? null : id;

                    var msg = notify.addInfoMessage("Opdaterer felt...", false);

                    referenceService.setOverviewReference(contract.id, contract.organizationId, referenceId)
                        .then(success => {
                                msg.toSuccessMessage("Feltet er opdateret!");
                                reload();
                            },
                            error => msg.toErrorMessage("Fejl! Prøv igen."));
                };

                $scope.deleteReference = referenceId => {
                    var msg = notify.addInfoMessage("Sletter...");

                    referenceService.deleteReference(referenceId, contract.organizationId)
                        .then(success => {
                                msg.toSuccessMessage("Slettet!");
                                reload();
                            },
                            error => msg.toErrorMessage("Fejl! Kunne ikke slette!"));
                };

                $scope.edit = refId => {
                    $state.go(".edit", { refId: refId, orgId: contract.organizationId });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                };

                $scope.mainGridOptions = {
                    dataSource: {
                        data: contract.externalReferences,
                        pageSize: 10
                    },
                    sortable: true,
                    pageable: {
                        refresh: false,
                        pageSizes: true,
                        buttonCount: 5
                    },
                    columns: [{
                        field: "title",
                        title: "Dokumenttitel",
                        template: data => {
                            if (Kitos.Utility.Validation.isValidExternalReference(data.url)) {
                                return `<a target="_blank" href="${data.url}">${data.title}</a>`;
                            } else {
                                return data.title;
                            }
                        },
                        width: 240
                    }, {
                        field: "externalReferenceId",
                        title: "Evt. dokumentID/Sagsnr./anden referenceContact"
                    }, {
                        field: "lastChanged",
                        title: "Ændret",
                        template: "#= kendo.toString(kendo.parseDate(lastChanged, 'yyyy-MM-dd'), 'dd. MMMM yyyy') #"
                    }, {
                        field: "lastChangedByUser.fullName",
                        title: "Senest ændret af",
                        width: 150
                    }, {
                        title: "Rediger",
                        template: dataItem => {
                            var HTML = `<button type='button' data-ng-disabled='${!$scope.hasWriteAccess}' class='btn btn-link' title='Redigér reference' data-ng-click="edit(${dataItem.id
                                })"><i class='fa fa-pencil'  aria-hidden='true'></i></button>`;
                            HTML += ` <button type='button' data-ng-disabled='${!$scope.hasWriteAccess
                                }' data-confirm-click="Er du sikker på at du vil slette?" class='btn btn-link' title='Slet reference' data-confirmed-click='deleteReference(${
                                dataItem.id})'><i class='fa fa-trash-o' aria-hidden='true'></i></button>`;
                            
                            if (dataItem.id === contract.referenceId) {
                                HTML = HTML + "<button data-uib-tooltip=\"Vises i overblik\" data-ng-disabled='" + !$scope.hasWriteAccess +"' tooltip-placement='right' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon chosen' src=\"/Content/img/VisIOverblik.svg\"/></button>";//valgt
                            } else {
                                HTML = HTML + "<button data-uib-tooltip=\"Vis objekt i overblik\" data-ng-disabled='" + !$scope.hasWriteAccess +"' tooltip-placement='right' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon' src=\"/Content/img/VisIOverblik.svg\"></img></button>";//vælg
                            }

                            return HTML;
                        }
                    }],
                    toolbar: [
                        {
                            name: "addReference",
                            text: "Tilføj reference",
                            template: () => {
                                if (hasWriteAccess) {
                                    return `<a id="addReference" class="btn btn-success btn-sm" href="\\#/contract/edit/${contract.id}/reference/createReference/${contract.id}">Tilføj reference</a>`;
                                }
                                else {
                                    return "";
                                }
                            }
                        }]
                };
            }]);
})(angular, app);

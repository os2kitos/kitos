((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.edit.references", {
            url: "/reference",
            templateUrl: "app/components/it-reference/it-reference.view.html",
            controller: "system.EditReference",
            resolve: {
                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createSystemReference()],
            }
        });
    }]);

    app.controller("system.EditReference", ["$scope", "$state", "notify", "hasWriteAccess", "referenceService", "itSystem",
        ($scope, $state, notify, hasWriteAccess, referenceService, itSystem) => {
            $scope.hasWriteAccess = hasWriteAccess;

            $scope.referenceName = itSystem.disabled ? itSystem.name + " - data i IT systemkatalog (Slettes)" : itSystem.name + " - data i IT systemkatalog";

            $scope.setChosenReference = id => {
                var referenceId = (id === itSystem.referenceId) ? null : id;

                var msg = notify.addInfoMessage("Opdaterer felt...", false);

                referenceService.setOverviewReference(itSystem.id, itSystem.organizationId, referenceId)
                    .then(success => {
                        msg.toSuccessMessage("Feltet er opdateret!");
                        reload();
                    },
                        error => msg.toErrorMessage("Fejl! Prøv igen."));
            };

            $scope.deleteReference = referenceId => {
                var msg = notify.addInfoMessage("Sletter...");

                referenceService.deleteReference(referenceId, itSystem.organizationId)
                    .then(success => {
                        msg.toSuccessMessage("Slettet!");
                        reload();
                    },
                        error => msg.toErrorMessage("Fejl! Kunne ikke slette!"));
            };

            $scope.edit = refId => {
                $state.go(".edit", { refId: refId, orgId: itSystem.organizationId });
            };

            function reload() {
                $state.go(".", null, { reload: true });
            };

            $scope.mainGridOptions = {
                dataSource: {
                    data: itSystem.externalReferences,
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
                    headerAttributes: {
                        "data-element-type": "referenceHeader"
                    },
                    attributes:
                    {
                        "data-element-type": "referenceObject"
                    },
                    template: data => {
                        if (Kitos.Utility.Validation.validateUrl(data.url)) {
                            return `<a target="_blank" href="${data.url}">${data.title}</a>`;
                        } else {
                            return data.title;
                        }
                    },
                    width: 240
                }, {
                    field: "externalReferenceId",
                    title: "Evt. dokumentID/Sagsnr./anden referenceContact",
                    headerAttributes: {
                        "data-element-type": "referenceHeaderId"
                    },
                    attributes:
                    {
                        "data-element-type": "referenceIdObject"
                    },
                }, {
                    field: "created",
                    title: "Oprettet",
                    template: "#= kendo.toString(kendo.parseDate(created, 'yyyy-MM-dd'), 'dd. MMMM yyyy') #"

                }, {
                    field: "objectOwner.fullName",
                    title: "Oprettet af",
                    width: 150
                }, {
                    title: "Rediger",
                    template: dataItem => {
                        var HTML = "<button type='button' data-ng-disabled='" + !$scope.hasWriteAccess + "' data-element-type='editReference' class='btn btn-link' title='Redigér reference' data-ng-click=\"edit(" + dataItem.id + ")\"><i class='fa fa-pencil' aria-hidden='true'></i></button>";
                        HTML += " <button type='button' data-ng-disabled='" + !$scope.hasWriteAccess + "' data-element-type='deleteReference' data-confirm-click=\"Er du sikker på at du vil slette?\" class='btn btn-link' title='Slet reference' data-confirmed-click='deleteReference(" + dataItem.id + ")'><i class='fa fa-trash-o' aria-hidden='true'></i></button>";


                        if (Kitos.Utility.Validation.validateUrl(dataItem.url)) {
                            if (dataItem.id === itSystem.referenceId) {
                                HTML = HTML + "<button data-uib-tooltip=\"Vises i overblik\" tooltip-placement='right' data-ng-disabled='" + !$scope.hasWriteAccess + "' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon chosen' src=\"/Content/img/VisIOverblik.svg\"/></button>";//valgt
                            } else {
                                HTML = HTML + "<button data-uib-tooltip=\"Vis objekt i overblik\"  tooltip-placement='right' data-ng-disabled='" + !$scope.hasWriteAccess + "' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon' src=\"/Content/img/VisIOverblik.svg\"></img></button>";//vælg

                            }
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
                                return "<a id=\"addReferenceItSystem\" class=\"btn btn-success btn-sm\" data-element-type=\"createReferenceButton\" href=\"\\#/system/edit/" + itSystem.id + "/reference/createReference/" + itSystem.id + "\"'>Tilføj reference</a>";
                            } else {
                                return "";
                            }
                        }
                    }]
            };

        }]);
})(angular, app);

((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.references", {
            url: "/reference",
            templateUrl: "app/components/it-reference/it-reference.view.html",
            controller: "it-system-usage.EditReference",
            resolve: {
                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createSystemUsageReference()]
            }
        });
    }]);

    app.controller("it-system-usage.EditReference", ["$scope", "$state", "itSystemUsage", "notify", "hasWriteAccess", "referenceService",
        ($scope, $state, itSystemUsage, notify, hasWriteAccess, referenceService) => {
            $scope.objectId = itSystemUsage.id;
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.referenceName = $scope.systemUsageName;


            $scope.setChosenReference = id => {
                var referenceId = (id === itSystemUsage.referenceId) ? null : id;

                var msg = notify.addInfoMessage("Opdaterer felt...", false);

                referenceService.setOverviewReference(itSystemUsage.id, itSystemUsage.organizationId, referenceId)
                    .then(success => {
                        msg.toSuccessMessage("Feltet er opdateret!");
                        reload();
                    },
                        error => msg.toErrorMessage("Fejl! Prøv igen."));
            };

            $scope.deleteReference = referenceId => {
                var msg = notify.addInfoMessage("Sletter...");

                referenceService.deleteReference(referenceId, itSystemUsage.organizationId)
                    .then(success => {
                        msg.toSuccessMessage("Slettet!");
                        reload();
                    },
                        error => msg.toErrorMessage("Fejl! Kunne ikke slette!"));
            };

            $scope.edit = refId => {
                $state.go(".edit", { refId: refId, orgId: itSystemUsage.organizationId });
            };

            function reload() {
                $state.go(".", null, { reload: true });
            };

            $scope.mainGridOptions = {
                dataSource: {
                    data: itSystemUsage.externalReferences,
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
                    template(data) {
                        if (Kitos.Utility.Validation.validateUrl(data.url)) {
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
                        var HTML = "<button type='button' data-ng-disabled='" + !$scope.hasWriteAccess + "' class='btn btn-link' title='Redigér reference' data-ng-click=\"edit(" + dataItem.id + ")\"><i class='fa fa-pencil' aria-hidden='true'></i></button>";
                        HTML += " <button type='button' data-confirm-click=\"Er du sikker på at du vil slette?\" class='btn btn-link' title='Slet reference' data-confirmed-click='deleteReference(" + dataItem.id + ")'><i class='fa fa-trash-o' aria-hidden='true'></i></button>";


                        if (Kitos.Utility.Validation.validateUrl(dataItem.url)) {
                            if (dataItem.id === itSystemUsage.referenceId) {
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
                        text: "Tilføj reference til IT System",
                        template: () => {
                            if (hasWriteAccess) {
                                return "<a id=\"addReferenceItSystemUsaged\" class=\"btn btn-success btn-sm\" href=\"\\#/system/usage/" + itSystemUsage.id + "/reference/createReference/" + itSystemUsage.id + "\"'>Tilføj reference</a>";
                            } else {
                                return "";
                            }
                        }
                    }]
            };
        }]);
})(angular, app);

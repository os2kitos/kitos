(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.edit.references", {
            url: "/reference",
            templateUrl: "app/components/it-reference/it-reference.view.html",
            controller: "system.EditReference",
            resolve: {
                theSystem: ["$http", "itSystem", ($http, itSystem) => $http.get(`odata/ItSystems(${itSystem.id})?$expand=ExternalReferences($expand=ObjectOwner)`).then(result => result.data)]
            }
        });
    }]);

    app.controller("system.EditReference", ["$scope", "$http", "$timeout", "$state", "$stateParams", "$confirm", "notify", "hasWriteAccess", "theSystem",
        function ($scope, $http, $timeout, $state, $stateParams, $confirm, notify, hasWriteAccess, theSystem) {
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.referenceName = theSystem.Name;

            $scope.setChosenReference = function (id) {
                var referenceId = (id === theSystem.ReferenceId) ? null : id;

                var data = {
                    ReferenceId: referenceId
                };

                var msg = notify.addInfoMessage("Opdaterer felt...", false);

                $http.patch("api/itSystem/" + theSystem.Id + "?organizationId=" + theSystem.OrganizationId, data)
                    .success(function (result) {
                        msg.toSuccessMessage("Feltet er opdateret!");
                        reload();
                    })
                    .error(function () {
                        msg.toErrorMessage("Fejl! Prøv igen.");
                    });
            };

            $scope.deleteReference = function (referenceId) {
                var msg = notify.addInfoMessage("Sletter...");

                $http.delete("api/Reference/" + referenceId + "?organizationId=" + theSystem.OrganizationId).success(() => {
                    msg.toSuccessMessage("Slettet!");
                }).error(() => {
                    msg.toErrorMessage("Fejl! Kunne ikke slette!");
                });
                reload();
            };

            $scope.edit = function (refId) {
                $state.go(".edit", { refId: refId, orgId: theSystem.OrganizationId });
            };

            function reload() {
                $state.go(".", null, { reload: true });
            };

            $scope.isValidUrl = url => {
                if (url) {
                    var regexp = /(http || https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
                    return regexp.test(url.toLowerCase());
                }
                return false;
            };

            $scope.mainGridOptions = {
                dataSource: {
                    data: theSystem.ExternalReferences,
                    pageSize: 10
                },
                sortable: true,
                pageable: {
                    refresh: false,
                    pageSizes: true,
                    buttonCount: 5
                },
                columns: [{
                    field: "Title",
                    title: "Dokumenttitel",
                    headerAttributes: {
                        "data-element-type": "referenceHeader"
                    },
                    attributes:
                    {
                        "data-element-type": "referenceObject"
                    },
                    template: data => {
                        if (Kitos.Utility.Validation.validateUrl(data.URL) ) {
                            return "<a target=\"_blank\" href=\"" + data.URL + "\">" + data.Title + "</a>";
                        } else {
                            return data.Title;
                        }
                    },
                    width: 240
                }, {
                    field: "ExternalReferenceId",
                    title: "Evt. dokumentID/Sagsnr./anden referenceContact"
                }, {
                    field: "Created",
                    title: "Oprettet",
                    template: "#= kendo.toString(kendo.parseDate(Created, 'yyyy-MM-dd'), 'dd. MMMM yyyy') #"

                }, {
                    field: "ObjectOwner.Name",
                    title: "Oprettet af",
                    template: (dataItem) => dataItem.ObjectOwner ? `${dataItem.ObjectOwner.Name} ${dataItem.ObjectOwner.LastName}` : "",
                    width: 150
                }, {
                    title: "Rediger",
                    template: dataItem => {
                        var HTML = "<button type='button' data-ng-disabled='" + !$scope.hasWriteAccess + "' data-element-type='EditReference' class='btn btn-link' title='Redigér reference' data-ng-click=\"edit(" + dataItem.Id + ")\"><i class='fa fa-pencil' aria-hidden='true'></i></button>";
                        if (dataItem.Id != theSystem.ReferenceId) {
                            HTML += " <button type='button' data-ng-disabled='" + !$scope.hasWriteAccess + "' data-confirm-click=\"Er du sikker på at du vil slette?\" class='btn btn-link' title='Slet reference' data-confirmed-click='deleteReference(" + dataItem.Id + ")'><i class='fa fa-trash-o' aria-hidden='true'></i></button>";
                        }

                        if (Kitos.Utility.Validation.validateUrl(dataItem.URL)) {
                            if (dataItem.Id === theSystem.ReferenceId) {
                                HTML = HTML + "<button data-uib-tooltip=\"Vises i overblik\" tooltip-placement='right' data-ng-disabled='" + !$scope.hasWriteAccess + "' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.Id + ")'><img class='referenceIcon chosen' src=\"/Content/img/VisIOverblik.svg\"/></button>";//valgt
                            } else {
                                HTML = HTML + "<button data-uib-tooltip=\"Vis objekt i overblik\"  tooltip-placement='right' data-ng-disabled='" + !$scope.hasWriteAccess + "' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.Id + ")'><img class='referenceIcon' src=\"/Content/img/VisIOverblik.svg\"></img></button>";//vælg

                            }
                        }

                        return HTML + ""
                    }
                }],
                toolbar: [
                    {
                        name: "addReference",
                        text: "Tilføj reference",
                        template: () => {
                            if (hasWriteAccess) {
                                return "<a id=\"addReferenceItSystem\" class=\"btn btn-success btn-sm\" href=\"\\#/system/edit/" + theSystem.Id + "/reference/createReference/" + theSystem.Id + "\"'>Tilføj reference</a>";
                            } else {
                                return "";
                            }
                        }
                    }]
            };

        }]);
})(angular, app);

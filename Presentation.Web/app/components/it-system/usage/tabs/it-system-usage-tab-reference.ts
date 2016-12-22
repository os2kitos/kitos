(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.references", {
            url: "/reference/",
            templateUrl: "app/components/it-reference/it-reference.view.html",
            controller: "it-system-usage.EditReference"
        });
    }]);

    app.controller("it-system-usage.EditReference",
        ["$scope", "$http", "$timeout", "$state", "$stateParams","itSystemUsage","$confirm","notify","hasWriteAccess",
            function ($scope, $http, $timeout, $state, $stateParams, itSystemUsage,$confirm,notify,hasWriteAccess) {

                $scope.objectId = itSystemUsage.id;

                $scope.objectReference = 'it-system.usage.references.create';
                
                $scope.references = itSystemUsage.externalReferences;

                $scope.setChosenReference = function (id) {

                    var data = {
                        referenceId: id
                    };

                    var msg = notify.addInfoMessage("Opdaterer felt...", false);

                    $http.patch("api/itSystemUsage/" + itSystemUsage.id + "?organizationId=" + itSystemUsage.organizationId, data)
                        .success(function (result) {
                            //   $scope.chosenReference = id;
                            msg.toSuccessMessage("Feltet er opdateret!");
                            reload();
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Prøv igen.");
                        });
                };


                $scope.deleteReference = function (referenceId) {
                    var msg = notify.addInfoMessage("Sletter...");
                    
                    $http.delete('api/Reference/' + referenceId + '?organizationId=' + itSystemUsage.organizationId).success(() => {
                        msg.toSuccessMessage("Slettet!");
                    }).error(() => {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                        });
                    reload();
                };

                $scope.isValidUrl = function (url) {
                    if (url) {
                        var regexp = /(http || https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
                        return regexp.test(url.toLowerCase());
                    }
                    return false;
                };

                $scope.edit = function (refId) {
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
                        template: function (data) {
                            if ($scope.isValidUrl(data.url)) {
                                return "<a href=\"" + data.url + "\">" + data.title + "</a>";
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
                            var HTML = "<button type='button' class='btn btn-link' title='Redigér reference' data-ng-click=\"edit(" + dataItem.id + ")\"><i class='fa fa-pencil'  aria-hidden='true'></i></button>"
                                + " <button type='button' data-confirm-click=\"Er du sikker på at du vil slette?\" class='btn btn-link' title='Slet reference' data-confirmed-click='deleteReference(" + dataItem.id + ")'><i class='fa fa-trash-o'  aria-hidden='true'></i></button>";

                            if ($scope.isValidUrl(dataItem.url)) {
                                if (dataItem.id === itSystemUsage.referenceId) {
                                    HTML = HTML + "<button data-uib-tooltip=\"Vises i overblik\" tooltip-placement='right' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon chosen' src=\"/Content/img/VisIOverblik.svg\"/></button>";//valgt
                                } else {
                                    HTML = HTML + "<button data-uib-tooltip=\"Vis objekt i overblik\"  tooltip-placement='right' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon' src=\"/Content/img/VisIOverblik.svg\"></img></button>";//vælg

                                }
                            }

                            return HTML;
                        }
                    }],
                    toolbar: [
                        {
                            name: "addReference",
                            text: "Tilføj reference",
                            template: "<a id=\"addReferenceasdasd\" data-ng-disabled=\"" + !hasWriteAccess +"\" class=\"btn btn-success btn-sm\" href=\"\\#/system/usage/" + itSystemUsage.id + "/reference/createReference/" + itSystemUsage.id + "\"'>#=text#</a>"
                        }]
                };
            }]);
})(angular, app);

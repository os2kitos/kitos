module Kitos.DataProcessing.Registration.Edit.Ref {
    "use strict";

    export class EditRefDataProcessingRegistrationController {
        static $inject: Array<string> = [
            "$scope",
            "$state",
            "user",
            "notify",
            "hasWriteAccess",
            "referenceService",
            "dataProcessingRegistration",
            "dataProcessingRegistrationService"
        ];

        constructor(
            private readonly $scope,
            $state: angular.ui.IStateService,
            private readonly user: Services.IUser,
            notify,
            public hasWriteAccess,
            referenceService: Services.IReferenceService,
            private dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO,
            private dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService) {

            $scope.referenceName = dataProcessingRegistration.name;

            this.$scope.mainGridOptions = {
                dataSource: {
                    data: dataProcessingRegistration.references,
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
                        "data-element-type": "referenceNameHeader"
                    },
                    attributes:
                    {
                        "data-element-type": "referenceObject"
                    },
                    template: data => {
                        if (Utility.Validation.isValidExternalReference(data.url)) {
                            return "<a target=\"_blank\" href=\"" + data.url + "\">" + data.name + "</a>";
                        } else {
                            return data.name;
                        }
                    },
                    width: 240
                }, {
                    field: "referenceId",
                    title: "Evt. dokumentID/Sagsnr./anden referenceContact",
                    headerAttributes: {
                        "data-element-type": "referenceHeaderId"
                    },
                    attributes:
                    {
                        "data-element-type": "referenceIdObject"
                    },
                }, {
                    field: "createdAt",
                    title: "Oprettet",
                        template: "#= kendo.toString(kendo.parseDate(createdAt, 'yyyy-MM-dd'), 'dd. MMMM yyyy') #"

                }, {
                    field: "createdByUser.name",
                    title: "Oprettet af",
                    width: 150
                }, {
                    title: "Rediger",
                    template: dataItem => {
                        var HTML = "<button type='button' data-element-type='editReference' data-ng-disabled='" + !this.hasWriteAccess + "' class='btn btn-link' title='Redigér reference' data-ng-click=\"edit(" + dataItem.id + ")\"><i class='fa fa-pencil' aria-hidden='true'></i></button>";
                        HTML += " <button type='button' data-element-type='deleteReference' data-ng-disabled='" + !this.hasWriteAccess + "' data-confirm-click=\"Er du sikker på at du vil slette?\" class='btn btn-link' title='Slet reference' data-confirmed-click='deleteReference(" + dataItem.id + ")'><i class='fa fa-trash-o'  aria-hidden='true'></i></button>";

                        if (Utility.Validation.isValidExternalReference(dataItem.url)) {
                            if (dataItem.masterReference) {
                                HTML = HTML + "<button data-ng-disabled='" + !this.hasWriteAccess + "' data-uib-tooltip=\"Vises i overblik\" tooltip-placement='right' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon chosen' src=\"/Content/img/VisIOverblik.svg\"/></button>";//valgt
                            }
                            else {
                                HTML = HTML + "<button data-ng-disabled='" + !this.hasWriteAccess + "' data-uib-tooltip=\"Vis objekt i overblik\"  tooltip-placement='right' class='btn btn-link' data-ng-click='setChosenReference(" + dataItem.id + ")'><img class='referenceIcon' src=\"/Content/img/VisIOverblik.svg\"></img></button>";//vælg
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
                            if (this.hasWriteAccess) {
                                return `<a id="addReference" data-element-type="createReferenceButton" class="btn btn-success btn-sm" href="\\#/data-processing/edit/${dataProcessingRegistration.id}/reference/createReference/${dataProcessingRegistration.id}"'>Tilføj reference</a>`;
                            } else {
                                return "";
                            }
                        }
                    }]
            };

            this.$scope.deleteReference = referenceId => {
                var msg = notify.addInfoMessage("Sletter...");

                referenceService.deleteReference(referenceId, user.currentOrganizationId)
                    .then(success => {
                        msg.toSuccessMessage("Slettet!");
                        reload();
                    },
                        error => msg.toErrorMessage("Fejl! Kunne ikke slette!"));
            };

            this.$scope.setChosenReference = id => {

                var msg = notify.addInfoMessage("Opdaterer felt...", false);

                this.dataProcessingRegistrationService.setMasterReference(this.dataProcessingRegistration.id, id).then(
                    nameChangeResponse => {
                        msg.toSuccessMessage("Feltet er opdateret!");
                        reload();
                    },
                    (errorResponse: Models.Api.ApiResponseErrorCategory) => {
                        switch (errorResponse) {
                            default:
                                msg.toErrorMessage("Fejl! Kunne ikke opdatere feltet!");
                                break;
                        }
                    });
            }

            $scope.edit = id => {
                $state.go(".edit", { refId: id, orgId: this.user.currentOrganizationId });
            };

            function reload() {
                $state.go(".", null, { reload: true });
            };
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.reference", {
                url: "/reference",
                templateUrl: "app/components/it-reference/it-reference.view.html",
                controller: EditRefDataProcessingRegistrationController,
                controllerAs: "vm",
                resolve: {
                    referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createDpaReference()],
                },
            });
        }]);
}
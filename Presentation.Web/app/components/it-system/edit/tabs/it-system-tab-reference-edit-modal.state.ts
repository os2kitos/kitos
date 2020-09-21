module Kitos.ItSystem.Catalog.Edit.Reference.Edit {
    "use strict";

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("it-system.edit.references.edit", {
                url: "/editReference/:refId/:orgId",
                onEnter: [
                    "$state", "$stateParams", "$uibModal", "referenceServiceFactory",
                    ($state: ng.ui.IStateService, $stateParams, $uibModal: ng.ui.bootstrap.IModalService, referenceServiceFactory) => {
                        var referenceService = referenceServiceFactory.createSystemReference();
                        $uibModal.open({
                            templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createSystemReference()],
                                reference: [() => referenceService.getReference($stateParams.refId).then(result => result)]
                            },
                            controller: Shared.GenericTabs.Reference.EditModalReferenceController,
                            controllerAs: "vm",
                        }).result.then(() => {

                        },
                            () => {
                                $state.go("^");
                            });
                    }
                ]
            });
        }]);
}


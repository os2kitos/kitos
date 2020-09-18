module Kitos.ItSystem.Overview.Usage.Reference.Create {
    "use strict";
    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("it-system.usage.references.create", {
                url: "/createReference/:id",
                onEnter: [
                    "$state", "$uibModal",
                    ($state: ng.ui.IStateService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/it-reference/it-reference-modal.view.html",
                            windowClass: "modal fade in",
                            resolve: {
                                referenceService: ["referenceServiceFactory", (referenceServiceFactory) => referenceServiceFactory.createSystemUsageReference()],
                            },
                            controller: Shared.GenericTabs.Reference.CreateModalReferenceController,
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


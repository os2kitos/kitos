module Kitos.DataProcessing.Agreement.Edit.ItSystems {
    "use strict";

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-agreement.it-systems", {
                url: "/it-systems",
                templateUrl: "app/shared/select-it-systems/generic-tab-it-systems.view.html",
                controller: Shared.GenericTabs.SelectItSystems.SelectItSystemsController,
                controllerAs: "vm",
                resolve: {
                    systemSelectionOptions: [
                        "hasWriteAccess", "dataProcessingAgreement", "dataProcessingAgreementService",
                        (hasWriteAccess: boolean, dataProcessingAgreement: Models.DataProcessing.IDataProcessingAgreementDTO, dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService) =>
                            <Shared.GenericTabs.SelectItSystems.IGenericItSystemsSelectionConfiguration>
                            { 
                                ownerName: dataProcessingAgreement.name,
                                overviewHeader: "Databehandleraftalen vedrører følgende IT Systemer", 

                                //TODO: replace these 3 with a lambda which returns a promise of the id,Name dto type
                                searchUrl: dataProcessingAgreementService.getAvailableSystemsUrl(dataProcessingAgreement.id),
                                searchUrlQueryComponent: "nameQuery",
                                searchUrlPageSizeComponent: "pageSize=50",

                                assignedSystems: dataProcessingAgreement.itSystems ? dataProcessingAgreement.itSystems.map(system => {
                                    //Transform into expected view model
                                    return {
                                        id: system.id,
                                        itSystem: system 
                                    }
                                }) : [],
                                hasWriteAccess: hasWriteAccess,
                                assign: (systemId: number) => dataProcessingAgreementService.assignSystem(dataProcessingAgreement.id, systemId),
                                remove: (systemId: number) => dataProcessingAgreementService.removeSystem(dataProcessingAgreement.id, systemId)
                            }
                    ]
                }
            });
        }]);
}

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
                                searchFunction:
                                    (query: string) =>
                                        dataProcessingAgreementService
                                            .getAvailableSystems(dataProcessingAgreement.id, query)
                                            .then
                                            (
                                                results =>
                                                    results
                                                        .map(result =>
                                                            <Models.ViewModel.Generic.Select2OptionViewModel>
                                                            {
                                                                id: result.id,
                                                                text: Helpers.SystemNameFormat.apply(result.name,result.disabled)
                                                            }),
                                                _ => []
                                            ),

                                assignedSystems: dataProcessingAgreement.itSystems ? dataProcessingAgreement.itSystems.map(system => {
                                    return <Shared.GenericTabs.SelectItSystems.ISystemViewModel>{
                                        id: system.id,
                                        name: system.name,
                                        disabled: system.disabled
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

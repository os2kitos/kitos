module Kitos.DataProcessing.Registration.Edit.ItSystems {
    "use strict";

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-registration.it-systems", {
                url: "/it-systems",
                templateUrl: "app/shared/select-it-systems/generic-tab-it-systems.view.html",
                controller: Shared.GenericTabs.SelectItSystems.SelectItSystemsController,
                controllerAs: "vm",
                resolve: {
                    systemSelectionOptions: [
                        "hasWriteAccess", "dataProcessingRegistration", "dataProcessingRegistrationService",
                        (hasWriteAccess: boolean, dataProcessingRegistration: Models.DataProcessing.IDataProcessingRegistrationDTO, dataProcessingRegistrationService: Services.DataProcessing.IDataProcessingRegistrationService) =>
                            <Shared.GenericTabs.SelectItSystems.IGenericItSystemsSelectionConfiguration>
                            {
                                ownerName: dataProcessingRegistration.name,
                                overviewHeader: "Databehandleraftalen vedrører følgende IT Systemer",
                                searchFunction:
                                    (query: string) =>
                                        dataProcessingRegistrationService
                                            .getAvailableSystems(dataProcessingRegistration.id, query)
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

                                assignedSystems: dataProcessingRegistration.itSystems ? dataProcessingRegistration.itSystems.map(system => {
                                    return <Shared.GenericTabs.SelectItSystems.ISystemViewModel>{
                                        id: system.id,
                                        name: system.name,
                                        disabled: system.disabled
                                    }
                                }) : [],
                                hasWriteAccess: hasWriteAccess,
                                assign: (systemId: number) => dataProcessingRegistrationService.assignSystem(dataProcessingRegistration.id, systemId),
                                remove: (systemId: number) => dataProcessingRegistrationService.removeSystem(dataProcessingRegistration.id, systemId)
                            }
                    ]
                }
            });
        }]);
}

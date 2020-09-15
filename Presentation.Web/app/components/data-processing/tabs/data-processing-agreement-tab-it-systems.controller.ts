module Kitos.DataProcessing.Agreement.Edit.Main {
    "use strict";

    export class EditSystemsDataProcessingAgreementController {
        static $inject: Array<string> = [
            "dataProcessingAgreementService",
            "notify",
            "hasWriteAccess",
            "dataProcessingAgreement",
            "$scope",
            "$state",
            "select2LoadingService"
        ];

        constructor(
            dataProcessingAgreementService: Services.DataProcessing.IDataProcessingAgreementService,
            notify,
            hasWriteAccess : boolean,
            dataProcessingAgreement: Models.DataProcessing.IDataProcessingAgreementDTO,
            $scope,
            $state: ng.ui.IStateService,
            select2LoadingService: Services.ISelect2LoadingService) {
            var viewModels = [];
            if (dataProcessingAgreement.itSystems) {
                dataProcessingAgreement
                    .itSystems
                    .forEach(system =>
                        viewModels.push(
                            {
                                id: system.id,
                                itSystem: system,
                            }
                        )
                    );
            }

            const reload = () => $state.go(".", null, { reload: true });
            $scope.itSysAssignmentVm = {
                ownerName: dataProcessingAgreement.name,
                hasWriteAccess: hasWriteAccess,
                overviewHeader: "Databehandleraftalen vedrører følgende IT Systemer",
                systemUsages: viewModels,
                formatSystemName: Helpers.SystemNameFormat.apply,
                delete:
                    systemId =>
                        dataProcessingAgreementService
                        .removeSystem(dataProcessingAgreement.id, systemId)
                        .then(
                            _ => {
                                notify.addSuccessMessage("Systemets tilknyttning er fjernet.");
                                reload();
                            },
                            _ => notify.addErrorMessage("Fejl! Kunne ikke fjerne systemets tilknyttning!")
                        ),
                save: () => {
                    const selectedItem = $scope.itSysAssignmentVm.selectedSystemUsage;
                    if (selectedItem) {
                        return dataProcessingAgreementService
                            .assignSystem(dataProcessingAgreement.id, $scope.itSysAssignmentVm.selectedSystemUsage.id)
                            .then(
                                _ => {
                                    notify.addSuccessMessage("Systemet er tilknyttet.");
                                    reload();
                                },
                                _ => notify.addErrorMessage("Fejl! Kunne ikke tilknytte systemet!")
                            );
                    }
                }
                ,
                itSystemUsagesSelectOptions: select2LoadingService.loadSelect2(dataProcessingAgreementService.getAvailableSystemsUrl(dataProcessingAgreement.id), true, ["pageSize=50"], false, "nameQuery"),
                selectedSystemUsage: null
            }
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("data-processing.edit-agreement.it-systems", {
                url: "/it-systems",
                templateUrl: "app/shared/select-it-systems/generic-tab-it-systems.view.html",
                controller: EditSystemsDataProcessingAgreementController,
                controllerAs: "vm"
            });
        }]);
}

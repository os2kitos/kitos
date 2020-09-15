module Kitos.Shared.GenericTabs.SelectItSystems {
    "use strict";

    export interface IGenericItSystemsSelectionConfiguration {
        ownerName: string;
        overviewHeader: string;
        searchUrl: string;
        searchUrlQueryComponent: string;
        searchUrlPageSizeComponent:string;
        assignedSystems: any[]; //TODO: Define a new presentation model and use that here
        hasWriteAccess: boolean;
        assign(id: number): angular.IPromise<any>;
        remove(id: number): angular.IPromise<any>;
    }

    export class SelectItSystemsController {
        static $inject: Array<string> = [
            "$scope",
            "notify",
            "$state",
            "select2LoadingService",
            "systemSelectionOptions"
        ];

        constructor(
            $scope,
            notify,
            $state: ng.ui.IStateService,
            select2LoadingService: Services.ISelect2LoadingService,
            systemSelectionOptions: IGenericItSystemsSelectionConfiguration) {
            const viewModels = systemSelectionOptions.assignedSystems || [];

            const reload = () => $state.go(".", null, { reload: true });
            $scope.itSysAssignmentVm = {
                ownerName: systemSelectionOptions.ownerName,
                hasWriteAccess: systemSelectionOptions.hasWriteAccess,
                overviewHeader: systemSelectionOptions.overviewHeader,
                systemUsages: viewModels,
                formatSystemName: Helpers.SystemNameFormat.apply,
                delete:
                    systemId =>
                        systemSelectionOptions.remove(systemId)
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
                        systemSelectionOptions
                            .assign($scope.itSysAssignmentVm.selectedSystemUsage.id)
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
                //TODO: Missing data handler - fix by injecting a search method in stead which must deliver a select2 compliant output! -> this config hell is not go
                itSystemUsagesSelectOptions: select2LoadingService.loadSelect2(systemSelectionOptions.searchUrl, false, [systemSelectionOptions.searchUrlPageSizeComponent], false, systemSelectionOptions.searchUrlQueryComponent),
                selectedSystemUsage: null
            }
        }
    }
}

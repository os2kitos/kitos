module Kitos.Shared.GenericTabs.SelectItSystems {
    "use strict";

    export interface ISystemViewModel {
        id: number;
        name: string;
        disabled: boolean;
    }

    export interface IGenericItSystemsSelectionConfiguration {
        ownerName: string;
        overviewHeader: string;
        searchFunction: Services.Select2AsyncDataSource;
        assignedSystems: ISystemViewModel[];
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
                itSystemUsagesSelectOptions: select2LoadingService.loadSelect2WithDataSource(systemSelectionOptions.searchFunction, false),
                selectedSystemUsage: null
            }
        }
    }
}

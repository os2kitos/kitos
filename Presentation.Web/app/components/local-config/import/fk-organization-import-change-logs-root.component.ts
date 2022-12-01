module Kitos.LocalAdmin.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                organizationUuid: "@"
            },
            controller: FkOrganizationImportChangeLogRootController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/import/fk-organization-import-change-logs-root.view.html`
        };
    }

    interface ISelect2ChangeLogModel {
        selectedElement: Models.ViewModel.Generic.Select2OptionViewModel<Kitos.Models.ViewModel.Organization.IFkOrganizationConnectionChangeLogsViewModel>;
        select2Config: any,
        elementSelected: (newElement: Models.ViewModel.Generic.Select2OptionViewModel<Kitos.Models.ViewModel.Organization.IFkOrganizationConnectionChangeLogsViewModel>) => void;
    }

    interface IFkOrganizationImportChangeLogRootController {
        organizationUuid: string;
    }

    class FkOrganizationImportChangeLogRootController implements IFkOrganizationImportChangeLogRootController {
        organizationUuid: string | null = null;

        changeLogs: Array<Kitos.Models.ViewModel.Organization.IFkOrganizationConnectionChangeLogsViewModel> = [];
        selectedChangeLog: Models.ViewModel.Generic.Select2OptionViewModel<Kitos.Models.ViewModel.Organization.IFkOrganizationConnectionChangeLogsViewModel> | null = null;

        isChangeLogLoaded = false;

        selectChangeLogModel: ISelect2ChangeLogModel;

        private readonly maxNumberOfLogs = 5;

        static $inject: string[] = ["stsOrganizationSyncService", "select2LoadingService"];
        constructor(
            private readonly stsOrganizationSyncService: Services.Organization.IStsOrganizationSyncService,
            private readonly select2LoadingService: Services.ISelect2LoadingService) {
        }

        $onInit() {
            if (!this.organizationUuid) {
                console.error("Missing parameter 'organizationUuid'");
                return;
            }

            this.stsOrganizationSyncService.getConnectionChangeLogs(this.organizationUuid, this.maxNumberOfLogs)
                .then(
                    response => {
                        this.changeLogs.pushArray(response);
                        this.changeLogs.forEach((x, index) => x.id = index);

                        this.bindChangeLogModel();
                        this.isChangeLogLoaded = true;
                    },
                    error => {
                        console.log(error);
                    });

        }

        bindChangeLogModel() {
            var optionMap = Helpers.ConnectionChangeLogHelper.createDictionaryFromChangeLogList(this.changeLogs);
            const options = this.changeLogs.map(option => optionMap[option.id]);
            
            this.selectChangeLogModel = {
                selectedElement: this.selectedChangeLog,
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(
                    () => options,
                    true,
                    (changeLog: { optionalObjectContext: Kitos.Models.ViewModel.Organization.IFkOrganizationConnectionChangeLogsViewModel }) => Helpers.Select2OptionsFormatHelper.formatChangeLog(changeLog.optionalObjectContext)),
                elementSelected: (newElement: Models.ViewModel.Generic.Select2OptionViewModel<Kitos.Models.ViewModel.Organization.IFkOrganizationConnectionChangeLogsViewModel>) => {
                    this.selectedChangeLog = newElement;
                }
            };
        }
    }

    angular.module("app")
        .component("fkOrganizationImportChangeLogsRoot", setupComponent());
}
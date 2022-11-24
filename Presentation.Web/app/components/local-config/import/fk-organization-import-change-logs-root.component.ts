module Kitos.LocalAdmin.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                organizationUuid: "@"
            },
            controller: FkOrganizationImportChangeLogController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/import/fk-organization-import-change-logs-root.view.html`
        };
    }

    interface IFkOrganizationImportChangeLogController extends ng.IComponentController, Utility.KendoGrid.IGridViewAccess<IConsequenceRow> {
        organizationUuid: string;
    }

    class FkOrganizationImportChangeLogController implements IFkOrganizationImportChangeLogController {
        organizationUuid: string | null = null;
        mainGrid: IKendoGrid<IConsequenceRow>;
        mainGridOptions: IKendoGridOptions<IConsequenceRow>;

        changeLogs: Array<Models.Api.Organization.ConnectionChangeLogDTO> = [];
        selectedChangeLog: Models.Api.Organization.ConnectionChangeLogDTO;

        isChangeLogLoaded = false;

        selectChangeLogModel = {};

        private readonly numberOfLogs = 5;

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

            this.stsOrganizationSyncService.getConnectionChangeLogs(this.organizationUuid, this.numberOfLogs)
                .then(
                    response => {
                        this.changeLogs.pushArray(response);
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
                selectedElement: this.selectedChangeLog && optionMap[this.selectedChangeLog.id],
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(
                    () => options,
                    true,
                    (changeLog: { optionalObjectContext: Models.Api.Organization.ConnectionChangeLogDTO }) => Helpers.Select2OptionsFormatHelper.formatChangeLog(changeLog.optionalObjectContext)),
                elementSelected: (newElement) => {
                    this.selectedChangeLog = newElement.optionalObjectContext;
                }
            };
        }
    }

    angular.module("app")
        .component("fkOrganizationImportChangeLogsRoot", setupComponent());
}
module Kitos.LocalAdmin.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                organizationUuid: "@"
            },
            controller: FkOrganizationImportChangeLogLogController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/import/fk-organization-import-change-logs-root.view.html`
        };
    }

    interface IFkOrganizationImportChangeLogController extends ng.IComponentController, Utility.KendoGrid.IGridViewAccess<IConsequenceRow> {
        organizationUuid: string;
    }

    class FkOrganizationImportChangeLogLogController implements IFkOrganizationImportChangeLogController {
        organizationUuid: string | null = null;
        mainGrid: IKendoGrid<IConsequenceRow>;
        mainGridOptions: IKendoGridOptions<IConsequenceRow>;

        changeLogs: Array<Models.Api.Organization.ConnectionChangeLogDTO> = [];
        selectedChangeLog: Models.Api.Organization.ConnectionChangeLogDTO;
        isChangeLogSelected = false;
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
            let existingChoice = null;
            if (this.selectedChangeLog?.id) {
                existingChoice = this.selectedChangeLog;

                if (!optionMap[existingChoice.id]) {
                    optionMap[existingChoice.id] = {
                        text: Helpers.ConnectionChangeLogHelper.getResponsibleEntityTextBasedOnOrigin(existingChoice),
                        id: existingChoice.id,
                        disabled: true,
                        optionalObjectContext: existingChoice
                    }
                }
            }

            const options = this.changeLogs.map(option => optionMap[option.id]);

            this.selectChangeLogModel = {
                selectedElement: existingChoice && optionMap[existingChoice.id],
                select2Config: this.select2LoadingService.select2LocalDataNoSearch(() => options, true),
                elementSelected: (newElement) => {
                    this.selectedChangeLog = newElement.optionalObjectContext;
                    this.isChangeLogSelected = true;
                }
            };
        }
    }

    angular.module("app")
        .component("fkOrganizationImportChangeLogsRoot", setupComponent());
}
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

        changeLogs: Array<Models.Api.Organization.ConnectionChangeLogDTO>;
        selectedChangeLog: Models.Api.Organization.ConnectionChangeLogDTO;
        isChangeLogSelected = false;
        selectChangeLogModel: any;

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
                    response => this.changeLogs.pushArray(response),
                    error => {
                        console.log(error);
                    });
        }

        bindChangeLogModel() {
            var optionMap = Helpers.ConnectionChangeLogHelper.createDictionaryFromChangeLogList(this.changeLogs);
            let existingChoice = null;
            if (!this.selectedChangeLog.id) {
                existingChoice = this.selectedChangeLog;

                if (!optionMap[existingChoice.id]) {
                    optionMap[existingChoice.id] = {
                        text: Helpers.ConnectionChangeLogHelper.getDropdownTextBasedOnOrigin(existingChoice),
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

        /*function bindCriticalities(contract: any) {

            const optionMap = Kitos.Helpers.OptionEntityHelper.createDictionaryFromOptionList(criticalityOptions);

            //If selected state is expired, add it for presentation reasons
            let existingChoice = null;
            if (contract.criticalityId !== undefined && contract.criticalityId !== null) {
                existingChoice = {
                    id: contract.criticalityId,
                    name: `${contract.criticalityName} (udgået)`
                };

                if (!optionMap[existingChoice.id]) {
                    optionMap[existingChoice.id] = {
                        text: existingChoice.name,
                        id: existingChoice.id,
                        disabled: true,
                        optionalObjectContext: existingChoice
                    }
                }
            }

            const options = criticalityOptions.map(option => optionMap[option.Id]);

            $scope.criticality = {
                selectedElement: existingChoice && optionMap[existingChoice.id],
                select2Config: select2LoadingService.select2LocalDataNoSearch(() => options, true),
                elementSelected: (newElement) => {
                    var payload = { criticalityId: newElement ? newElement.id : null };
                    $scope.contract.criticalityId = newElement?.id;
                    patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                }
            };
        }*/

    }

    angular.module("app")
        .component("fkOrganizationImportChangeLogsRoot", setupComponent());
}
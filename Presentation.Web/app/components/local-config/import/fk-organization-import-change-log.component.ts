module Kitos.LocalAdmin.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                changeLog: "<"
            },
            controller: FkOrganizationImportChangeLogLogController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/import/fk-organization-import-change-log.view.html`
        };
    }
    
    interface IFkOrganizationImportChangeLogController extends ng.IComponentController, Utility.KendoGrid.IGridViewAccess<IConsequenceRow> {
        changeLog: Models.Api.Organization.ConnectionChangeLogDTO
    }

    class FkOrganizationImportChangeLogLogController implements IFkOrganizationImportChangeLogController {
        changeLog: Models.Api.Organization.ConnectionChangeLogDTO | null = null;
        mainGrid: IKendoGrid<IConsequenceRow>;
        mainGridOptions: IKendoGridOptions<IConsequenceRow>;
        responsibleEntityText: string;
        logTime: string;

        static $inject: string[] = [];

        $onInit() {
            if (!this.changeLog) {
                console.error("Missing parameter 'changeLog'");
                return;
            }

            this.responsibleEntityText = Helpers.ConnectionChangeLogHelper.getResponsibleEntityTextBasedOnOrigin(this.changeLog);
            this.logTime = Helpers.RenderFieldsHelper.renderDate(this.changeLog.logTime);
        }
    }

    angular.module("app")
        .component("fkOrganizationImportChangeLog", setupComponent());
}
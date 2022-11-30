module Kitos.LocalAdmin.Components {
    "use strict";

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                changeLog: "<"
            },
            controller: FkOrganizationImportChangeLogController,
            controllerAs: "ctrl",
            templateUrl: `app/components/local-config/import/fk-organization-import-change-log.view.html`
        };
    }
    
    interface IFkOrganizationImportChangeLogController{
        changeLog: Models.Api.Organization.ConnectionChangeLogDTO;
        responsibleEntityText: string;
        logTime: string;
    }

    class FkOrganizationImportChangeLogController implements IFkOrganizationImportChangeLogController {
        changeLog: Models.Api.Organization.ConnectionChangeLogDTO | null = null;
        responsibleEntityText: string | null = null;
        logTime: string | null = null;

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
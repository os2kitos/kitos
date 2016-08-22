module Kitos.Reports.Viewer {
    'use strict';
    export class ReportViewerController {
        Title: string;

        constructor() {
            this.Title = "This is a ReportViewer";
        }

    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state("reports.viewer", {
                    url: "/reportviewer",
                    templateUrl: "app/components/reports/report-viewer.view.html",
                    controller: ReportViewerController,
                    controllerAs: "vm",
                    resolve: {
                        user: ["userService", userService => userService.getUser()]
                    }
                });
            }
        ]);

}
module Kitos.Reports.Viewer {
    'use strict';

    export class ReportViewerController {
        Title: string;

        static $inject = ['user','report'];

        constructor(user: Models.IUser, report: Models.IReport) {
            this.Title = "This is a ReportViewer for report id:" + report.Id;

        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state("reports.viewer", {
                    url: "/reportviewer/{id:int}",
                    templateUrl: "app/components/reports/report-viewer.view.html",
                    controller: ReportViewerController,
                    controllerAs: "vm",
                    resolve: {
                        user: ["userService", userService => userService.getUser()],
                        report: ['reportService', '$stateParams', (rpt:Services.ReportService, $stateParams) => rpt.GetById($stateParams['id']).then(result => result.data)]
                    }
                });
            }
        ]);
}
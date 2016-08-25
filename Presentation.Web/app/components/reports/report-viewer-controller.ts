module Kitos.Reports.Viewer {
    "use strict";

    export class ReportViewerController {
        public title: string;
        public categories: any;
        public static $inject = ["user", "report", "stimulsoftService", "$timeout"];

        constructor(user: Models.IUser, report: Models.IReport, stimulsoftService: Kitos.Services.StimulsoftService, private $timeout: ng.ITimeoutService) {
            this.title = `This is a ReportViewer for report id:${report.Id}`;

            const options = stimulsoftService.getOptions();
            options.height = "100%";
            options.appearance.scrollbarsMode = true;
            options.toolbar.showDesignButton = true;
            options.appearance.fullScreenMode = false;

            var viewer = stimulsoftService.getViewer(options, "Viewer");
            viewer.showProcessIndicator();

            $timeout(() => {
                //Create a new report instance
                var stiReport= stimulsoftService.getReport();

                if (report.Definition && report.Definition.length > 0) {
                    //  Load reports from JSON object
                    stiReport.load(report.Definition);
                }

                //Assign the report to the viewer
                viewer.report = stiReport;
            }, 50);

            viewer.renderHtml("reportViewer");
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
                        report: ["reportService", "$stateParams", (rpt: Services.ReportService, $stateParams) => rpt.GetById($stateParams["id"]).then(result => result.data)]
                    }
                });
            }
        ]);
}
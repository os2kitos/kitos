module Kitos.Reports {

    export class ReportViewerController {
        public static $inject = ["stimulsoftService", "$timeout"];
        constructor(stimulsoftService: Kitos.Services.StimulsoftService, private $timeout: ng.ITimeoutService) {

            const options = stimulsoftService.getOptions();
            options.appearance.scrollbarsMode = true;
            options.toolbar.showDesignButton = true;
            options.appearance.fullScreenMode = true;

            var viewer = stimulsoftService.getViewer(options, "Viewer");
            viewer.showProcessIndicator();

            $timeout(() => {
                //Create a new report instance
                var stiReport = stimulsoftService.getReport();

                //    if (report.Definition && report.Definition.length > 0) {
                //        //  Load reports from JSON object
                //        stiReport.load(report.Definition);
                //    }

                //Assign the report to the viewer
                viewer.report = stiReport;
            }, 50);
            viewer.renderHtml("reportViewer");
        }
    }

    angular
        .module("reportApp")
        .config(["$stateProvider", ($stateProvider) => {
            $stateProvider.state("reports-viewer",
                {
                    url: "/viewer",
                    templateUrl: "reportApp/reports-viewer.view.html",
                    controller: ReportViewerController,
                    controllerAs: "vm"
                });
        }
        ]);

}
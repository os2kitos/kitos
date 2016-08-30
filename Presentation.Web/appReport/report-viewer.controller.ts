module Kitos.Reports {

    export class ReportViewerController {
        public static $inject = ["stimulsoftService", "$timeout"];

        constructor(stimulsoftService: Kitos.Services.StimulsoftService, private $timeout: ng.ITimeoutService) {

            const options = stimulsoftService.getOptions();
            options.appearance.scrollbarsMode = true;
            options.toolbar.showDesignButton = true;
            options.appearance.fullScreenMode = true;

            var viewer = stimulsoftService.getViewer(options, "Viewer");

            // Add the design button event
            viewer.onDesignReport = function (e) {
                this.visible = false;
                var designOptions = stimulsoftService.getDesignerOptions();
                designOptions.appearance.fullScreenMode = true;
                var designer = stimulsoftService.getDesigner(designOptions, "designer");

                designer.onSaveReport = function(saveEvent) {
                    viewer.report = saveEvent.report;
                    console.log("saving a report");
                }

                designer.onExit = function (exitEvent) {
                    console.log("Closing designer");
                    designer.visible = false;
                    //viewer.report = exitEvent.report;
                    viewer.visible = true;
                }


                designer.renderHtml("reportDesigner");
                designer.visible = true;
                designer.report = e.report;
            };

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

    

}
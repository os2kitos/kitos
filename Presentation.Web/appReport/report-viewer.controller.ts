
module Kitos.Reports {
    "use strict";

    export class ReportViewerController {
        public static $inject = ["stimulsoftService", "$timeout", "reportService", "$location"];
        viewer: any;
        report: Kitos.Models.IReport;

        constructor(private stimulsoftService: Kitos.Services.StimulsoftService,
            $timeout: ng.ITimeoutService,
            private reportService: Kitos.Services.ReportService,
            $location: ng.ILocationService) {

            const options = stimulsoftService.getOptions();
            options.appearance.scrollbarsMode = true;
            options.toolbar.showDesignButton = true;
            options.appearance.fullScreenMode = true;

            this.viewer = stimulsoftService.getViewer(options, "Viewer");

            // Add the design button event
            this.viewer.onDesignReport = function (e) {
                this.visible = false;
                var designOptions = stimulsoftService.getDesignerOptions();
                designOptions.appearance.fullScreenMode = true;
                var designer = stimulsoftService.getDesigner(designOptions, "designer");

                designer.onSaveReport = function (saveEvent) {
                    this.viewer.report = saveEvent.report;
                    console.log("saving a report");
                };

                designer.onExit = function (exitEvent) {
                    console.log("Closing designer");
                    designer.visible = false;
                    //viewer.report = exitEvent.report;
                    this.viewer.visible = true;
                };

                designer.renderHtml("reportDesigner");
                designer.visible = true;
                designer.report = e.report;
            };

            this.viewer.showProcessIndicator();
            this.viewer.renderHtml("reportViewer");
            this.loadReport();
        }

        loadReport = () => {
            this.reportService.GetById(1).then((result) => {
                this.report = result.data;
                let stiReport = this.stimulsoftService.getReport();

                if (this.report.Definition && this.report.Definition.length > 0) {
                    //  Load reports from JSON object
                    stiReport.load(this.report.Definition);
                }
                //Assign the report to the viewer
                this.viewer.report = stiReport;
            });
        };

        saveReport = () => {
            alert("Rapporten gemmes!");
        }
    }

    angular.module("reportApp").controller("reportViewerController", Kitos.Reports.ReportViewerController);
}

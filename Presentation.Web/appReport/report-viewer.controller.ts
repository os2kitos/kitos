
module Kitos.Reports {
    "use strict";

    export class ReportViewerController {
        public static $inject = ["stimulsoftService", "$timeout", "reportService", "$location"];
        viewer: any;
        designer: any;
        report: Kitos.Models.IReport;
        stiReport: any;

        constructor(private stimulsoftService: Kitos.Services.StimulsoftService,
            $timeout: ng.ITimeoutService,
            private reportService: Kitos.Services.ReportService,
            $location: ng.ILocationService) {
            let self = this;


            this.viewer = stimulsoftService.getViewer(this.buildViewerOptions(), "Viewer");

            // Add the design button event
            this.viewer.onDesignReport = function (e) {
                this.visible = false;
                // set designer options TODO load from db
                var designOptions = stimulsoftService.getDesignerOptions();
                designOptions.appearance.fullScreenMode = true;
                // create designer object
                this.designer = stimulsoftService.getDesigner(designOptions, "designer");
                // bind events to designer object
                this.designer.onExit = self.designerOnExit;
                this.designer.onSaveReport = self.designerSaveReport;
                
                // render designer on dom element
                this.designer.renderHtml("reportDesigner");
                this.designer.visible = true;
                this.designer.report = e.report;
            };

            this.viewer.showProcessIndicator();
            this.viewer.renderHtml("reportViewer");
            this.loadReport();
        }

        buildViewerOptions = () => {
            const options = this.stimulsoftService.getOptions();
            options.appearance.scrollbarsMode = true;
            options.toolbar.showDesignButton = true;
            options.appearance.fullScreenMode = true;
            options.showSaveDialog = false;
            return options;
        }

        loadReport = () => {
            this.reportService.GetById(1).then((result) => {
                this.report = result.data;
                this.stiReport = this.stimulsoftService.getReport();

                if (this.report.Definition && this.report.Definition.length > 0) {
                    //  Load reports from JSON object
                    this.stiReport.load(this.report.Definition);
                }
                //Assign the report to the viewer
                this.viewer.report = this.stiReport;
            });
        };

        designerSaveReport = (saveEvent) => {
            this.report.Definition = saveEvent.report.saveToJsonString();
            this.reportService.saveReport(this.report);
            this.viewer.report = saveEvent.report;
            console.log("saving a report");
            // save to DB
        }

        designerOnExit = (exitEvent) => {
            console.log("Closing designer");
            this.designer.visible = false;
            this.viewer.report = exitEvent.report;
            this.viewer.visible = true;
        }
    }

    angular.module("reportApp").controller("reportViewerController", Kitos.Reports.ReportViewerController);
}

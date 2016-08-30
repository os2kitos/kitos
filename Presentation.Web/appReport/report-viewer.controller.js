var Kitos;
(function (Kitos) {
    var Reports;
    (function (Reports) {
        "use strict";
        var ReportViewerController = (function () {
            function ReportViewerController(stimulsoftService, $timeout, reportService, $location) {
                var _this = this;
                this.stimulsoftService = stimulsoftService;
                this.reportService = reportService;
                this.buildViewerOptions = function () {
                    var options = _this.stimulsoftService.getOptions();
                    options.appearance.scrollbarsMode = true;
                    options.toolbar.showDesignButton = true;
                    options.appearance.fullScreenMode = true;
                    options.showSaveDialog = false;
                    return options;
                };
                this.loadReport = function () {
                    _this.reportService.GetById(1).then(function (result) {
                        _this.report = result.data;
                        _this.stiReport = _this.stimulsoftService.getReport();
                        if (_this.report.Definition && _this.report.Definition.length > 0) {
                            //  Load reports from JSON object
                            _this.stiReport.load(_this.report.Definition);
                        }
                        //Assign the report to the viewer
                        _this.viewer.report = _this.stiReport;
                    });
                };
                this.designerSaveReport = function (saveEvent) {
                    _this.report.Definition = saveEvent.report.saveToJsonString();
                    _this.reportService.saveReport(_this.report);
                    _this.viewer.report = saveEvent.report;
                    console.log("saving a report");
                    // save to DB
                };
                this.designerOnExit = function (exitEvent) {
                    console.log("Closing designer");
                    _this.designer.visible = false;
                    _this.viewer.report = exitEvent.report;
                    _this.viewer.visible = true;
                };
                var self = this;
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
            ReportViewerController.$inject = ["stimulsoftService", "$timeout", "reportService", "$location"];
            return ReportViewerController;
        }());
        Reports.ReportViewerController = ReportViewerController;
        angular.module("reportApp").controller("reportViewerController", Kitos.Reports.ReportViewerController);
    })(Reports = Kitos.Reports || (Kitos.Reports = {}));
})(Kitos || (Kitos = {}));
//# sourceMappingURL=report-viewer.controller.js.map
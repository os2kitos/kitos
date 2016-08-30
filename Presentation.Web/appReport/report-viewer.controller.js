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
                this.loadReport = function () {
                    _this.reportService.GetById(1).then(function (result) {
                        _this.report = result.data;
                        var stiReport = _this.stimulsoftService.getReport();
                        if (_this.report.Definition && _this.report.Definition.length > 0) {
                            //  Load reports from JSON object
                            stiReport.load(_this.report.Definition);
                        }
                        //Assign the report to the viewer
                        _this.viewer.report = stiReport;
                    });
                };
                var options = stimulsoftService.getOptions();
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
            ReportViewerController.$inject = ["stimulsoftService", "$timeout", "reportService", "$location"];
            return ReportViewerController;
        }());
        Reports.ReportViewerController = ReportViewerController;
        angular.module("reportApp").controller("reportViewerController", Kitos.Reports.ReportViewerController);
    })(Reports = Kitos.Reports || (Kitos.Reports = {}));
})(Kitos || (Kitos = {}));
//# sourceMappingURL=report-viewer.controller.js.map
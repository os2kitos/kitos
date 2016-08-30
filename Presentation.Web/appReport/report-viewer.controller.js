var Kitos;
(function (Kitos) {
    var Reports;
    (function (Reports) {
        var ReportViewerController = (function () {
            function ReportViewerController(stimulsoftService, $timeout) {
                this.$timeout = $timeout;
                var options = stimulsoftService.getOptions();
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
                    designer.onSaveReport = function (saveEvent) {
                        viewer.report = saveEvent.report;
                        console.log("saving a report");
                    };
                    designer.onExit = function (exitEvent) {
                        console.log("Closing designer");
                        designer.visible = false;
                        //viewer.report = exitEvent.report;
                        viewer.visible = true;
                    };
                    designer.renderHtml("reportDesigner");
                    designer.visible = true;
                    designer.report = e.report;
                };
                viewer.showProcessIndicator();
                $timeout(function () {
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
            ReportViewerController.$inject = ["stimulsoftService", "$timeout"];
            return ReportViewerController;
        }());
        Reports.ReportViewerController = ReportViewerController;
    })(Reports = Kitos.Reports || (Kitos.Reports = {}));
})(Kitos || (Kitos = {}));
//# sourceMappingURL=report-viewer.controller.js.map
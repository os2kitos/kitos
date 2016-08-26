module ReportApp {

    angular.module("reportApp", []);

    export class ReportAppCtrl {
        public title: string;

        public static $inject = ["stimulsoftService", "$timeout"];
        constructor(stimulsoftService: Kitos.Services.StimulsoftService, private $timeout: ng.ITimeoutService) {
            //constructor() {
            this.title = "Er der hul igennem, hva?";

            const options = stimulsoftService.getOptions();
            options.height = "100%";
            options.appearance.scrollbarsMode = true;
            options.toolbar.showDesignButton = true;
            options.appearance.fullScreenMode = false;

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

    angular.module("reportApp").controller("ReportAppCtrl", ReportAppCtrl);

    angular.element(document).ready(() => {
        angular.bootstrap(document, ["reportApp"]);
    });
}
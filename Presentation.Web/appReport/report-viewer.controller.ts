
module Kitos.Reports {
    "use strict";

    /*
    
    Use next commands to change language of viewer's GUI.
    Add a new language to the Select UI language menu. The file is uploaded to a client when the language is selected.
    JavaScript
    Stimulsoft.Base.Localization.StiLocalization.addLocalizationFile("Localizations/pt-BR.xml", false, "Portuguese (Brazil)");
    
    Add a new language to the Select UI language menu and upload the file to a client.
    JavaScript
    Stimulsoft.Base.Localization.StiLocalization.addLocalizationFile("Localizations/ro.xml", true);
    
    Add a new language to the Select UI language menu and set it as a current language.
    JavaScript
    Stimulsoft.Base.Localization.StiLocalization.setLocalizationFile("Localizations/es.xml");
    
    Remove all languages from the Select UI language menu, upload it to a client and set it as a current language.
    JavaScript
    Stimulsoft.Base.Localization.StiLocalization.setLocalizationFile("Localizations/es.xml", true);
    
    */


    export class ReportViewerController {
        public static $inject = ["stimulsoftService", "reportService", "$window", "notify"];
        viewer: any;
        designer: any;
        report: Kitos.Models.IReport;
        stiReport: any;

        emptyreport = {"ReportVersion":"2016.1.28","ReportGuid":"9ad83767ecc3b68cb62c086805556fce","ReportName":"Report","ReportAlias":"Report","ReportCreated":"/Date(1472651877000+0200)/","ReportChanged":"/Date(1472651877000+0200)/","EngineVersion":"EngineV2","CalculationMode":"Interpretation","Pages":{"0":{"Ident":"StiPage","Name":"Page1","Guid":"a4875a4e-cd43-da99-360b-a7560ca0b913","Interaction":{"Ident":"StiInteraction"},"Border":";;2;;;;;solid:Black","Brush":"solid:Transparent","Components":{"0":{"Ident":"StiText","Name":"Text1","MinSize":"0,0","MaxSize":"0,0","ClientRectangle":"0.6,4.8,17.4,4.2","Interaction":{"Ident":"StiInteraction"},"Text":{"Value":"Rapporten er tom og venter på at blive designet."},"HorAlignment":"Center","Font":"Verdana;28;;","Border":";;;;;;;solid:Black","Brush":"solid:Transparent","TextBrush":"solid:Black","TextOptions":{"WordWrap":true},"Type":"Expression"}},"PageWidth":21.01,"PageHeight":29.69,"Watermark":{"TextBrush":"solid:50,0,0,0"},"Margins":{"Left":1,"Right":1,"Top":1,"Bottom":1}}}}

        constructor(private stimulsoftService: Kitos.Services.StimulsoftService,
            private reportService: Kitos.Services.ReportService,
            private $window: ng.IWindowService,
            private notify) {
            let self = this;

            //stimulsoftService.setLocalizationFile("./locales/nb-NO.xml")
            this.viewer = stimulsoftService.getViewer(this.buildViewerOptions(), "Viewer");

            // Add the design button event
            this.viewer.onDesignReport = function (e) {
                this.visible = false;

                // create designer object
                this.designer = stimulsoftService.getDesigner(self.buildDesignerOptions(), "designer");
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
            this.loadReport(this.getReportId());
        }

        getReportId = () => {
            var searchObject = this.$window.location.search;
            var regex = /([0-9]+)/g;
            var matches = searchObject.match(regex);
            let id = matches[0];
            if (matches.length === 1) {
                return parseInt(id);
            }
        };

        buildViewerOptions = () => {
            let options = this.stimulsoftService.getViewerOptions();
            options.appearance.scrollbarsMode = true;
            options.toolbar.showDesignButton = true;
            options.appearance.fullScreenMode = true;
            return options;
        };

        buildDesignerOptions = () => {
            // set designer options TODO load from db
            let options = this.stimulsoftService.getDesignerOptions();
            options.appearance.fullScreenMode = true;
            options.appearance.showSaveDialog = false;
            return options;
        };

        loadReport = (id: number) => {
            this.reportService.GetById(id).then((result) => {
                this.report = result.data;
                this.stiReport = this.stimulsoftService.getReport();

                if (this.report.Definition && this.report.Definition.length > 0) {
                    //  Load reports from JSON object
                    this.stiReport.load(this.report.Definition);
                }
                else {
                    // this.reportService.GetEmptyReport().then((result) => {
                    //     var d = JSON.stringify(result.data); 
                    //     this.stiReport.load(d);
                    // });
                    this.stiReport.load(this.emptyreport);
                }
                //Assign the report to the viewer
                this.viewer.report = this.stiReport;
            })
        }


        designerSaveReport = (saveEvent) => {
            this.report.Definition = saveEvent.report.saveToJsonString();
            this.reportService.saveReport(this.report);
            this.viewer.report = saveEvent.report;
            this.notify.addSuccessMessage("Rapporten er gemt", true);
        };

        designerOnExit = (exitEvent) => {
            console.log("Closing designer");
            this.designer.visible = false;
            this.viewer.report = exitEvent.report;
            this.viewer.visible = true;
        }
    }

    angular.module("reportApp").controller("reportViewerController", Kitos.Reports.ReportViewerController);
}

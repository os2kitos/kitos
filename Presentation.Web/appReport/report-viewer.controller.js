var Kitos;
(function (Kitos) {
    var Reports;
    (function (Reports) {
        "use strict";
        var ReportViewerController = /** @class */ (function () {
            function ReportViewerController(stimulsoftService, reportService, $window, notify, userService, _, authorizationServiceFactory) {
                var _this = this;
                this.stimulsoftService = stimulsoftService;
                this.reportService = reportService;
                this.$window = $window;
                this.notify = notify;
                this.userService = userService;
                this._ = _;
                this.authorizationServiceFactory = authorizationServiceFactory;
                this.emptyreport = { "ReportVersion": "2016.1.28", "ReportGuid": "9ad83767ecc3b68cb62c086805556fce", "ReportName": "Report", "ReportAlias": "Report", "ReportAuthor": "", "ReportDescription": "", "ReportCreated": "/Date(1472651877000+0200)/", "ReportChanged": "/Date(1472651877000+0200)/", "EngineVersion": "EngineV2", "CalculationMode": "Interpretation", "Dictionary": { "Variables": { "0": { "Name": "CurrentOrganizationName", "DialogInfo": { "DateTimeType": "DateAndTime" }, "Alias": "CurrentOrganizationName", "Type": "System.String", "ReadOnly": true } } }, "Pages": { "0": { "Ident": "StiPage", "Name": "Page1", "Guid": "a4875a4e-cd43-da99-360b-a7560ca0b913", "Interaction": { "Ident": "StiInteraction" }, "Border": ";;2;;;;;solid:Black", "Brush": "solid:Transparent", "Components": { "0": { "Ident": "StiText", "Name": "Text1", "MinSize": "0,0", "MaxSize": "0,0", "ClientRectangle": "0.6,4.8,17.4,4.2", "Interaction": { "Ident": "StiInteraction" }, "Text": { "Value": "Rapporten er tom og venter på at blive designet." }, "HorAlignment": "Center", "Font": "Verdana;28;;", "Border": ";;;;;;;solid:Black", "Brush": "solid:Transparent", "TextBrush": "solid:Black", "TextOptions": { "WordWrap": true }, "Type": "Expression" } }, "PageWidth": 21.01, "PageHeight": 29.69, "Watermark": { "TextBrush": "solid:50,0,0,0" }, "Margins": { "Left": 1, "Right": 1, "Top": 1, "Bottom": 1 } } } };
                this.getReportId = function () {
                    var searchObject = _this.$window.location.search;
                    var regex = /([0-9]+)/g;
                    var matches = searchObject.match(regex);
                    var id = matches[0];
                    if (matches.length === 1) {
                        return parseInt(id);
                    }
                    return null;
                };
                this.buildViewerOptions = function () {
                    var options = _this.stimulsoftService.getViewerOptions();
                    options.appearance.scrollbarsMode = true;
                    options.toolbar.showDesignButton = _this.canDesignReport;
                    options.appearance.fullScreenMode = true;
                    return options;
                };
                this.buildDesignerOptions = function () {
                    // set designer options
                    var options = _this.stimulsoftService.getDesignerOptions();
                    options.appearance.fullScreenMode = true;
                    options.appearance.showSaveDialog = false;
                    return options;
                };
                this.loadReport = function (id, user) {
                    _this.reportService.GetById(id)
                        .then(function (result) {
                        _this.report = result.data;
                        _this.stiReport = _this.stimulsoftService.getReport();
                        var reportDef = _this.emptyreport;
                        if (_this.report.Definition && _this.report.Definition.length > 0) {
                            //  Load reports from JSON object
                            reportDef = JSON.parse(_this.report.Definition);
                        }
                        // Set standard values for report
                        reportDef.ReportName = _this.report.Name;
                        reportDef.ReportAlias = _this.report.Name;
                        reportDef.ReportAuthor = user.fullName;
                        reportDef.ReportDescription = _this.report.Description;
                        reportDef.ReportCreated = new Date().toString();
                        _this.stiReport.load(reportDef);
                        var addKitos = true;
                        var i;
                        for (i = 0; i < _this.stiReport.dictionary.databases.count; i++) {
                            if (_this.stiReport.dictionary.databases.getByIndex(i).name === "Kitos") {
                                addKitos = false;
                                _this.stiReport.dictionary.databases.getByIndex(i).connectionString = window.location.origin + "/odata";
                            }
                        }
                        if (addKitos) {
                            var odata = _this.stimulsoftService.getODataDatabase();
                            odata.name = "Kitos";
                            odata.alias = "Kitos";
                            odata.connectionString = window.location.origin + "/odata";
                            _this.stiReport.dictionary.databases.add(odata);
                            _this.stiReport.dictionary.synchronize();
                        }
                        // Add default variables to report
                        var currentOrgName = _this.stimulsoftService.getVariable();
                        currentOrgName.name = "CurrentOrganizationName";
                        currentOrgName.alias = "CurrentOrganizationName";
                        currentOrgName.value = user.currentOrganizationName;
                        currentOrgName.description = "Auto generated and injected variable";
                        currentOrgName.readOnly = true;
                        currentOrgName.requestFromUser = false;
                        currentOrgName.allowUseAsSqlParameter = false;
                        currentOrgName.typeT = typeof (String);
                        currentOrgName.category = "";
                        _this.stiReport.dictionary.variables.add(currentOrgName);
                        var currentOrgId = _this.stimulsoftService.getVariable();
                        currentOrgId.name = 'CurrentOrganizationId';
                        currentOrgId.alias = 'CurrentOrganizationId';
                        currentOrgId.value = user.currentOrganizationId.toString();
                        currentOrgId.description = "Auto generated and injected variable";
                        currentOrgId.readOnly = true;
                        currentOrgId.requestFromUser = false;
                        currentOrgId.allowUseAsSqlParameter = false;
                        currentOrgId.typeT = typeof (String);
                        currentOrgId.category = "";
                        _this.stiReport.dictionary.variables.add(currentOrgId);
                        var currentUserName = _this.stimulsoftService.getVariable();
                        currentUserName.name = 'CurrentUserName';
                        currentUserName.alias = 'CurrentUserName';
                        currentUserName.value = user.fullName;
                        currentUserName.description = "Auto generated and injected variable";
                        currentUserName.readOnly = true;
                        currentUserName.requestFromUser = false;
                        currentUserName.allowUseAsSqlParameter = false;
                        currentUserName.typeT = typeof (String);
                        currentUserName.category = "";
                        _this.stiReport.dictionary.variables.add(currentUserName);
                        var currentUserId = _this.stimulsoftService.getVariable();
                        currentUserId.name = 'CurrentUserId';
                        currentUserId.alias = 'CurrentUserId';
                        currentUserId.value = user.id.toString();
                        currentUserId.description = "Auto generated and injected variable";
                        currentUserId.readOnly = true;
                        currentUserId.requestFromUser = false;
                        currentUserId.allowUseAsSqlParameter = false;
                        currentUserId.typeT = typeof (String);
                        currentUserId.category = "";
                        _this.stiReport.dictionary.variables.add(currentUserId);
                        //Assign the report to the viewer
                        _this.viewer.report = _this.stiReport;
                    })
                        .catch((function (result) {
                        alert("Ingen adgang til rapporten!");
                    }));
                };
                this.designerSaveReport = function (saveEvent) {
                    _this.report.Definition = saveEvent.report.saveToJsonString();
                    _this.reportService.saveReport(_this.report);
                    _this.viewer.report = saveEvent.report;
                    _this.notify.addSuccessMessage("Rapporten er gemt", true);
                };
                this.designerOnExit = function (exitEvent) {
                    _this.designer.visible = false;
                    _this.viewer.report = exitEvent.report;
                    _this.viewer.visible = true;
                };
                var self = this;
                var reportId = this.getReportId();
                if (reportId !== null) {
                    authorizationServiceFactory
                        .createReportAuthorization()
                        .getAuthorizationForItem(reportId)
                        .then(function (accessRightsResponse) {
                        self.canDesignReport = accessRightsResponse.canEdit;
                        _this.userService.getUser()
                            .then(function (user) {
                            // 02/11 MEMA: The translation is far from done. Add back, when translated.
                            //stimulsoftService.setLocalizationFile("./appReport/locales/da-DK.xml")
                            _this.viewer = stimulsoftService.getViewer(_this.buildViewerOptions(), "Viewer");
                            // Add the design button event
                            _this.viewer.onDesignReport = function (e) {
                                this.visible = false;
                                // create designer object
                                this.designer = stimulsoftService.getDesigner(self.buildDesignerOptions(), "designer");
                                // bind events to designer object
                                this.designer.onExit = self.designerOnExit;
                                this.designer.onSaveReport = self.designerSaveReport;
                                // render designer on dom element
                                this.designer.renderHtml("reportDesigner");
                                this.designer.visible = self.canDesignReport;
                                this.designer.report = e.report;
                            };
                            _this.viewer.showProcessIndicator();
                            _this.viewer.renderHtml("reportViewer");
                            _this.loadReport(reportId, user);
                        });
                    });
                }
            }
            ReportViewerController.$inject = ["stimulsoftService", "reportService", "$window", "notify", "userService", "_", "authorizationServiceFactory"];
            return ReportViewerController;
        }());
        Reports.ReportViewerController = ReportViewerController;
        angular.module("reportApp").controller("reportViewerController", Kitos.Reports.ReportViewerController);
    })(Reports = Kitos.Reports || (Kitos.Reports = {}));
})(Kitos || (Kitos = {}));
//# sourceMappingURL=report-viewer.controller.js.map
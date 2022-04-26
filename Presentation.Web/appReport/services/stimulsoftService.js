var Kitos;
(function (Kitos) {
    var Services;
    (function (Services) {
        var StimulsoftService = /** @class */ (function () {
            function StimulsoftService($window) {
                this.$window = $window;
            }
            StimulsoftService.prototype.getViewerOptions = function () {
                return new this.$window.Stimulsoft.Viewer.StiViewerOptions();
            };
            StimulsoftService.prototype.getViewer = function (options, name) {
                return new this.$window.Stimulsoft.Viewer.StiViewer(options, name, false);
            };
            StimulsoftService.prototype.getReport = function () {
                return new this.$window.Stimulsoft.Report.StiReport();
            };
            StimulsoftService.prototype.getVariable = function () {
                return new this.$window.Stimulsoft.Report.Dictionary.StiVariable();
            };
            StimulsoftService.prototype.getODataDatabase = function () {
                return new this.$window.Stimulsoft.Report.Dictionary.StiODataDatabase();
            };
            StimulsoftService.prototype.getDesignerOptions = function () {
                return new this.$window.Stimulsoft.Designer.StiDesignerOptions();
            };
            StimulsoftService.prototype.getDesigner = function (options, name) {
                return new this.$window.Stimulsoft.Designer.StiDesigner(options, name, false);
            };
            StimulsoftService.prototype.setLocalizationFile = function (path) {
                this.$window.Stimulsoft.Base.Localization.StiLocalization.setLocalizationFile(path);
            };
            StimulsoftService.$inject = ["$window"];
            return StimulsoftService;
        }());
        Services.StimulsoftService = StimulsoftService;
        angular.module("reportApp").service("stimulsoftService", Kitos.Services.StimulsoftService);
    })(Services = Kitos.Services || (Kitos.Services = {}));
})(Kitos || (Kitos = {}));
//# sourceMappingURL=stimulsoftService.js.map
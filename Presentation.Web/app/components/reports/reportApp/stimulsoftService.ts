module Kitos.Services {
    interface IStimulsoftService {
        getOptions(): any;
        getViewer(options: any, name: string): any;
        getReport(): any;
        getDesigner(options: any, name: string): any;
        getDesignerOptions(): any;
    }

    export class StimulsoftService implements IStimulsoftService {

        public static $inject = ["$window"];

        constructor(private $window) {
        }

        public getOptions(): any {
            return new this.$window.Stimulsoft.Viewer.StiViewerOptions();
        }


        public getViewer(options, name: string): any {
            return new this.$window.Stimulsoft.Viewer.StiViewer(options, name, false);
        }

        public getReport(): any {
            return new this.$window.Stimulsoft.Report.StiReport();
        }

        public getDesignerOptions(): any {
            return new this.$window.Stimulsoft.Designer.StiDesignerOptions();
        }

        public getDesigner(options, name: string): any {
            return new this.$window.Stimulsoft.Designer.StiDesigner(options, name, false);
        }
    }

}
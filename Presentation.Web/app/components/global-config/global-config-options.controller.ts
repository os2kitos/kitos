module Kitos.GlobalConfig.Options {
    "use strict";

    export class OptionsController {
        public title: string;
        public isActive: boolean;
        //Op/ned
        public nr: number;
        public name: string;
        public hasWriteAccess: boolean;
        public description: string;
        public action: string;


        public static $inject: string[] = ["$uibModalInstance", "$stateParams", "$http", "notify", "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $stateParams: ng.ui.IStateParamsService,
            private $http: ng.IHttpService,
            private notify,
            private reportService: Services.ReportService,
            private _: ILoDashWithMixins) {

            this.actionSelector($stateParams["action"]);
        }

        private actionSelector = (action: string) => {
            switch (action) {
                case "create":
                    this.title = "Opret";
                    break;
                case "edit":
                    this.title = "Redigér";
                    break;
            }
        }

        public cancel() {
            this.$uibModalInstance.close();
        }
    }
}
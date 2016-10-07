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
        public optionId: number;


        public static $inject: string[] = ["$uibModalInstance", "$stateParams", "$http", "notify", "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $stateParams: ng.ui.IStateParamsService,
            private $http: ng.IHttpService,
            private notify,
            private reportService: Services.ReportService,
            private _: ILoDashWithMixins,
            private baseUrl: string) {

            this.baseUrl = "/odata/options";
            this.initModal($stateParams["optionId"]);
        }

        //TODO Not done!
        private initModal = (optionId: number) => {
            this.optionId = optionId;

            if (optionId === 0) {
                this.title = "Opret";
            } else {
                this.title = "Redigér";
            }

            if (optionId === 1) {
                let option = this.$http.get<Models.IOptionEntity>(`${this.baseUrl}(${optionId})`);

                option.then((result) => {
                    let opt = result.data;
                    this.name = opt.Name;
                    this.description = opt.Note;
                });
            }

        }

        //TODO Not done!
        public ok() {
            if (this.optionId === 0) {
                let payload = {
                    Name: this.name,
                    Note: this.description
                }
                this.$http.post(`${this.baseUrl}`, payload).then((response) => {
                    this.$uibModalInstance.close();
                    this.notify.addSuccessMessage("Oprettet.");
                }).catch((response) => {
                    this.notify.addErrorMessage("Oprettelse mislykkedes.");
                });
            }
        }

        public cancel() {
            this.$uibModalInstance.close();
        }
    }
}
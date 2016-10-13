module Kitos.GlobalConfig.Options {
    "use strict";

    export class OptionsController {
        public pageTitle: string;
        public isObligatory: boolean;
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
        initModal = (optionId: number) => {
            this.optionId = optionId;

            if (optionId === 0) {
                this.pageTitle = "Opret";
            } else {
                this.pageTitle = "Redigér";
            }

            if (optionId === 1) {
                let option = this.$http.get<Models.IOptionEntity>(`${this.baseUrl}(${optionId})`);

                option.then((result) => {
                    let opt = result.data;
                    this.name = opt.Name;
                    this.description = opt.Note;
                });
            }

        };

        //TODO Not done!
        public ok() {
            if (this.optionId === 0) {
                let payload = {
                    Name: this.name,
                    Note: this.description
                };
                this.$http.post(`${this.baseUrl}`, payload).then((response) => {
                    this.$uibModalInstance.close();
                    this.notify.addSuccessMessage("Oprettet.");
                }).catch((response) => {
                    this.notify.addErrorMessage("Oprettelse mislykkedes.");
                });
            }
        };

        public cancel() {
            this.$uibModalInstance.close();
        };
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("config.org.edit-roles", {
                url: "/{id:int}/edit",
                onEnter: [
                    "$state", "$stateParams", "$uibModal",
                    ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/global-config/global-config-option-edit.modal.view.html",
                            // fade in instead of slide from top, fixes strange cursor placement in IE
                            // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                            windowClass: "modal fade in",
                            controller: OptionsController,
                            controllerAs: "vm"
                        }).result.then(() => {
                            // OK
                            // GOTO parent state and reload
                            $state.go("^", null, { reload: true });
                        },
                            () => {
                                // Cancel
                                // GOTO parent state
                                $state.go("^");
                            });
                    }
                ]
            });
        }]);
}

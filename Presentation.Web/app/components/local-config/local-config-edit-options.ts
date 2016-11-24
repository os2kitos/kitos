module Kitos.LocalAdmin.LocalOptions {
    "use strict";

    export class LocalOptionsController {
        public pageTitle: string;

        public optionName: string;
        public description: string;

        public static $inject: string[] = ["$uibModalInstance", "$stateParams", "$http", "notify", "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $stateParams: ng.ui.IStateParamsService,
            private $http: ng.IHttpService,
            private notify,
            private reportService: Services.ReportService,
            private _: ILoDashWithMixins,
            private optionsUrl: string,
            private optionId: number,
            private optionType: string) {

            this.optionsUrl = this.$stateParams["optionsUrl"];
            this.optionId = this.$stateParams["id"];
            this.optionType = this.$stateParams["optionType"];
            this.initModal(this.optionId, this.optionsUrl);
        }

        private initModal = (optionId: number, optionsUrl: string) => {

            this.pageTitle = "Redigér";

            const option = this.$http.get<Models.IOptionEntity>(`${optionsUrl}(${optionId})`);

            option.then((result) => {
                const opt = result.data;

                this.optionName = opt.Name;
                this.description = opt.Description;
            });
        };

        public ok() {
            const payload = {
                Description: this.description
            };
            this.$http.patch(`${this.optionsUrl}(${this.optionId})`, payload)
                .then((response) => {
                    this.$uibModalInstance.close();
                    this.notify.addSuccessMessage("Værdien blev redigeret.");
                }).catch((response) => {
                    this.notify.addErrorMessage("Værdien blev ikke redigeret.");
                });
        };

        public cancel() {
            this.$uibModalInstance.close();
        };

        public resetDescription() {
            this.$http.patch(`${this.optionsUrl}(${this.optionId})`, { Description: null });
            this.$uibModalInstance.close();
        };
    }

    angular.module("app").config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
        $stateProvider.state("local-config.current-org.edit-current-org-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-organisation-roles",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/local-config/local-config-option-edit.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: LocalOptionsController,
                        controllerAs: "vm"
                    })
                        .result.then(() => {
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
        }).state("local-config.project.edit-project-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-project-roles",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/local-config/local-config-option-edit.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: LocalOptionsController,
                        controllerAs: "vm"
                    })
                        .result.then(() => {
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
        }).state("local-config.system.edit-system-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-system-roles",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/local-config/local-config-option-edit.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: LocalOptionsController,
                        controllerAs: "vm"
                    })
                        .result.then(() => {
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
        }).state("local-config.contract.edit-contract-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-contract-roles",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/local-config/local-config-option-edit.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: LocalOptionsController,
                        controllerAs: "vm"
                    })
                        .result.then(() => {
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
        }).state("local-config.project.edit-project-types", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-project-types",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/local-config/local-config-option-edit.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: LocalOptionsController,
                        controllerAs: "vm"
                    })
                        .result.then(() => {
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
        }).state("local-config.system.edit-system-types", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-system-types",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/local-config/local-config-option-edit.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: LocalOptionsController,
                        controllerAs: "vm"
                    }).result.then(() => {
                        // OK
                        // GOTO parent state and reload
                        $state.go("^", null, { reload: true });
                    }, () => {
                        // Cancel
                        // GOTO parent state
                        $state.go("^");
                    });
                }
            ]
        }).state("local-config.contract.edit-contract-types", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-contract-types",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/local-config/local-config-option-edit.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: LocalOptionsController,
                        controllerAs: "vm"
                    }).result.then(() => {
                        // OK
                        // GOTO parent state and reload
                        $state.go("^", null, { reload: true });
                    }, () => {
                        // Cancel
                        // GOTO parent state
                        $state.go("^");
                    });
                }
            ]
        });
    }]);
}

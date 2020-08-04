module Kitos.LocalAdmin.LocalOptions {
    "use strict";

    export class LocalOptionsController {
        public pageTitle: string;

        public optionName: string;
        public description: string;

        private optionId: number;
        private localOptionService: Kitos.Services.LocalOptions.ILocalOptionService;

        public static $inject: string[] = ["$uibModalInstance", "$stateParams", "$http", "notify", "localOptionServiceFactory"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $stateParams: ng.ui.IStateParamsService,
            private $http: ng.IHttpService,
            private notify,
            localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory,
        ) {
            this.localOptionService = localOptionServiceFactory.create(this.$stateParams["optionType"]);
            this.optionId = this.$stateParams["id"];
            this.initModal(this.optionId);
        }

        private initModal = (optionId: number) => {

            this.pageTitle = "Redigér";

            const option = this.localOptionService.get(optionId);

            option.then((result) => {
                this.optionName = result.Name;
                this.description = result.Description;
            });
        };

        public ok() {
            const payload: Models.IEditedLocalOptionEntity = {
                Description: this.description
            };

            this.localOptionService.update(this.optionId, payload)
                .then((isSuccess) => {
                    if (isSuccess) {
                        this.$uibModalInstance.close();
                        this.notify.addSuccessMessage("Værdien blev redigeret.");
                    } else {
                        this.notify.addErrorMessage("Værdien blev ikke redigeret.");
                    }
                }
            );
        };

        public cancel() {
            this.$uibModalInstance.close();
        };

        public resetDescription() {
            const payload: Models.IEditedLocalOptionEntity = {
                Description: null
            };

            this.localOptionService.update(this.optionId, payload)
                .then((isSuccess) => {
                        if (isSuccess) {
                            this.$uibModalInstance.close();
                            this.notify.addSuccessMessage("Værdien blev redigeret.");
                        } else {
                            this.notify.addErrorMessage("Værdien blev ikke redigeret.");
                        }
                    }
                );
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
            url: "/{id:int}/{optionType:int}/edit-project-roles",
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

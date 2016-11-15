module Kitos.GlobalConfig.Options {
    "use strict";

    export class OptionsController {
        public pageTitle: string;

        public name: string;
        public isObligatory: boolean;
        public hasWriteAccess: boolean;
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

            if (optionId === 0) {
                this.pageTitle = "Opret";
            } else {
                this.pageTitle = "Redigér";

                const option = this.$http.get<Models.IOptionEntity>(`${optionsUrl}(${optionId})`);

                option.then((result) => {
                    const opt = result.data;

                    this.name = opt.Name;
                    this.isObligatory = opt.IsObligatory;
                    this.hasWriteAccess = opt.HasWriteAccess;
                    this.description = opt.Description;
                });
            }

        };

        private createPayload(type: string): Object | Object {
            if (type === "role") {
                return {
                    Name: this.name,
                    IsObligatory: this.isObligatory,
                    HasWriteAccess: this.hasWriteAccess,
                    Description: this.description
                }
            }
            return {
                Name: this.name,
                IsObligatory: this.isObligatory,
                Description: this.description
            }

        }

        public ok() {

            if (this.optionId === 0) {
                const payload = this.createPayload(this.optionType);
                this.$http.post(`${this.optionsUrl}`, payload)
                    .then((response) => {
                        this.$uibModalInstance.close();
                        this.notify.addSuccessMessage("Værdien blev oprettet.");
                    }).catch((response) => {
                        this.notify.addErrorMessage("Oprettelse mislykkedes.");
                    });
            } else {
                const payload = this.createPayload(this.optionType);
                this.$http.patch(`${this.optionsUrl}(${this.optionId})`, payload)
                    .then((response) => {
                        this.$uibModalInstance.close();
                        this.notify.addSuccessMessage("Værdien blev redigeret.");
                    }).catch((response) => {
                        this.notify.addErrorMessage("Værdien blev ikke redigeret.");
                    });
            }
        };

        public cancel() {
            this.$uibModalInstance.close();
        };
    }

    angular.module("app").config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
        $stateProvider.state("global-admin.org.edit-org-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-organisation-roles",
            onEnter: [
                "$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/global-admin/global-admin-option-edit-roles.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: OptionsController,
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
        }).state("global-admin.project.edit-project-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-project-roles",
            onEnter: [
                "$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/global-admin/global-admin-option-edit-roles.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: OptionsController,
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
        }).state("global-admin.system.edit-system-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-system-roles",
            onEnter: [
                "$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/global-admin/global-admin-option-edit-roles.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: OptionsController,
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
        }).state("global-admin.contract.edit-contract-roles", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-contract-roles",
            onEnter: [
                "$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/global-admin/global-admin-option-edit-roles.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: OptionsController,
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
        }).state("global-admin.project.edit-project-types", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-project-types",
            onEnter: [
                "$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/global-admin/global-admin-option-edit-types.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: OptionsController,
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
        }).state("global-admin.system.edit-system-types", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-system-types",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/global-admin/global-admin-option-edit-types.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: OptionsController,
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
        }).state("global-admin.contract.edit-contract-types", {
            url: "/{:optionsUrl}/{id:int}/{:optionType}/edit-contract-types",
            onEnter: ["$state", "$stateParams", "$uibModal",
                ($state: ng.ui.IStateService,
                    $stateParams: ng.ui.IStateParamsService,
                    $uibModal: ng.ui.bootstrap.IModalService) => {
                    $uibModal.open({
                        templateUrl: "app/components/global-admin/global-admin-option-edit-types.modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        controller: OptionsController,
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

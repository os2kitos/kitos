module Kitos.LocalAdmin.LocalOptions {
    "use strict";

    export class LocalOptionsController {
        pageTitle: string;

        optionName: string;
        description: string;

        private readonly optionId: number;
        private readonly localOptionService: Kitos.Services.LocalOptions.ILocalOptionService;

        static $inject: string[] = ["$uibModalInstance", "$stateParams", "notify", "localOptionServiceFactory"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly $stateParams: ng.ui.IStateParamsService,
            private readonly notify,
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

        ok() {
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

        cancel() {
            this.$uibModalInstance.close();
        };

        resetDescription() {
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

        const states = [
            {
                id: "local-config.current-org.edit-current-org-roles",
                urlSuffix: "edit-organisation-roles"
            },
            {
                id: "local-config.project.edit-project-roles",
                urlSuffix: "edit-project-roles"
            },
            {
                id: "local-config.system.edit-system-roles",
                urlSuffix: "edit-system-roles"
            },
            {
                id: "local-config.contract.edit-contract-roles",
                urlSuffix: "edit-contract-roles"
            },
            {
                id: "local-config.project.edit-project-types",
                urlSuffix: "edit-project-types"
            },
            {
                id: "local-config.system.edit-system-types",
                urlSuffix: "edit-system-types"
            },
            {
                id: "local-config.contract.edit-contract-types",
                urlSuffix: "edit-contract-types"
            },
            {
                id: "local-config.data-processing.edit-data-processing-agreement-roles",
                urlSuffix: "edit-contract-types"
            }
        ];

        states.forEach(currentStateConfig =>
            $stateProvider.state(currentStateConfig.id, {
                url: `/{id:int}/{optionType:int}/${currentStateConfig.urlSuffix}`,
                onEnter: ["$state", "$uibModal",
                    ($state: ng.ui.IStateService,
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
            }));

    }]);
}

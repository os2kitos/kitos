module Kitos.GlobalConfig.Options {
    "use strict";

    export class OptionsController {
        public pageTitle: string;

        public name: string;
        public isObligatory: boolean;
        public hasWriteAccess: boolean;
        public description: string;
        public buttonDisabled: boolean;

        public static $inject: string[] = ["$uibModalInstance", "$stateParams", "$http", "notify", "user"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $stateParams: ng.ui.IStateParamsService,
            private $http: ng.IHttpService,
            private notify,
            private user: Services.IUser,
            private optionsUrl: string,
            private optionId: number,
            private optionType: string) {

            this.optionsUrl = this.$stateParams["optionsUrl"];
            this.optionId = this.$stateParams["id"];
            this.optionType = this.$stateParams["optionType"];
            this.initModal(this.optionId, this.optionsUrl);
            this.buttonDisabled = false;
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
            this.buttonDisabled = true;
            if (this.optionId === 0) {
                const payload = this.createPayload(this.optionType);
                this.$http.post(`${this.optionsUrl}?organizationId=${this.user.currentOrganizationId}`, payload)
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

        const states = [
            {
                id: "global-admin.org.edit-org-roles",
                urlSuffix: "edit-organisation-roles",
                isRole: true
            },
            {
                id: "global-admin.project.edit-project-roles",
                urlSuffix: "edit-project-roles",
                isRole: true
            },
            {
                id: "global-admin.system.edit-system-roles",
                urlSuffix: "edit-system-roles",
                isRole: true
            },
            {
                id: "global-admin.contract.edit-contract-roles",
                urlSuffix: "edit-contract-roles",
                isRole: true
            },
            {
                id: "global-admin.project.edit-project-types",
                urlSuffix: "edit-project-types",
                isRole: false
            },
            {
                id: "global-admin.system.edit-system-types",
                urlSuffix: "edit-system-types",
                isRole: false
            },
            {
                id: "global-admin.contract.edit-contract-types",
                urlSuffix: "edit-contract-types",
                isRole: false
            },
            {
                id: "global-admin.report.edit-report-types",
                urlSuffix: "edit-report-types",
                isRole: false
            }
        ];

        states.forEach(currentStateConfig =>
            $stateProvider.state(currentStateConfig.id,
                {
                    url: `/{:optionsUrl}/{id:int}/{:optionType}/${currentStateConfig.urlSuffix}`,
                    onEnter: [
                        "$state", "$stateParams", "$uibModal",
                        ($state: ng.ui.IStateService,
                            $stateParams: ng.ui.IStateParamsService,
                            $uibModal: ng.ui.bootstrap.IModalService) => {
                            $uibModal.open({
                                    templateUrl: currentStateConfig.isRole
                                        ? "app/components/global-admin/global-admin-option-edit-roles.modal.view.html"
                                        : "app/components/global-admin/global-admin-option-edit-types.modal.view.html",
                                    // fade in instead of slide from top, fixes strange cursor placement in IE
                                    // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                                    windowClass: "modal fade in",
                                    controller: OptionsController,
                                    controllerAs: "vm",
                                    resolve: {
                                        user: [
                                            "userService", userService => userService.getUser()
                                        ]
                                    }
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

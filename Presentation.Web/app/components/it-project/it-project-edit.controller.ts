module Kitos.ItProject.Edit {
    "use strict";

    export interface IEditController {
        allowClearOption: IAllowClearOption;
        autosaveUrl: string;
        dropdownData: Array<IDropdownOption>;
        hasWriteAccess: boolean;
        parentSelectOptions: any;
        project: any;
        projectTypes: Array<any>;
        selectedData: Array<IDropdownOption>;
        selectSettings: ISelectSettings;
        selectTranslation: ISelectTranslation;
    }

    class EditController implements IEditController {
        public allowClearOption: IAllowClearOption;
        public autosaveUrl: string;
        public dropdownData: Array<IDropdownOption>;
        public parentSelectOptions;
        public selectedData: Array<IDropdownOption>;
        public selectTranslation: ISelectTranslation;
        public selectSettings: ISelectSettings;

        public static $inject: Array<string> = [
            "$scope",
            "$http",
            "_",
            "project",
            "projectTypes",
            "user",
            "hasWriteAccess",
            "autofocus"
        ];

        constructor(
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private _: ILoDashWithMixins,
            public project,
            public projectTypes,
            private user,
            public hasWriteAccess,
            private autofocus) {
            autofocus();

            if (this.project.parentId) {
                this.project.parent = {
                    id: this.project.parentId,
                    text: this.project.parentName
                };
            }

            this.allowClearOption = {
                allowClear: true
            };

            this.selectSettings = {
                dynamicTitle: false,
                buttonClasses: "btn btn-default btn-sm"
            };

            this.selectTranslation = {
                checkAll: "Vis alle",
                uncheckAll: "Skjul alle",
                buttonDefaultText: "Faner "
            };

            this.selectedData = [];

            if (this.project.isStatusGoalVisible) {
                this.selectedData.push({ id: 1 });
            }
            if (this.project.isStrategyVisible) {
                this.selectedData.push({ id: 2 });
            }
            if (this.project.isHierarchyVisible) {
                this.selectedData.push({ id: 3 });
            }
            if (this.project.isEconomyVisible) {
                this.selectedData.push({ id: 4 });
            }
            if (this.project.isStakeholderVisible) {
                this.selectedData.push({ id: 5 });
            }
            if (this.project.isRiskVisible) {
                this.selectedData.push({ id: 6 });
            }
            if (this.project.isCommunicationVisible) {
                this.selectedData.push({ id: 7 });
            }
            if (this.project.isHandoverVisible) {
                this.selectedData.push({ id: 8 });
            }

            this.dropdownData = [
                { id: 1, label: "Vis Status: Mål" },
                { id: 2, label: "Vis Strategi" },
                { id: 3, label: "Vis Hierarki" },
                { id: 4, label: "Vis Økonomi" },
                { id: 5, label: "Vis Interessenter" },
                { id: 6, label: "Vis Risiko" },
                { id: 7, label: "Vis Kommunikation" },
                { id: 8, label: "Vis Overlevering" }
            ];

            this.autosaveUrl = `api/itproject/${this.project.id}`;

            this.parentSelectOptions = this.selectLazyLoading("api/itproject", true, ["overview=true", `orgId=${this.user.currentOrganizationId}`]);

            this.setupSelectedDataWatch();
        }

        private setupSelectedDataWatch(): void {
            // todo refactor this garbage!
            this.$scope.$watch(() => this.selectedData, (newValue: Array<IDropdownOption>, oldValue: Array<IDropdownOption>) => {
                var payload: IPayload = {
                    isStatusGoalVisible: null,
                    isStrategyVisible: null,
                    isHierarchyVisible: null,
                    isEconomyVisible: null,
                    isStakeholderVisible: null,
                    isRiskVisible: null,
                    isCommunicationVisible: null,
                    isHandoverVisible: null
                };

                if (newValue.length > oldValue.length) {
                    // something was added
                    var addIds = this._.difference(this._.map(newValue, "id"), this._.map(oldValue, "id"));
                    this._.each(addIds, (id: number) => {
                        switch (id) {
                        case 1:
                            payload.isStatusGoalVisible = true;
                            break;
                        case 2:
                            payload.isStrategyVisible = true;
                            break;
                        case 3:
                            payload.isHierarchyVisible = true;
                            break;
                        case 4:
                            payload.isEconomyVisible = true;
                            break;
                        case 5:
                            payload.isStakeholderVisible = true;
                            break;
                        case 6:
                            payload.isRiskVisible = true;
                            break;
                        case 7:
                            payload.isCommunicationVisible = true;
                            break;
                        case 8:
                            payload.isHandoverVisible = true;
                            break;
                        }
                    });
                } else if (newValue.length < oldValue.length) {
                    // something was removed
                    var removedIds = _.difference(this._.map(oldValue, "id"), this._.map(newValue, "id"));
                    this._.each(removedIds, id => {
                        switch (id) {
                        case 1:
                            payload.isStatusGoalVisible = false;
                            break;
                        case 2:
                            payload.isStrategyVisible = false;
                            break;
                        case 3:
                            payload.isHierarchyVisible = false;
                            break;
                        case 4:
                            payload.isEconomyVisible = false;
                            break;
                        case 5:
                            payload.isStakeholderVisible = false;
                            break;
                        case 6:
                            payload.isRiskVisible = false;
                            break;
                        case 7:
                            payload.isCommunicationVisible = false;
                            break;
                        case 8:
                            payload.isHandoverVisible = false;
                            break;
                        }
                    });
                }
                if (this._.size(<any>payload) > 0) {
                    this.$http({ method: "PATCH", url: this.autosaveUrl + "?organizationId=" + this.user.currentOrganizationId, data: payload })
                        .then((result: ng.IHttpPromiseCallbackArg<IApiResponse<IPayload>>) => {
                            var data = result.data.response;
                            this.project.isStatusGoalVisible = data.isStatusGoalVisible;
                            this.project.isStrategyVisible = data.isStrategyVisible;
                            this.project.isHierarchyVisible = data.isHierarchyVisible;
                            this.project.isEconomyVisible = data.isEconomyVisible;
                            this.project.isStakeholderVisible = data.isStakeholderVisible;
                            this.project.isRiskVisible = data.isRiskVisible;
                            this.project.isCommunicationVisible = data.isCommunicationVisible;
                            this.project.isHandoverVisible = data.isHandoverVisible;
                        });
                }
            }, true);
        }

        private selectLazyLoading(url: string, excludeSelf: boolean, paramAry: Array<string>) {
            return {
                minimumInputLength: 1,
                allowClear: true,
                placeholder: " ",
                initSelection: () => {
                },
                ajax: {
                    data: (term) => {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport: (queryParams) => {
                        var extraParams = paramAry ? "&" + paramAry.join("&") : "";
                        var res = this.$http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success);
                        // res.abort = () => null;

                        return res;
                    },

                    results: (data: {data: IApiResponse<any> }) => {
                        var results = [];

                        _.each(data.data.response, (obj: { id; name; cvr; }) => {
                            if (excludeSelf && obj.id == this.project.id)
                                return; // don't add self to result

                            results.push({
                                id: obj.id,
                                text: obj.name ? obj.name : "Unavngiven",
                                cvr: obj.cvr
                            });
                        });

                        return { results: results };
                    }
                }
            };
        }
    }

    angular
        .module("app")
        .controller("project.EditCtrl", EditController)
        .config([
            "$stateProvider", $stateProvider => {
                $stateProvider.state("it-project.edit", {
                    url: "/edit/{id:[0-9]+}",
                    templateUrl: "app/components/it-project/it-project-edit.view.html",
                    controller: EditController,
                    controllerAs: "projectEditVm",
                    resolve: {
                        project: [
                            "$http", "$stateParams", ($http: ng.IHttpService, $stateParams) => {
                                return $http.get("api/itproject/" + $stateParams.id)
                                    .then((result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => result.data.response);
                            }
                        ],
                        projectTypes: [
                            "$http", $http => {
                                return $http.get("api/itprojecttype/")
                                    .then((result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => result.data.response);
                            }
                        ],
                        user: [
                            "userService", userService => userService.getUser()
                        ],
                        hasWriteAccess: [
                            "$http", "$stateParams", "user", ($http, $stateParams, user) => {
                                return $http.get("api/itproject/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                                    .then((result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => result.data.response);
                            }
                        ]
                    }
                });
            }
        ]);
}

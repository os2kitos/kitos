module Kitos.GlobalAdmin.HelpTexts {
    "use strict"

    interface ICreateViewModel {
        title: string;
        key: string;
    }

    class CreateHelpTextController {
        public busy: boolean;
        public vm: ICreateViewModel;

        public static $inject: string[] = ["$uibModalInstance", "$http", "$q", "notify", "autofocus", "user", "_", "helpTexts"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private notify,
            private autofocus,
            private user: Kitos.Services.IUser,
            private _: _.LoDashStatic,
            private helpTexts) {
            if (!user.currentOrganizationId) {
                notify.addErrorMessage("Fejl! Kunne ikke oprette hjælpetekst.", true);
                return;
            }
                
            autofocus();
            this.busy = false;
        }

        public cancel() {
            this.$uibModalInstance.close();
        }

        public create() {
            //Check duplicates
            var key = this.vm.key;
            if (_.find(this.helpTexts, function (o: Models.IHelpText) { return o.Key === key; })) {
                this.notify.addErrorMessage("Fejl! Der findes allerede en hjælpetekst med den angivne key.", true);
                return;
            }

            this.busy = true;
            var payload = {
                Title: this.vm.title,
                Key: this.vm.key
            };

            var msg = this.notify.addInfoMessage("Opretter hjælpetekst", false);

            this.$http.post(`odata/HelpTexts?organizationId=${this.user.currentOrganizationId}`, payload, { handleBusy: true })
                .then((response) => {
                    msg.toSuccessMessage(`${this.vm.title} er oprettet i KITOS`);
                    this.cancel();
                }, () => {
                    msg.toErrorMessage(`Fejl! Noget gik galt ved oprettelsen af ${this.vm.title}!`);
                    this.cancel();
                });
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("global-admin.help-texts.create", {
                url: "/create",
                onEnter: [
                    "$state", "$stateParams", "$uibModal",
                    ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/global-admin/global-admin-help-texts-create.modal.view.html",
                            // fade in instead of slide from top, fixes strange cursor placement in IE
                            // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                            windowClass: "modal fade in",
                            controller: CreateHelpTextController,
                            controllerAs: "ctrl",
                            resolve: {
                                user: ["userService", userService => userService.getUser()],
                                helpTexts: [
                                    "$http", $http => $http.get("/odata/HelpTexts").then(result => result.data.value)
                                ]
                            }
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

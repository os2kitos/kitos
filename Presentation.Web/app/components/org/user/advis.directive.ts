module Kitos.Organization.Users {
    "use strict";

    function setupDirective(): ng.IDirective {
        return {
            scope: {},
            controller: AdvisButtonDirective,
            controllerAs: "ctrl",
            bindToController: {
                user: "=",
                currentOrganizationId: "@",
                ngDisabled: "="
            },
            template: `<button type="button" data-ng-if="ctrl.user.LastAdvisDate === null" class="btn btn-success btn-xs" style="width: 90%" data-ng-click="ctrl.sendAdvis(ctrl.user, false)" data-ng-disabled="ctrl.ngDisabled">
                    Send Advis
                </button>
                <button type="button" data-ng-if="ctrl.user.LastAdvisDate !== null" class="btn btn-warning btn-xs" style="width: 90%" data-ng-click="ctrl.sendAdvis(ctrl.user, true)" data-ng-disabled="ctrl.ngDisabled">
                    {{ ctrl.user.LastAdvisDate | date:'dd-MM-yy' }}
                </button>`
        };
    }

    interface IDirectiveScope {
        user;
        currentOrganizationId: number;
        ngDisabled: boolean;
    }

    class AdvisButtonDirective implements IDirectiveScope {
        public user;
        public currentOrganizationId: number;
        public ngDisabled: boolean;

        public static $inject: string[] = ["$http", "notify"];

        constructor(private $http: IHttpServiceWithCustomConfig, private notify) {
        }

        public sendAdvis(userToAdvis, reminder) {
            var params: { sendReminder; sendAdvis; organizationId; } = { sendReminder: null, sendAdvis: null, organizationId: null };
            var type;

            if (reminder) {
                params.sendReminder = true;
                type = "påmindelse";
            } else {
                params.sendAdvis = true;
                type = "advis";
            }
            params.organizationId = this.currentOrganizationId;

            var msg = this.notify.addInfoMessage("Sender " + type + " til " + userToAdvis.email, false);
            this.$http.post<Kitos.API.Models.IApiWrapper<any>>("api/user", userToAdvis, { handleBusy: true, params: params })
                .then((result) => {
                    userToAdvis.LastAdvisDate = result.data.response.lastAdvisDate;
                    msg.toSuccessMessage("Advis sendt til " + userToAdvis.Email);
                }, (error) => {
                    msg.toErrorMessage("Kunne ikke sende " + type + "!");
                });
        }
    }

    angular.module("app")
        .directive("advisButton", setupDirective);
}
